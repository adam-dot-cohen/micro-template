using System;
using System.IO;
using System.Linq;
using System.Text;
using Laso.IO.Structured;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Laso.IO.Tests.Structured
{
    public class DelimitedFileReaderTests
    {
        public class TheData
        {
            public string MerchantID { get; set; }
            public DateTime ReportDate { get; set; }
            public int SalesCount { get; set; }
            public double SalesAmount { get; set; }
        }

        private const string Header =
            "MerchantID,ReportDate,SalesCount,SalesAmount\r\n";

        private const string Body = 
            "123456,2017-09-10,14,1555.75\r\n" +
            "789012,2017-09-11,24,2555.75";

        [Fact]
        public void Reading_Single_Records_Should_Succeed()
        {
            // Arrange
            using var reader = new DelimitedFileReader();
            const string Content = Header + Body;

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Content));
            reader.Open(stream);

            // Act
            var result = reader.ReadRecord<TheData>();
            var result2 = reader.ReadRecord<TheData>();

            // Assert
            result.ShouldNotBeNull();
            result.MerchantID.ShouldBe("123456");
            result.ReportDate.ShouldBe(DateTime.Parse("2017-09-10"));
            result.SalesCount.ShouldBe(14);
            result.SalesAmount.ShouldBe(1555.75);

            result2.MerchantID.ShouldBe("789012");
            result2.ReportDate.ShouldBe(DateTime.Parse("2017-09-11"));
            result2.SalesCount.ShouldBe(24);
            result2.SalesAmount.ShouldBe(2555.75);
        }

        [Fact]
        public void Reading_All_Records_Should_Succeed()
        {
            // Arrange
            using var reader = new DelimitedFileReader();
            const string Content = Header + Body;

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Content));
            reader.Open(stream);

            // Act
            var result = reader.ReadRecords<TheData>().ToList();

            // Assert
            result.ShouldNotBeEmpty();
            result.Count.ShouldBe(2);

            result[0].ShouldNotBeNull();
            result[0].MerchantID.ShouldBe("123456");
            result[0].ReportDate.ShouldBe(DateTime.Parse("2017-09-10"));
            result[0].SalesCount.ShouldBe(14);
            result[0].SalesAmount.ShouldBe(1555.75);

            result[1].ShouldNotBeNull();
            result[1].MerchantID.ShouldBe("789012");
            result[1].ReportDate.ShouldBe(DateTime.Parse("2017-09-11"));
            result[1].SalesCount.ShouldBe(24);
            result[1].SalesAmount.ShouldBe(2555.75);
        }

        [Fact]
        public void Reading_Without_Header_Should_Succeed()
        {
            // Arrange
            using var reader = new DelimitedFileReader
            {
                Configuration = { HasHeaderRecord = false }
            };

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Body));
            reader.Open(stream);

            // Act
            var result = reader.ReadRecord<TheData>();
            var result2 = reader.ReadRecord<TheData>();

            // Assert
            result.ShouldNotBeNull();
            result.MerchantID.ShouldBe("123456");
            result.ReportDate.ShouldBe(DateTime.Parse("2017-09-10"));
            result.SalesCount.ShouldBe(14);
            result.SalesAmount.ShouldBe(1555.75);

            result2.MerchantID.ShouldBe("789012");
            result2.ReportDate.ShouldBe(DateTime.Parse("2017-09-11"));
            result2.SalesCount.ShouldBe(24);
            result2.SalesAmount.ShouldBe(2555.75);
        }

        [Fact]
        public void Reading_With_Different_Delimiter_Should_Succeed()
        {
            // Arrange
            using var reader = new DelimitedFileReader
            {
                Configuration = { Delimiter = "|" }
            };
            var content = (Header + Body).Replace(",", reader.Configuration.Delimiter);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            reader.Open(stream);

            // Act
            var result = reader.ReadRecords<TheData>().ToList();

            // Assert
            result.ShouldNotBeEmpty();
            result.Count.ShouldBe(2);

            result[0].ShouldNotBeNull();
            result[0].MerchantID.ShouldBe("123456");
            result[0].ReportDate.ShouldBe(DateTime.Parse("2017-09-10"));
            result[0].SalesCount.ShouldBe(14);
            result[0].SalesAmount.ShouldBe(1555.75);

            result[1].ShouldNotBeNull();
            result[1].MerchantID.ShouldBe("789012");
            result[1].ReportDate.ShouldBe(DateTime.Parse("2017-09-11"));
            result[1].SalesCount.ShouldBe(24);
            result[1].SalesAmount.ShouldBe(2555.75);
        }


        [Fact]
        public void Reading_End_Of_File_Should_Return_Null()
        {
            // Arrange
            using var reader = new DelimitedFileReader();
            const string Content = Header + Body;

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Content));
            reader.Open(stream);

            // Act
            var result = reader.ReadRecord<TheData>();
            var result2 = reader.ReadRecord<TheData>();
            var result3 = reader.ReadRecord<TheData>();

            // Assert
            result.ShouldNotBeNull();
            result2.ShouldNotBeNull();
            result3.ShouldBeNull();
        }
    }
}
