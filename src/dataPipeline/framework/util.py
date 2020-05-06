def rchop(s, sub):
    return s[:-len(sub)] if s.endswith(sub) else s

def lchop(s, sub):
    return s[len(sub):] if s.startswith(sub) else s

def are_equal(value1: str, value2: str, strict: bool):
        return value1 == value2 if strict else value1.lower() == value2.lower()

def ensure_output_dir(self, uri: str):
    from pathlib import Path
    output_dir = Path(uri).parents[0]
    output_dir.mkdir(parents=True, exist_ok=True)

def copyfile(src, dest):
    from shutil import copyfile
    copyfile(src, dest)

def get_dbutils():
    spark = SparkSession.builder.getOrCreate()
    try:
        from pyspark.dbutils import DBUtils
        return DBUtils(spark)
    except ImportError:
        from IPython import get_ipython
        return get_ipython().user_ns['dbutils']