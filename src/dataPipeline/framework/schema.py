import copy
from pyspark.sql.types import *
from enum import Enum, auto
from collections import OrderedDict

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
                }
        }    
    _TypeMap = {
        "string" : StringType,
        "number" : DoubleType,
        "float"  : FloatType,
        "integer": IntegerType,
        "boolean": BooleanType,
        "object" : StructType,
        "array"  : ArrayType
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

        return True, StructType([StructField(k,self._TypeMap.get(v['type'])(),True) for k,v in schema.items()])

    