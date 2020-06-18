import copy
import itertools
import codecs
from framework.pipeline import (PipelineStep, PipelineContext)
from framework.manifest import (Manifest, DocumentDescriptor, DocumentMetrics)
from framework.schema import SchemaManager, SchemaType
from framework.crypto import DecryptingReader, EncryptionData
from framework.pipeline.PipelineTokenMapper import PipelineTokenMapper
from framework.uri import native_path, pyspark_path
from framework.util import *
from dataclasses import dataclass, field, fields

from pyspark.sql.types import *

from pyspark.sql import functions as f
from pyspark.sql.functions import lit

from .DataQualityStepBase import *


class ValidateCSVStep(DataQualityStepBase):
    def __init__(self, config: dict, rejected_manifest_type: str='rejected', **kwargs):
        super().__init__(rejected_manifest_type)
        self.config : dict = config

    def exec(self, context: PipelineContext):
        """ Read in CSV into a dataframe, export the bad rows to a file and keep the good rows in the dataframe"""
        super().exec(context)
        
        data_category = self.document.DataCategory
        s_uri, r_uri, t_uri = self.get_uris(self.document.Uri)
        print(f'\n\ts_uri={s_uri}\n\tr_uri={r_uri}\n\tt_uri={t_uri}')

        t_uri = native_path(t_uri)
        self.ensure_output_dir(t_uri, is_dir=True)
        self.add_cleanup_location('purge', t_uri)
        self.logger.debug(f'Added purge location ({t_uri},None) to context')
        
        try:
            # INITIALIZE THE DOCUMENT DESCRIPTOR
            self.document.Metrics = DocumentMetrics()

            tempDir = tempfile.TemporaryDirectory(dir=t_uri)
            work_document = self._create_work_doc(tempDir.name, self.document)

            # RAW I/O LOGIC
            success1, errors1 = self.validate_header(work_document)
            
            # SPARK SESSION LOGIC
            session = self.get_sesssion(self.config)

            success2, errors2 = self.validate_min_rows(session, work_document)

            if not (success1 and success2):
                self.fail_file(session, s_uri, r_uri, errors1 + errors2)
                self.Success = False
                return

            schema_found, schema = SchemaManager().get(data_category, SchemaType.weak_error, 'spark')
        
            df = (session.read.format("csv")
                    .options(sep=",", header="true", mode="PERMISSIVE")
                    .schema(schema)
                    .option("columnNameOfCorruptRecord","_error")
                    .load(work_document.Uri))

            df_badrows = df.filter('_error is not NULL').drop(*['_error']).withColumn('_errors', lit("Malformed CSV row"))

            self.document.Metrics.sourceRows = self.get_row_metrics(session, df)  
            self.document.Metrics.rejectedCSVRows = self.get_row_metrics(session, df_badrows)

            #df = (session.read.format("csv")
            #        .options(sep=",", header="true", mode="PERMISSIVE")
            #        .schema(schema)
            #        .option("columnNameOfCorruptRecord","_error")
            #        .load(reader))

            #df = (session.read 
            #   .options(sep=",", header="true", mode="PERMISSIVE") 
            #   .schema(schema) 
            #   .option("columnNameOfCorruptRecord","_error")
            #   .csv(reader))
            # session.read.format?
            # temp unencrypted file?

                #self.document.Metrics.sourceRows = self.get_row_metrics(session, df)

            # add row index
            # df = df.withColumn('row', f.monotonically_increasing_id())

            #####################

            self.push_dataframe(df_badrows, f'spark.dataframe.{data_category}')   # share dataframe of badrows with subsequent steps

            self.document.Metrics.quality = 1
            self.emit_document_metrics()


        except Exception as e:
            print(e)
            self._journal(str(e))
            self._journal(f'Failed to validate csv file {s_uri}')
            self.SetSuccess(False, e) 

        self.Result = True  # is this needed?



    
# VALIDATION RULES
#region 
    def validate_header(self, document: DocumentDescriptor):
        """
        Rule CSV.1 - number of header columns match number of schema columns  
        Rule CSV.2 - head column names hatch schema column names (ordered)  
        """
        data_category = self.document.DataCategory
        schema_found, expectedSchema = SchemaManager().get(data_category, SchemaType.weak, 'cerberus')
        if not schema_found:
            raise ValueError(f'Failed to find schema: {data_category}:{SchemaType.weak.name}:cerberus')

        # only get the first n lines so we don't scan the entire file
        # we really only need the first one line (header row)
        #rdd_txt = spark.read.text(uri).limit(settings.header_check_row_count)
        #sourceHeaders = rdd_txt.head(1)[0].value.replace('"','').split(',')
        sourceHeaders = self.get_header(document)        

        #rdd_txt =
        #spark.read.text(uri).limit(settings.header_check_row_count).rdd.flatMap(lambda
        #x:x)
        #self.logger.info(f'Read first {settings.header_check_row_count} lines
        #from {uri} resulting in {rdd_txt.count()} rows')

        #df_headersegment = (spark
        #                  .read
        #                  .options(inferSchema="true", header="true")
        #                  .csv(rdd_txt))
        #sourceHeaders = df_headersegment.columns
        schemaHeaders = expectedSchema.keys()  # the expectedSchema is an OrderedDict so the column ordering is preserved

        isvalid, errors = self._validate_header_list(sourceHeaders, schemaHeaders)

        return isvalid, errors

    def get_header(self, document):
        """
        Get the first line of the file as the header

        :param document: DocumentDescriptor for the target document
        """
        byte_buffer = b''
        with open(native_path(document.Uri), 'rb') as file:
            byte_buffer = file.read(4 * 1024)

        # get the original text encoding
        enc = self.detect_by_bom(byte_buffer, 'utf-8')
        # convert the bytes to a string
        line = byte_buffer.decode(enc)
        # find the end of the line
        eol_pos = line.find('\n')
        if eol_pos >= 0:
            eol_pos = eol_pos + 1

        header = line[:eol_pos].rstrip('\n').rstrip('\r')

        return header.replace('"','').split(',')

    def _validate_header_list(self, header_columns: list, schema_columns: list):
        errors = []

        self.logger.debug("SOURCE COLUMNS")
        self.logger.debug(header_columns)
        self.logger.debug("SCHEMA COLUMNS")
        self.logger.debug(schema_columns)

        # CSV.1 - name code TBD
        headerColumnCount = len(header_columns)
        schemaColumnCount = len(schema_columns)
        if headerColumnCount != schemaColumnCount:
            errors.append(f"RULE CSV.1 - Column Count: {headerColumnCount} {schemaColumnCount}")

        # CSV.2 - name code TBD Column Order, Column Name
        for pair in itertools.zip_longest(header_columns, schema_columns):
            if not are_equal(pair[0], pair[1], self.quality_settings.csv.strict):
                errors.append(f"RULE CSV.2 - Header column mismatch {pair[0]}:{pair[1]}")

        return len(errors) == 0, errors

    def validate_min_rows(self, spark: SparkSession, document: DocumentDescriptor):
        """
        Rule CSV.3 - number of minimum data rows (+1 for header)
        """
        # if min_data_rows == 0, then skip the check
        if self.quality_settings.csv.min_data_rows <= 0:
            return True, []

        # only get the first n lines so we don't scan the entire file
        # we really only need the first two lines (header + a data row)
        with open(document.Uri, 'rb') as csv_file:
            rdd_txt = spark.read.text(csv_file).limit(self.quality_settings.csv.min_data_rows + 1).rdd.flatMap(lambda x:x)

        totalRows = rdd_txt.count()
        self.logger.info(f'Read first {self.quality_settings.csv.min_data_rows} lines from {document.Uri} resulting in {totalRows} rows')
        
        errors = []
        if totalRows < self.quality_settings.csv.min_data_rows:
            errors.append(f"RULE CSV.3 - Minimum row count: {totalRows}:{self.quality_settings.csv.min_data_rows}")

        return len(errors) == 0, errors
#endregion

    def detect_by_bom(self, raw: bytes, default):
        """
        Detect the encoding of the file by reading the Byte Order Mark, if present
        """
        preamble = raw[:4]
        for enc,boms in \
                ('utf-8-sig',   (codecs.BOM_UTF8,)),\
                ('utf-16',      (codecs.BOM_UTF16_LE,codecs.BOM_UTF16_BE)),\
                ('utf-32',      (codecs.BOM_UTF32_LE,codecs.BOM_UTF32_BE)):
            if any(preamble.startswith(bom) for bom in boms): return enc
        return default


    def get_uris(self, source_uri: str):
        """
        Get the source and dest uris.
            We assume that the source_uri has already been mapped into our context based on the options provided to the runtime
            We must generate the dest_uri using the same convention as the source uri.  
            When we publish the uri for the destination, we will map to to the external context when needed
        """
        tokens = FileSystemMapper.tokenize(source_uri)
        rejected_uri = self.get_rejected_uri(tokens)
        temp_uri = self.get_temp_uri(tokens)

        return source_uri, rejected_uri, temp_uri

    def fail_file(self, spark: SparkSession, s_uri, r_uri, errors):
        self.logger.error(f'Failing csv file {s_uri}')
        for line in errors:
            self.logger.error(f'\t{line}')

        pd_txt = spark.read.text(s_uri).toPandas()
        totalRows = len(pd_txt.index)
        del pd_txt  # release memory

        dest_uri = '/dbfs' + r_uri
        self.ensure_output_dir(dest_uri)
        copyfile('/dbfs' + s_uri, dest_uri)

        rejected_manifest = self.get_manifest('rejected')  # this will create the manifest if needed

        rejected_document = copy.deepcopy(self.document)

        rejected_document.Uri = r_uri
        rejected_document.Metrics = DocumentMetrics(quality = 0, rejectedCSVRows = totalRows, sourceRows = totalRows)
        rejected_document.AddErrors(errors)

        rejected_manifest.AddDocument(rejected_document)

        self.emit_document_metrics(rejected_document)





