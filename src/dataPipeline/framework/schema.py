import copy
from pyspark.sql.types import *
from enum import Enum, auto
from collections import OrderedDict
from datetime import datetime

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


class SchemaManager:
    _schemas = {
        # these are ordereddicts to preserve the order when converting to a list for spark
            'demographic': OrderedDict([
                                    ( 'LASO_CATEGORY',          {'type': 'string',  'required': False} ),
                                    ( 'ClientKey_id',           {'type': 'integer', 'required': True,       'coerce': int} ),
                                    ( 'BRANCH_ID',              {'type': 'string',  'required': True} ),
                                    ( 'CREDIT_SCORE',           {'type': 'integer', 'required': True,       'coerce': int} ),
                                    ( 'CREDIT_SCORE_SOURCE',    {'type': 'string',  'required': True} )
                                ]),
            'accounttransaction': OrderedDict([
                                    ('LASO_CATEGORY',           {'type': 'string',   'required': False}),
                                    ('AcctTranKey_id',          {'type': 'integer',  'required': True,      'coerce': int}),
                                    ('ACCTKey_id',              {'type': 'integer',  'required': True,      'coerce': int}),
                                    ('TRANSACTION_DATE',        {'type': 'datetime', 'required': True,      'coerce': to_date}),
                                    ('POST_DATE',               {'type': 'datetime', 'required': False,     'coerce': to_date}),
                                    ('TRANSACTION_CATEGORY',    {'type': 'string',   'required': True}),
                                    ('AMOUNT',                  {'type': 'float',    'required': True,      'coerce': float}),
                                    ('MEMO_FIELD',              {'type': 'string',   'required': False}),
                                    ('MCC_CODE',                {'type': 'string',   'required': False}),
                                    ('Balance_After_Transaction', {'type': 'float',  'required': False,     'coerce': float}),
                                ])
                }


    _TypeMap = {
        "string"    : StringType,
        "number"    : DoubleType,
        "float"     : DoubleType,
        "integer"   : IntegerType,
        "boolean"   : BooleanType,
        "object"    : StructType,
        "array"     : ArrayType,
        'datetime'  : TimestampType 
        }
    
    def get(self, name: str, schema_type: SchemaType, target: str):
        target = target.lower()
        if not (target in ['cerberus','spark']):
            raise ValueError(f'target must be one of "cerberus,spark"')

        raw_schema = self._schemas.get(name.lower(), None)
        
        if raw_schema is None:
            return False, None

        if schema_type.iserror():
            # copy the dict so we can add the error column
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
                                                StringType() if schema_type.isweak() else self._TypeMap.get(v['type'])(), 
                                                True) 
                                                for k,v in schema.items()
                                ])

    