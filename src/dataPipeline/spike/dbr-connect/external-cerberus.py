from schema import *
from collections import OrderedDict
import json


class Example(object):
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
                                    ( 'CREDIT_SCORE_SOURCE',    {'type': 'string', 'required': 'False'}         )
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
                                    ('MCC_CODE',                {'type': 'string'})
                                ])
                }

    def run(self):
        data_category = "demographic"    
        sm = SchemaManager()

        _, error_schema = sm.get(data_category, SchemaType.strong_error,  'spark')    
        _, strong_schema = sm.get(data_category, SchemaType.strong, 'cerberus')

        #print(strong_schema)

        raw_schema = self._schemas.get(data_category+"_boundary", None)
        print((raw_schema))
        print(type(raw_schema))
        #print(dict(raw_schema))

        # schema: dict = OrderedDict(raw_schema.items())
        json_obj = json.dumps(raw_schema,indent=4)
        print(json_obj)

        ordDict = json.loads(json_obj, object_hook=OrderedDict)
        print(ordDict)

        
        
        # print(json.dumps(d))

#('CREDIT_SCORE', {'type': 'integer', 'coerce': <class 'int'>, 'required': False, 'min': 550, 'meta': ('BDY.2', {'replace_value': 550})})
    # Replace field definition in raw cerberus schema
    def replace_field_def(self):
        dby2Rule = json.load(open(".\dby-2.json"))

        #dby2Rule = json.load(open(".\dby-2.json"), object_pairs_hook=OrderedDict)
        #print('dby2Rule:\n',dict(dby2Rule))

        raw_schema = self._schemas.get("demographic", None)
        # print("raw_schema['CREDIT_SCORE']:\n",raw_schema['CREDIT_SCORE'])
        print('raw_schema:\n',raw_schema)
        # raw_schema['CREDIT_SCORE'] = dict(dby2Rule['CREDIT_SCORE'])
        # print('MODraw_schema:\n',raw_schema)

        for x,v in dby2Rule["CREDIT_SCORE"].items():
            if v=="<int>": 
                dby2Rule["CREDIT_SCORE"][x] = int                
            print (x,v)

        print(dict(dby2Rule["CREDIT_SCORE"]))

        schema: dict = OrderedDict(raw_schema.items())
        schema["CREDIT_SCORE"] = dict(dby2Rule["CREDIT_SCORE"])
        print('MODraw_schema:\n',schema)

    # add elements to raw schema from json
    def add_dby2_rule(self):
        dby2Rule = json.load(open(".\dby2-add.json"))
        
        raw_schema = self._schemas.get("demographic", None)
        # print("raw_schema['CREDIT_SCORE']:\n",raw_schema['CREDIT_SCORE'])
        print('raw_schema:\n',raw_schema)

        # find key and add new elements to key's dictionary.
        schema: dict = OrderedDict(raw_schema.items())
        for col, elements in dby2Rule.items():
            #print(elements)
            for elem, val in elements.items():
                print(elem, val)
                schema[col][elem] = val

        print('MOD_schema:\n',schema)

if __name__ == '__main__':
    Example().add_dby2_rule()