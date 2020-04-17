import copy
from pyspark.sql.types import *
from enum import Enum, auto
from collections import OrderedDict
from datetime import datetime

to_date = (lambda myDateTime:  datetime.strptime(myDateTime, '%Y-%m-%d %H:%M:%S'))

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
class SchemaManager:
    _schemas = {
            'demographic': {
                    SchemaType.strong : OrderedDict([
                                    ( 'LASO_CATEGORY',          {'type': 'string'}                                  ),
                                    ( 'ClientKey_id',           {'type': 'integer', 'coerce': int, 'required': True} ),
                                    ( 'BRANCH_ID',              {'type': 'string', 'required': True}                    ),
                                    ( 'CREDIT_SCORE',           {'type': 'integer', 'coerce': int, 'required': False}),
                                    ( 'CREDIT_SCORE_SOURCE',    {'type': 'string', 'required': False}         )
                                ]),                                                                          
                    SchemaType.weak : OrderedDict([
                                    ( 'LASO_CATEGORY',        {'type': 'string'} ),
                                    ( 'ClientKey_id',         {'type': 'string'} ),
                                    ( 'BRANCH_ID',            {'type': 'string'} ),
                                    ( 'CREDIT_SCORE',         {'type': 'string'} ),
                                    ( 'CREDIT_SCORE_SOURCE',  {'type': 'string'} )
                                ])
                },
            'accounttransaction': {
                    SchemaType.strong : OrderedDict([
                                    ('LASO_CATEGORY',           {'type': 'string'}),
                                    ('AcctTranKey_id',          {'type': 'integer',  'coerce': int}),
                                    ('ACCTKey_id',              {'type': 'integer',  'coerce': int}),
                                    ('TRANSACTION_DATE',        {'type': 'datetime', 'coerce': to_date}),
                                    ('POST_DATE',               {'type': 'datetime', 'coerce': to_date}),
                                    ('TRANSACTION_CATEGORY',    {'type': 'string'}),
                                    ('AMOUNT',                  {'type': 'float',    'coerce': float}),
                                    ('MEMO_FIELD',              {'type': 'string'}),
                                    ('MCC_CODE',                {'type': 'string'})
                                ]),
                    SchemaType.weak : OrderedDict([
                                    ('LASO_CATEGORY',           {'type': 'string'}),
                                    ('AcctTranKey_id',          {'type': 'string'}),
                                    ('ACCTKey_id',              {'type': 'string'}),
                                    ('TRANSACTION_DATE',        {'type': 'string'}),
                                    ('POST_DATE',               {'type': 'string'}),
                                    ('TRANSACTION_CATEGORY',    {'type': 'string'}),
                                    ('AMOUNT',                  {'type': 'string'}),
                                    ('MEMO_FIELD',              {'type': 'string'}),
                                    ('MCC_CODE',                {'type': 'string'})
                                ])
                }
        }    
    _TypeMap = {
        "string"    : StringType,
        "number"    : DoubleType,
        "float"     : DoubleType,
        "integer"   : IntegerType,
        "boolean"   : BooleanType,
        "object"    : StructType,
        "array"     : ArrayType,
        'datetime'  :TimestampType 
        }
    
    def get(self, name: str, schema_type: SchemaType, target: str):
        schema_dict = self._schemas.get(name.lower(), None)
        
        if schema_dict is None:
            return False, None

        schema: dict = copy.deepcopy(schema_dict[SchemaType.weak if schema_type.isweak() else SchemaType.strong])
        if schema_type.iserror():
            schema["_error"] = {'type': 'string'}

        return self.to_target(schema, target)

    def to_target(self, schema: dict, target: str):
        target = target.lower()
        if not (target in ['cerberus','spark']):
            raise ValueError(f'target must be one of "cerberus,spark"')

        if target.lower() == 'cerberus':
            return True, schema

        return True, StructType([StructField(k, self._TypeMap.get(v['type'])(), True) for k,v in schema.items()])

    