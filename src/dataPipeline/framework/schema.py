import copy
from pyspark.sql.types import *
from enum import Enum, auto
from collections import OrderedDict
from datetime import datetime
from importlib import resources
import json

# coerscion functions
to_date = (lambda myDateTime:  datetime.strptime(myDateTime, '%Y-%m-%d %H:%M:%S'))  #TODO: should this go to isoformat?

class SchemaType(Enum):
    weak = auto(),
    strong = auto(),
    weak_error = auto(),
    strong_error = auto()

    def isweak(self):
        return self in [SchemaType.weak, SchemaType.weak_error]
    def isstrong(self):
        return self in [SchemaType.strong, SchemaType.strong_error]
    def iserror(self):
        return self in [SchemaType.weak_error, SchemaType.strong_error]


# TODO: Collapse down to just strong, mutate to weak on request
# TODO: when externalized use meta field to augment dictionary for _errflag_*
class SchemaManager:
    _schemas = {
        # these are ordereddicts to preserve the order when converting to a list for spark
            'demographic': OrderedDict([
                                    ( 'LASO_CATEGORY',          {'type': 'string'}                                  ),
                                    ( 'ClientKey_id',           {'type': 'integer', 'coerce': int, 'required': True} ),
                                    ( 'BRANCH_ID',              {'type': 'string', 'required': True}                    ),
                                    ( 'CREDIT_SCORE',           {'type': 'integer', 'coerce': int, 'required': False}),
                                    ( 'CREDIT_SCORE_SOURCE',    {'type': 'string', 'required': False}         )
                                ]),
            'demographic_boundary': OrderedDict([
                                    ( 'LASO_CATEGORY',          {'type': 'string'}                                  ),
                                    ( 'ClientKey_id',           {'type': 'integer', 'coerce': int, 'required': True} ),
                                    ( 'BRANCH_ID',              {'type': 'string', 'required': True}                    ),
                                    ( 'CREDIT_SCORE',           {'type': 'integer', 'coerce': int, 'required': False, 'min': 550,
                                                                  'meta': ('BDY.2', {'replace_value': 550})
                                                                }),
                                    ( 'CREDIT_SCORE_SOURCE',    {'type': 'string', 'required': False}         )
                                ]),
            'demographic_boundary_err': OrderedDict([
                                    ( 'LASO_CATEGORY',          {'type': 'string'}                                  ),
                                    ( 'ClientKey_id',           {'type': 'integer', 'coerce': int, 'required': True} ),
                                    ( 'BRANCH_ID',              {'type': 'string', 'required': True}                    ),
                                    ( 'CREDIT_SCORE',           {'type': 'integer', 'coerce': int, 'required': False, 'min': 550,
                                                                    'meta': ('BDY.2', {'replace_value': 550})
                                                                }),
                                    ( 'CREDIT_SCORE_SOURCE',    {'type': 'string', 'required': False}         ),
                                    ( '_errflag_CREDIT_SCORE',  {'type': 'integer'}                                  ) 
                                ]),
            'accounttransaction': OrderedDict([
                                    ('LASO_CATEGORY',           {'type': 'string'}),
                                    ('AcctTranKey_id',          {'type': 'integer',  'coerce': int}),
                                    ('ACCTKey_id',              {'type': 'integer',  'coerce': int}),
                                    ('TRANSACTION_DATE',        {'type': 'datetime', 'coerce': to_date}),
                                    ('POST_DATE',               {'type': 'datetime', 'coerce': to_date}),
                                    ('TRANSACTION_CATEGORY',    {'type': 'string'}),
                                    ('AMOUNT',                  {'type': 'float',    'coerce': float}),
                                    ('MEMO_FIELD',              {'type': 'string'}),
                                    ('MCC_CODE',                {'type': 'string', 'nullable': True})
                                ])
                }

    _TypeMap = {
        "string"    : StringType(),
        "number"    : DoubleType(),
        "float"     : DoubleType(),
        "integer"   : DecimalType(20,0),  #changed to LongType as the REDACTED number fields are larger than IntegerType.
        "boolean"   : BooleanType(),
        "object"    : StructType(),
        "array"     : ArrayType,
        'datetime'  :TimestampType() 
        }
    
    def get(self, name: str, schema_type: SchemaType, target: str, schemas: list):
        target = target.lower()
        if not (target in ['cerberus','spark']):
            raise ValueError(f'target must be one of "cerberus,spark"')

        DataCategoryConfig = [c for c in schemas if c["DataCategory"]== name]
        if not DataCategoryConfig:
            return False, None
        
        raw_schema = DataCategoryConfig[0]["Schema"]
        raw_schema2 = self._schemas.get(name.lower(), None)        
        #raw_schema = self._schemas.get(name.lower(), None)
        
        if raw_schema is None:
            return False, None

        if schema_type.iserror():
            # copy the dict so we can possible add the error column
            schema: dict = OrderedDict(raw_schema.items())
            schema["_error"] = {'type': 'string'}
        else:
            schema = raw_schema  # no additions to schema

        # cerberus takes a dict, so we are done
        if target.lower() == 'cerberus':
            return True, dict(schema)

        # reshape the dictionary into a list of StructFields for spark
        return True, StructType([
                                    StructField(k, 
                                                StringType() if schema_type.isweak() else self._TypeMap.get(v['type']), 
                                                True) 
                                                for k,v in schema.items()
                                ])

# recieve base schema and return cerberus' compatible dictionary
def create_OrderedDict(self, schema):

    classTypeMap = {
        "int": int,
        "string": str,
        "float" : float
    }

    functionMap = {
        "to_date": to_date #TODO: check if actual function here also works, e.g. (lambda myDateTime:  datetime.strptime(myDateTime, '%Y-%m-%d %H:%M:%S')) 
    }

    # decode python objects from schema, e.g. {"class":"int"} to <class 'int'>, {"function": "to_date"}
    #TODO: if newVal==None raise an error
    expandedSchema = schema        
    for col, spec in schema.items(): #For every column get its dictionary, e.g. {'type': 'string', 'required': True} given 'BRANCH_ID': {'type': 'string', 'required': True}
        for elem, val in spec.items(): #For every property within the dictonary get its value, e.g. 'string' given 'type': 'string'  
            if isinstance(val, dict):  #if dict then it is a python object                  
                #print(col,elem,val)
                newVal = classTypeMap.get(val.get("class",None))
                if newVal == None:
                    newVal = functionMap.get(val.get("function",None)) 
                expandedSchema[col][elem] = newVal

    #print("decodedSchema:\n",expandedSchema)
    return OrderedDict(expandedSchema)
       
# return base schema and ruleset for each category
def load_schemas(self, productId):
    with resources.open_text(self.host.hostconfigmodule, "products.json") as products_file:
        products = json.loads(products_file.read())
    with resources.open_text(self.host.hostconfigmodule, "schemas.json") as schemas_file:
        schemas = json.loads(schemas_file.read())

    product_config = [p for p in products.get("Products") if p.get("ProductId")==productId]


    product_schemas = []
    if product_config:        
        for c in product_config[0]["DataCategories"]:
            
            # get base schema for the data category
            schema_config = dict([s for s in schemas.get("Schemas") if s.get("SchemaId") == c.get("SchemaId")][0]) #TODO: exception in case index out of range, i.e. schemaId not present in schemas.json???
            baseSchema = schema_config.get("Schema")

            #TODO: Overwrite schema as per the DataCategory's configuration.
            #baseSchema = apply_overwrites(baseSchema, c.get("Overwrites"))
            #print("baseSchema:\n",baseSchema)
            self.logger.debug(f"\Schema for {c.get('CategoryName')} before decoding:\n{baseSchema}")

            # get cerberus' compatible dict
            schema = create_OrderedDict(self, baseSchema)
            self.logger.info(f"\Base schema for {c.get('CategoryName')}:\n{schema}")

            product_schemas.append({'DataCategory':c.get('CategoryName'), 'Schema':schema, 'RuleSet':schema_config.get("RuleSet")})
        
        return  product_schemas
    else:        
        return None