from pyspark.sql.types import *
from enum import Enum, auto
from collections import OrderedDict
from datetime import datetime
from importlib import resources
import json
import pandas as pd
from datetime import datetime

# coerscion functions
to_date = (lambda value:  datetime.strptime(value, '%Y-%m-%d %H:%M:%S'))  
#to_date = (lambda myDateTime:  pd.to_datetime(myDateTime))  # use pandas to parse common formats

class SchemaType(Enum):
    weak = auto()
    strong = auto()
    weak_error = auto()
    strong_error = auto()
    positional = auto()

    def isweak(self):
        return self in [SchemaType.weak, SchemaType.weak_error]
    def isstrong(self):
        return self in [SchemaType.strong, SchemaType.strong_error]
    def iserror(self):
        return self in [SchemaType.weak_error, SchemaType.strong_error]
    def ispositional(self):
        return self in [SchemaType.positional]


# TODO: Collapse down to just strong, mutate to weak on request
class SchemaManager:        
    #cache schemas here as part of the init()
    _TypeMap = {
        "string"    : StringType(),
        "number"    : DoubleType(),
        "float"     : DoubleType(),
        "integer"   : DecimalType(38,0),  #changed to DecimalType (BigDecimal) with max precision as the REDACTED number fields are larger than IntegerType to avoid collision. DecimalType.MAX_PRECISION() not implemented in python only scala.
        "boolean"   : BooleanType(),
        "object"    : StructType(),
        "array"     : ArrayType,
        'datetime'  : TimestampType() 
        }
    
    _Schemas = {}

    def __init__(self, productId, hostconfigmodule):
        self._Schemas = self.load_schemas(productId, hostconfigmodule)

    # take a rule_id find it in the base-schema's ruleSet and add it to the given schema. Otherwise, return given schema.
    def add_dq_rule(self, name, schemas, schema, rule_id):
        augmentedSchema = schema
        DataCategoryConfig = [c for c in schemas if c["DataCategory"]== name]
        if not DataCategoryConfig or not rule_id:
            return augmentedSchema
 
        #print("Schemas in\n", schemas)
        ruleSet = DataCategoryConfig[0]["RuleSet"]
        #print("RuleSet\n", ruleSet)

        rule = [r for r in ruleSet if r.get("RuleId")==rule_id]
        if not rule:
            return augmentedSchema

        ruleSpecs = rule[0]["RuleSpecification"]
        #print("ruleSpecs\n", ruleSpecs)
   
        # replace column definition with rule's attributes
        for spec in ruleSpecs:
            #print(spec)
            for col, colSpec in spec.items():
                #print("col, colSpec:\n ", col, colSpec)
                augmentedSchema[col] = colSpec
                #for elem, val in colSpec.items(): #this approach add elements as opposed to replace.
                #    #print("colSpec.items():\n", elem, val)
                #    augmentedSchema[col][elem] = val

        
        return augmentedSchema

    def get(self, name: str, schema_type: SchemaType, target: str, rule_id: str = None):
        target = target.lower()
        schemas = self._Schemas
        if not (target in ['cerberus','spark']):
            raise ValueError(f'target must be one of "cerberus,spark"')
        
        DataCategoryConfig = [c for c in schemas if c["DataCategory"]== name]
        if not DataCategoryConfig:
            return False, None
        
        raw_schema = DataCategoryConfig[0]["Schema"].copy()
        
        if raw_schema is None:
            return False, None

        if schema_type.iserror():
            # copy the dict so we can add the error column
            schema: dict = OrderedDict(raw_schema.items())
            schema["_error"] = {'type': 'string', 'nullable': True}

        else:
            schema = raw_schema  # no additions to schema
        
        # positional only cares about column position.
        if schema_type.ispositional():    
            for col, val in schema.items():
                schema[col] = {}

        # cerberus takes a dict, so we are done
        if target.lower() == 'cerberus':
            schema = self.add_dq_rule(name, schemas, schema, rule_id)  #augment with rule_id's metadata if needed
            return True, dict(schema)

        # reshape the dictionary into a list of StructFields for spark
        return True, StructType([
                                    StructField(k, 
                                                StringType() if schema_type.isweak() else self._TypeMap.get(v.get('type')), 
                                                v.get('nullable', True))
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
    def load_schemas(self, productId, hostconfigmodule):
        with resources.open_text(hostconfigmodule, "products.json") as products_file:
            products = json.loads(products_file.read())
        with resources.open_text(hostconfigmodule, "schemas.json") as schemas_file:
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
                #print(f"\Schema for {c.get('CategoryName')} before decoding:\n{baseSchema}")

                # get cerberus' compatible dict
                schema = self.create_OrderedDict(baseSchema)
                #print(f"\Base schema for {c.get('CategoryName')}:\n{schema}")

                product_schemas.append({'DataCategory':c.get('CategoryName'), 'Schema':schema, 'RuleSet':schema_config.get("RuleSet")})
        
            return  product_schemas
        else:        
            return None