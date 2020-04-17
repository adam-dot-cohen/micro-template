import unittest
from framework.schema import *

class Test_test_SchemaManager(unittest.TestCase):
    def test_get_demographics_weak_cerberus_1(self):
        success, schema = SchemaManager().get('demographic', SchemaType.weak, 'cerberus')        
        self.assertEqual(success, True)

    def test_get_demographics_weak_cerberus_2(self):
        success, schema = SchemaManager().get('Demographic', SchemaType.weak, 'cerberus')        
        self.assertEqual(success, True)

    def test_get_demographics_weak_error_cerberus(self):
        success, schema = SchemaManager().get('demographic', SchemaType.weak_error, 'cerberus')        
        self.assertEqual(success, True)

    def test_get_demographics_strong_cerberus(self):
        success, schema = SchemaManager().get('demographic', SchemaType.strong, 'cerberus')        
        self.assertEqual(success, True)

    def test_get_demographics_strong_error_cerberus(self):
        success, schema = SchemaManager().get('demographic', SchemaType.strong_error, 'cerberus')        
        self.assertEqual(success, True)

if __name__ == '__main__':
    unittest.main()
