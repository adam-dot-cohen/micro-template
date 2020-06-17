import unittest
from framework.partitions import *
from framework.pipeline.PipelineTokenMapper import *

class Test_test_PipelineTokenMapper(unittest.TestCase):
    def test_dateHierarchy_default(self):
        tokenString = "{dateHierarchy}"
        expected = DailyPartitionStrategy.get()
        actual, _ = PipelineTokenMapper().resolve(None, tokenString)

        self.assertEqual(expected, actual)

    def test_dateHierarchy_Daily(self):
        tokenString = "{dateHierarchy}"
        expected = DailyPartitionStrategy.get()
        actual, _ = PipelineTokenMapper().resolve(None, tokenString, PartitionStrategy.Daily)

        self.assertEqual(expected, actual)

    def test_dateHierarchy_Weekly(self):
        tokenString = "{dateHierarchy}"
        expected = WeeklyPartitionStrategy.get()
        actual, _ = PipelineTokenMapper().resolve(None, tokenString, PartitionStrategy.Weekly)

        self.assertEqual(expected, actual)

    def test_dateHierarchy_Monthly(self):
        tokenString = "{dateHierarchy}"
        expected = MonthlyPartitionStrategy.get()
        actual, _ = PipelineTokenMapper().resolve(None, tokenString, PartitionStrategy.Monthly)

        self.assertEqual(expected, actual)

    def test_dateHierarchy_CalendarQuarter(self):
        tokenString = "{dateHierarchy}"
        expected = CalendarQuarterPartitionStrategy.get()
        actual, _ = PipelineTokenMapper().resolve(None, tokenString, PartitionStrategy.CalendarQuarterly)

        self.assertEqual(expected, actual)

    def test_dateHierarchy_CalendarHalf(self):
        tokenString = "{dateHierarchy}"
        expected = CalendarHalfPartitionStrategy.get()
        actual, _ = PipelineTokenMapper().resolve(None, tokenString, PartitionStrategy.CalendarHalf)

        self.assertEqual(expected, actual)

    def test_dateHierarchy_Yearly(self):
        tokenString = "{dateHierarchy}"
        expected = YearlyPartitionStrategy.get()
        actual, _ = PipelineTokenMapper().resolve(None, tokenString, PartitionStrategy.Yearly)

        self.assertEqual(expected, actual)


if __name__ == '__main__':
    unittest.main()
