from collections import OrderedDict
  
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
                                    ( 'CREDIT_SCORE_SOURCE',    {'type': 'string', 'required': False, 
                                                                  'meta': ('BDY.2', {'replace_value': 600})
                                                                })
                                ])
                }

#('APP', {'other_meta': 'test'})

#Working w/o array
raw_schema = _schemas.get('demographic_boundary', None)
#print(raw_schema['CREDIT_SCORE']['meta'])
for item, v in raw_schema.items():
  print('\n',item, v.items())
  meta_dict =  dict(filter(lambda elem: elem[0] == 'meta', v.items()))
  #print(meta_dict.items())
  dqdby_dict =  dict(filter(lambda elem: elem[0] == 'BDY.2', meta_dict.values()))
  if dqdby_dict:
  #print(meta_dict.get('meta', None))
  #print(type(meta_dict.get('meta', None)), '\n')
    print(dqdby_dict)
    print(dqdby_dict.get('BDY.2', None).get('replace_value'), '\n') 



# _schemas = {
#         # these are ordereddicts to preserve the order when converting to a list for spark
#             'demographic': OrderedDict([
#                                     ( 'LASO_CATEGORY',          {'type': 'string'}                                  ),
#                                     ( 'ClientKey_id',           {'type': 'integer', 'coerce': int, 'required': True} ),
#                                     ( 'BRANCH_ID',              {'type': 'string', 'required': True}                    ),
#                                     ( 'CREDIT_SCORE',           {'type': 'integer', 'coerce': int, 'required': False}),
#                                     ( 'CREDIT_SCORE_SOURCE',    {'type': 'string', 'required': False}         )
#                                 ]),
#             'demographic_boundary': OrderedDict([
#                                     ( 'LASO_CATEGORY',          {'type': 'string'}                                  ),
#                                     ( 'ClientKey_id',           {'type': 'integer', 'coerce': int, 'required': True} ),
#                                     ( 'BRANCH_ID',              {'type': 'string', 'required': True}                    ),
#                                     ( 'CREDIT_SCORE',           {'type': 'integer', 'coerce': int, 'required': False, 'min': 550,
#                                                                  'meta': (('BDY.2', {'replace_value': 550}),
#                                                                           ('OTHER', {'other_meta': 22})  
#                                                                          )
#                                                                 }),
#                                     ( 'CREDIT_SCORE_SOURCE',    {'type': 'string', 'required': False, 
#                                                                   'meta': ('BDY.2', {'replace_value': 600})
#                                                                 })
#                                 ])
#                 }

# #('APP', {'other_meta': 'test'})

# raw_schema = _schemas.get('demographic_boundary', None)
# #print(raw_schema['CREDIT_SCORE']['meta'])
# for item, v in raw_schema.items():
#   print('\n',item, v.items())
#   meta_dict =  dict(filter(lambda elem: elem[0] == 'meta', v.items()))
#   print(meta_dict.values())
#   bdy_dict =  dict(filter(lambda elem: elem[0] == 'BDY.2', meta_dict.items()))
#   print(bdy_dict.values())
#   dqdby_dict =  dict(filter(lambda elem: elem[0] == 'BDY.2', meta_dict.values()))
#   if dqdby_dict:
#   #print(meta_dict.get('meta', None))
#   #print(type(meta_dict.get('meta', None)), '\n')
#     print(dqdby_dict)
#     print(dqdby_dict.get('BDY.2', None).get('replace_value'), '\n') 



#other test
uri = "/mnt/curated/00000000-0000-0000-0000-000000000000/2020/202005/20200514/file.csv" 
from pathlib import Path
mypath = Path(uri).parents[0] / "delta"
print(mypath)

lFields = [{"col":"CREDIT_SCORE","replacement_value": "334"},
            {"col":"OTHER","replacement_value": "fgf"}]
#{col: lFields[col] for col in raw_schema}
# newDic = {option : raw_schema[option] for option in lFields if option in raw_schema}
# if newDic:
#   print(newDic)
#   print(raw_schema["CREDIT_SCORE"])
colOnly = [x['col'] for x in lFields]
print(colOnly)
for x in colOnly:
  print(x)