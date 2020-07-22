using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;
using Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Queries;
using Laso.AdminPortal.UnitTests.Extensions;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Laso.AdminPortal.UnitTests.Monitoring.DataQualityPipeline
{
    public class GetFileBatchInfoHandlerTests
    {
        [Fact]
        public async Task When_valid_files_Should_succeed()
        {
            // Arrange
            var handler = new GetFileBatchInfoHandler();
            var partnerId = Guid.Parse("93383d2d-07fd-488f-938b-f9ce1960fee3");
            var dummyId = Guid.NewGuid();
            var input = new GetFileBatchInfoQuery
            {
                FilePaths = new[]
                {
                    /* 0 */ $"https://escrow.blob/transfer-{partnerId}/Bank_Laso_d_AccountTransaction_20191029_20191029095932.csv",
                    /* 1 */ $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_W_AccountTransaction_201910_20191029095932.csv",
                    /* 2 */ $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_Q_AccountTransaction_2019_20191029095932.csv",
                    /* 3 */ $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_Y_AccountTransaction_20191029_201910290959.csv",
                    /* 4 */ $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_m_AccountTransaction_20191029_2019102909.csv",
                    /* 5 */ $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_R_AccountTransaction_20191029_20191029.csv",
                    /* 6 */ $"https://escrow.blob/transfer-{partnerId}/incoming/transfer-{dummyId}/Bank_Laso_R_AccountTransaction_20191029_20191029095932.csv.gpg.zip",
                    /* 7 */ $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_R_AccountTransaction_20191029_20191029095932.csv.gpg.zip",
                    /* 8 */ $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_R_AccountTransaction_20191029_20191029095932.csv.gpg",
                }
            };

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.ShouldSucceed();
            var result = response.Result.Files.ToArray();
            result.All(r => r.PartnerId == partnerId.ToString()).ShouldBeTrue();

            result[0].Frequency.ShouldBe("D");
            result[0].EffectiveDate.ShouldBe(new DateTimeOffset(2019, 10, 29, 0, 0, 0, TimeSpan.Zero));
            result[0].TransmissionTime.ShouldBe(new DateTimeOffset(2019, 10, 29, 9, 59, 32, TimeSpan.Zero));
            result[0].DataCategory.ShouldBe("AccountTransaction");
            result[0].Filename.ShouldBe("Bank_Laso_d_AccountTransaction_20191029_20191029095932.csv");
            result[0].Path.ShouldBe(input.FilePaths.First());

            result[1].EffectiveDate.ShouldBe(new DateTimeOffset(2019, 10, 1, 0, 0, 0, TimeSpan.Zero));
            result[1].Frequency.ShouldBe("W");

            result[2].EffectiveDate.ShouldBe(new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero));
            result[2].Frequency.ShouldBe("Q");

            result[3].TransmissionTime.ShouldBe(new DateTimeOffset(2019, 10, 29, 9, 59, 0, TimeSpan.Zero));
            result[3].Frequency.ShouldBe("Y");

            result[4].TransmissionTime.ShouldBe(new DateTimeOffset(2019, 10, 29, 9, 0, 0, TimeSpan.Zero));
            result[4].Frequency.ShouldBe("M");

            result[5].TransmissionTime.ShouldBe(new DateTimeOffset(2019, 10, 29, 0, 0, 0, TimeSpan.Zero));
        }

        [Theory]
        [InlineData("https://escrow.blob/transfer-93383d2d-07fd-488f-938b-f9ce1960fee3/incoming/", "filename", "filename not found")]
        [InlineData("https://escrow.blob/93383d2d-07fd-488f-938b-f9ce1960fee3/incoming/Bank_Laso_R_AccountTransaction_20191029_20191029095932.csv", "path", "partner id not found")]
        [InlineData("https://escrow.blob/incoming/transfer-93383d2d-07fd-488f-938b-f9ce1960fee3/Bank_Laso_R_AccountTransaction_20191029_20191029095932.csv.gpg.zip", "path", "partner id not found")]
        [InlineData("https://escrow.blob/transfer-93383d2d-07fd-488f-938b-f9ce1960fee3/incoming/Bank_Laso_R_AccountTransaction_2019102_20191029095932.csv.gpg.zip", "datetime", "not be parsed")]
        [InlineData("https://escrow.blob/transfer-93383d2d-07fd-488f-938b-f9ce1960fee3/incoming/Bank_Laso_R_AccountTransaction_20191029_2019102909593.csv.gpg.zip", "datetime", "not be parsed")]
        public async Task When_filename_invalid_Should_fail(string path, string errorKey, string errorMessageContains)
        {
            // Arrange
            var handler = new GetFileBatchInfoHandler();
            var input = new GetFileBatchInfoQuery
            {
                FilePaths = new[]
                {
                    path
                }
            };

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.ShouldFail();
            response.ValidationMessages[0].Key.ShouldBe(errorKey);
            response.ValidationMessages[0].Message.ToLowerInvariant().ShouldContain(errorMessageContains);
        }

        [Fact]
        public async Task When_filename_missing_component_Should_fail()
        {
            // Arrange
            var handler = new GetFileBatchInfoHandler();
            var partnerId = Guid.Parse("93383d2d-07fd-488f-938b-f9ce1960fee3");
            var input = new GetFileBatchInfoQuery
            {
                FilePaths = new[]
                {
                    $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_AccountTransaction_20191029_20191029095932.csv.gpg",
                    $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_R_20191029_20191029095932.csv.gpg",
                    $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_R_AccountTransaction_20191029095932.csv.gpg",
                    $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_R_AccountTransaction_20191029_.csv.gpg",
                    $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_R_AccountTransaction_20191029_20191029095932",
                    $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_R_AccountTransaction_20191029_20191029095932.csv.gpg",
                    $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_R_AccountTransaction_20191029_20191029095932.csv.gpg",
                    $"https://escrow.blob/transfer-{partnerId}/incoming/Bank_Laso_R_AccountTransaction_20191029_20191029095932.csv.gpg",
                }
            };

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.ShouldFail();
            foreach (var message in response.ValidationMessages)
            {
                message.Key.ShouldBe("filename");
                message.Message.ShouldContain("format is invalid");
            }
        }
    }
}
