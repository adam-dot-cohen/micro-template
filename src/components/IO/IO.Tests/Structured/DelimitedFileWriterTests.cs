using System;
using System.IO;
using Laso.IO.Extensions;
using Laso.IO.Structured;
using Shouldly;
using Xunit;

namespace Laso.IO.Tests.Structured
{
public class DelimitedFileWriterTests
    {
        public class TheData
        {
            public string TestString { get; set; }
            public DateTime TestDate { get; set; }
            public int TestInteger { get; set; }
            public double TestDouble { get; set; }
            public decimal TestDecimal { get; set; }
        }

        [Fact]
        public void When_Invoked_Should_Succeed()
        {
            var data = new TheData
            {
                TestString = "test string",
                TestDate = DateTime.Parse("2017-12-21 13:37:08.001"),
                TestInteger = 12345,
                TestDouble = 12345.67890d,
                TestDecimal = 12345.67890123m
            };

            // Arrange
            using var stream = new MemoryStream();
            using (var writer = new DelimitedFileWriter())
            {
                writer.Open(stream);

                // Act
                writer.WriteRecords(new[] { data });
            }

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            stream.GetString().ShouldBe("TestString,TestDate,TestInteger,TestDouble,TestDecimal\r\ntest string,12/21/2017 1:37:08 PM,12345,12345.6789,12345.67890123\r\n");
        }

        [Fact]
        public void When_Invoked_With_Converter_Should_Succeed()
        {
            var data = new TheData
            {
                TestString = "test string",
                TestDate = DateTime.Parse("2017-12-21 13:37:08.001"),
                TestInteger = 12345,
                TestDouble = 12345.67890d,
                TestDecimal = 12345.67890123m
            };

            // Arrange
            using var stream = new MemoryStream();
            using (var writer = new DelimitedFileWriter())
            {
                writer.Configuration.TypeConverterOptions.Add(new TypeConverterOption
                {
                    Type = typeof(DateTime),
                    Format = "yyyy-MM-dd HH:mm:ss.fff"
                });
                writer.Configuration.TypeConverterOptions.Add(new TypeConverterOption
                {
                    Type = typeof(DateTime?),
                    Format = "yyyy-MM-dd HH:mm:ss.fff"
                });
                writer.Open(stream);

                // Act
                writer.WriteRecords(new[] { data });
            }

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            stream.GetString().ShouldBe("TestString,TestDate,TestInteger,TestDouble,TestDecimal\r\ntest string,2017-12-21 13:37:08.001,12345,12345.6789,12345.67890123\r\n");
        }

        [Fact]
        public void When_Invoked_With_Converter_Should_Convert_All_Of_Type()
        {
            var data = new
            {
                TestDate1 = DateTime.Parse("2017-12-21 13:37:08.001"),
                TestDate2 = DateTime.Parse("2016-1-21 01:06:08.001"),
                TestDate3 = DateTime.Parse("2017-12-22 13:27:08.101"),
                TestDate4 = (DateTime?)DateTime.Parse("2017-12-22 13:27:08.101")
            };

            // Arrange
            using var stream = new MemoryStream();
            using (var writer = new DelimitedFileWriter())
            {
                writer.Configuration.TypeConverterOptions.Add(new TypeConverterOption
                {
                    Type = typeof(DateTime),
                    Format = "yyyy-MM-dd HH:mm:ss.fff"
                });
                writer.Configuration.TypeConverterOptions.Add(new TypeConverterOption
                {
                    Type = typeof(DateTime?),
                    Format = "yyyy-MM-dd HH:mm:ss.fff"
                });
                writer.Open(stream);

                // Act
                writer.WriteRecords(new[] { data });
            }

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            stream.GetString().ShouldBe("TestDate1,TestDate2,TestDate3,TestDate4\r\n2017-12-21 13:37:08.001,2016-01-21 01:06:08.001,2017-12-22 13:27:08.101,2017-12-22 13:27:08.101\r\n");
        }
    }
}
