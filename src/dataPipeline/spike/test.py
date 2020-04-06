import pandas as pd

def _get_col_dtype(col):
    """
    Infer datatype of a pandas column, process only if the column dtype is object. 
    input:   col: a pandas Series representing a df column. 
    """
    if col.dtype =="object":
        # try numeric
        try:
            col_new = pd.to_numeric(col.dropna().unique())
            return col_new.dtype.name
        except:
            try:
                col_new = pd.to_datetime(col.dropna().unique())
                return col_new.dtype.name
            except:
                return 'object'
    else:
        return col.dtype

df = pd.read_csv('e:\\TEMP\\Dataset\\SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv', nrows=100000, verbose=1)
print('raw dataframe')
df.info()

infer_type = lambda x: pd.api.types.infer_dtype(x, skipna=True)
df_types = pd.DataFrame(df.apply(_get_col_dtype, axis=0)).reset_index().rename(columns={'index': 'column', 0: 'type'})
print('datatypes dataframe')
df_types

loop_types = df_types.values.tolist()

for col in loop_types:
    if col[1] in ['mixed','object']:
        pass
    else:
        df[col[0]] = df[col[0]].astype(col[1])

print('updated dataframe')
df.info()
