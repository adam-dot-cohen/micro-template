import unittest
from framework.util import *

class Test_util(unittest.TestCase):
    def test_are_equal_both_None(self):
        self.assertEqual(are_equal(None, None, False), False)

    def test_are_equal_both_same(self):
        self.assertEqual(are_equal('A', 'A', False), True)

    def test_are_equal_different_case(self):
        self.assertEqual(are_equal('A', 'a', False), True)

    def test_are_equal_different_values(self):
        self.assertEqual(are_equal('A', 'b', False), False)

    def test_are_equal_both_None_strict(self):
        self.assertEqual(are_equal(None, None, True), False)

    def test_are_equal_both_same(self):
        self.assertEqual(are_equal('A', 'A', True), True)

    def test_are_equal_different_case(self):
        self.assertEqual(are_equal('A', 'a', True), False)

    def test_are_equal_different_values(self):
        self.assertEqual(are_equal('A', 'b', True), False)


if __name__ == '__main__':
    unittest.main()
