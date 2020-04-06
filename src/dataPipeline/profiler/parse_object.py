# Databricks notebook source
"""
Prototype for:
1.cerberus library as schema validator
2.cerberus with spark
3.schema infer
"""

from cerberus import Validator
#import pandas as pd
import csv



#df = pd.DataFrame()
#for chunk in pd.read_csv('D:\Talend_Staging\Insight\Input\SterlingNational_Laso_R_Demographic_11107019_11107019.csv', delimiter=',', header=1,chunksize=1000):
#    df = chunk


fileName = 'D:\Talend_Staging\Insight\Input\SterlingNational_Laso_R_Demographic_11107019_11107019_small.csv' 
fileNameRejected = 'D:\Talend_Staging\Insight\Input\SterlingNational_Laso_R_Demographic_11107019_11107019_small.csv' + '.REJECTED' 
fileNameDQ1 = 'D:\Talend_Staging\Insight\Input\SterlingNational_Laso_R_Demographic_11107019_11107019_small.csv' + '.DQ1'

#ToDo: source schema from a repo. It could be json/yaml.
#DQ level1 = is_parsable + required columns
schema = {
            'LASO_CATEGORY': {'type': 'string'},
            'ClientKey_id': {'type': 'string', 'required': True},
            'BRANCH_ID': {'type': 'string', 'required': True},
            'CREDIT_SCORE': {'type': 'integer', 'coerce': int, 'required': False},
            'CREDIT_SCORE_SOURCE': {'type': 'string', 'dependencies': 'CREDIT_SCORE', 'required': False}
        }

#ToDo: derive this from schema file and add errors+unpersableValues fields
fieldnames = ['LASO_CATEGORY','ClientKey_id','BRANCH_ID','CREDIT_SCORE','CREDIT_SCORE_SOURCE','unparsableValues', 'error']

v = Validator(schema)



with open(fileName, newline='') as csvfile, open(fileNameRejected, 'w', newline='') as csvWriteRej, open(fileNameDQ1, 'w', newline='') as csvWriteDQ1:
    #reader = csv.reader(csvfile, delimiter=',', doublequote=True)
    reader = csv.DictReader(csvfile, restkey='unparsableValues', delimiter=',')
    fileRejected = csv.DictWriter(csvWriteRej, fieldnames)
    fileNameDQ1 = csv.DictWriter(csvWriteDQ1, fieldnames) 
    #fileNameDQ1.writeheader()
    fileRejected.writeheader()
    for row in reader:                
        if not v.validate(row):                        
            print("--------------")
            print(row)
            print("---")
            print(v.errors)
            row['error']=v.errors
            fileRejected.writerow(row)                
        else:
            fileNameDQ1.writerow(row)



"""
#schema = {'name': {'type': 'string'}, 'CREDIT_SCORE': {'type': 'integer', 'coerce': int}}
document = {'name': 'john doe', 'CREDIT_SCORE': 'x50'}
if v.validate(document):
    print('data is valid')    
else:
    print('invalid data')
    print(v.errors)
"""
