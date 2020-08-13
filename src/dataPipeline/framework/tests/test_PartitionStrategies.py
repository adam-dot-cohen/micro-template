import unittest
from framework.partitions import *
from datetime import datetime

class Test_test_PartitionStrategies(unittest.TestCase):
    def test_dateHierarchy_Daily(self):
        formatter = DailyPartitionStrategy
        target_date = datetime(2019, 7, 15, 6, 0, 0)
        expected = "2019/201907/20190715"

        self.assertEqual(formatter.get(target_date), expected)

    def test_dateHierarchy_Weekly(self):
        formatter = WeeklyPartitionStrategy
        target_date = datetime(2019, 7, 15, 6, 0, 0)
        expected = "2019/2019W29"

        self.assertEqual(formatter.get(target_date), expected)

    def test_dateHierarchy_Monthly(self):
        formatter = MonthlyPartitionStrategy
        target_date = datetime(2019, 7, 15, 6, 0, 0)
        expected = "2019/201907"

        self.assertEqual(formatter.get(target_date), expected)

    def test_dateHierarchy_CalendarQuarter(self):
        formatter = CalendarQuarterPartitionStrategy
        target_date = datetime(2019, 7, 15, 6, 0, 0)
        expected = "2019/2019CQ3"

        self.assertEqual(formatter.get(target_date), expected)

    def test_dateHierarchy_CalendarHalf(self):
        formatter = CalendarHalfPartitionStrategy
        target_date = datetime(2019, 7, 15, 6, 0, 0)
        expected = "2019/2019CH2"

        self.assertEqual(formatter.get(target_date), expected)

    def test_dateHierarchy_Yearly(self):
        formatter = YearlyPartitionStrategy
        target_date = datetime(2019, 7, 15, 6, 0, 0)
        expected = "2019"

        self.assertEqual(formatter.get(target_date), expected)

if __name__ == '__main__':
    unittest.main()
