import pandas as pd
import pandas_profiling
import os 
from datetime import datetime

class DataProfiler(object):
    """Profiles the data in an external CSV file and emits a data profile report"""

    def __init__(self, source):
        self.__source = source

    def exec(self, outputFileName: str=None):

        if outputFileName is None:
            outputFileName = os.path.splitext(self.__source)[0] + '.html'

        print(f'Generating data profile report for {self.__source}')

        start_timestamp = datetime.now()
        print(f'Start: {start_timestamp}')

        df = pd.read_csv(self.__source, error_bad_lines=False, warn_bad_lines=False)
        readcomplete_timestamp = datetime.now()
        print(f'Read Complete: {readcomplete_timestamp}')

        profile = df.profile_report(title='Data Profiling Report')
        profilecomplete_timestamp = datetime.now()
        print(f'Profile Complete: {profilecomplete_timestamp}')

        print(f'Generating data profile report to {outputFileName}')
        profile.to_file(output_file=outputFileName)
        end_timestamp = datetime.now()
        print(f'Complete: {end_timestamp}')

