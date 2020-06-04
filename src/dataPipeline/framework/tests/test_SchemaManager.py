import unittest
from framework.schema import SchemaManager, SchemaType
from pyspark.sql.types import StringType

class Test_test_SchemaManager(unittest.TestCase):
    def test_get_noexist_weak_cerberus(self):
        success, schema = SchemaManager().get('demographics', SchemaType.weak, 'cerberus')        

        self.assertEqual(success, False)
        self.assertEqual(schema, None)

    def test_get_demographics_weak_cerberus_1(self):
        success, schema = SchemaManager().get('demographic', SchemaType.weak, 'cerberus')        

        self.assertEqual(success, True)
        self.assertNotEqual(schema, None)

    def test_get_demographics_weak_cerberus_2(self):
        success, schema = SchemaManager().get('Demographic', SchemaType.weak, 'cerberus')        

        self.assertEqual(success, True)
        self.assertNotEqual(schema, None)

    def test_get_demographics_weak_error_cerberus(self):
        success, schema = SchemaManager().get('demographic', SchemaType.weak_error, 'cerberus')        

        self.assertEqual(success, True)
        self.assertNotEqual(schema, None)
        lastitem = list(schema.items())[-1]
        self.assertEqual(lastitem[0], "_error")

    def test_get_demographics_strong_cerberus(self):
        success, schema = SchemaManager().get('demographic', SchemaType.strong, 'cerberus')        

        self.assertEqual(success, True)
        self.assertNotEqual(schema, None)

    def test_get_demographics_strong_error_cerberus(self):
        success, schema = SchemaManager().get('demographic', SchemaType.strong_error, 'cerberus')        

        self.assertEqual(success, True)
        self.assertNotEqual(schema, None)
        lastitem = list(schema.items())[-1]
        self.assertEqual(lastitem[0], "_error")

    def test_get_demographics_weak_spark_1(self):
        success, schema = SchemaManager().get('demographic', SchemaType.weak, 'spark')  
        
        self.assertEqual(success, True)
        self.assertNotEqual(schema, None)
        self.assertTrue( all((lambda: isinstance(field.dataType, StringType))() for field in schema.fields) )

    def test_get_demographics_weak_spark_2(self):
        success, schema = SchemaManager().get('Demographic', SchemaType.weak, 'spark')        

        self.assertEqual(success, True)
        self.assertNotEqual(schema, None)
        self.assertTrue( all((lambda: isinstance(field.dataType, StringType))() for field in schema.fields) )

    def test_get_demographics_weak_error_spark(self):
        success, schema = SchemaManager().get('demographic', SchemaType.weak_error, 'spark')        

        self.assertEqual(success, True)
        self.assertNotEqual(schema, None)
        self.assertTrue( all((lambda: isinstance(field.dataType, StringType))() for field in schema.fields) )
        lastitem = schema.fields[-1]
        self.assertEqual(lastitem.name, "_error")

    def test_get_demographics_strong_spark(self):
        success, schema = SchemaManager().get('demographic', SchemaType.strong, 'spark')        
        self.assertEqual(success, True)

    def test_get_demographics_strong_error_spark(self):
        success, schema = SchemaManager().get('demographic', SchemaType.strong_error, 'spark')        
        self.assertEqual(success, True)
        lastitem = schema.fields[-1]
        self.assertEqual(lastitem.name, "_error")

if __name__ == '__main__':
    unittest.main()
