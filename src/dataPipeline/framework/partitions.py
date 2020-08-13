from abc import ABC, abstractmethod
from datetime import datetime, timezone
from enum import Enum, unique, auto
#from fiscalyear import *

@unique
class PartitionStrategy(Enum):
    Daily = auto()
    Weekly = auto()
    Monthly = auto()
    CalendarQuarterly = auto()
    CalendarHalf = auto()
    Yearly = auto()

class PartitionStrategyBase(ABC):
    @classmethod
    def get(cls, target_date: datetime = None) -> str:
        pass


class DailyPartitionStrategy(PartitionStrategyBase):
    @classmethod
    def get(cls, target_date: datetime = None) -> str:
        if not target_date: target_date = datetime.now(timezone.utc)

        return target_date.strftime("%Y/%Y%m/%Y%m%d")

class WeeklyPartitionStrategy(PartitionStrategyBase):
    @classmethod
    def get(cls, target_date: datetime = None) -> str:
        if not target_date: target_date = datetime.now(timezone.utc)

        return target_date.strftime("%Y/%YW%V") # %V is ISO 8601 week number, monday as first day

class MonthlyPartitionStrategy(PartitionStrategyBase):
    @classmethod
    def get(cls, target_date: datetime = None) -> str:
        if not target_date: target_date = datetime.now(timezone.utc)

        return target_date.strftime("%Y/%Y%m")

class CalendarQuarterPartitionStrategy(PartitionStrategyBase):
    @classmethod
    def get(cls, target_date: datetime = None) -> str:
        if not target_date: target_date = datetime.now(timezone.utc)
        quarter = (target_date.month-1)//3 + 1
        format = f"%Y/%YCQ{quarter}"
        return target_date.strftime(format)

class CalendarHalfPartitionStrategy(PartitionStrategyBase):
    @classmethod
    def get(cls, target_date: datetime = None) -> str:
        if not target_date: target_date = datetime.now(timezone.utc)
        half = round((target_date.month-1) / 12 + 1)
        format = f"%Y/%YCH{half}"
        return target_date.strftime(format)


class YearlyPartitionStrategy(PartitionStrategyBase):
    @classmethod
    def get(cls, target_date: datetime = None) -> str:
        if not target_date: target_date = datetime.now(timezone.utc)

        return target_date.strftime("%Y")

class PartitionStrategyFactory:
    @staticmethod
    def get(strategy: PartitionStrategy):
        strategies = {
            PartitionStrategy.Daily: DailyPartitionStrategy,
            PartitionStrategy.Weekly: WeeklyPartitionStrategy,
            PartitionStrategy.Monthly: MonthlyPartitionStrategy,
            PartitionStrategy.CalendarQuarterly: CalendarQuarterPartitionStrategy,
            PartitionStrategy.CalendarHalf: CalendarHalfPartitionStrategy,
            PartitionStrategy.Yearly: YearlyPartitionStrategy,
        }
        result = strategies[strategy]
        return result

