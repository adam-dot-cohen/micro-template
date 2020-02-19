import pandas as pd
import pandas_profiling
import os 

class DataProfiler(object):
    """Profiles the data in an external CSV file and emits a data profile report"""

    def __init__(self, source):
        self.__source = source

    def exec(self, outputFileName=None):

        if outputFileName is None:
            outputFileName = os.path.splitext(self.__source)[0] + '.html'

        print('Generating data profile report for ' + self.__source)

        df = pd.read_csv(self.__source)
        profile = df.profile_report(title='Data Profiling Report')
        profile.to_file(outputfile=outputFileName)
