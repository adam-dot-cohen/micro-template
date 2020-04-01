using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Queries
{
    public class GetFileBatchInfoHandler : IQueryHandler<GetFileBatchInfoQuery, FileBatchInfo>
    {
        private const string Frequency = "Frequency";
        private const string DataCategory = "Category";
        private const string EffectiveDate = "EffectiveDate";
        private const string TransmissionTime = "TransmissionTime";
        private const string FileTypeExtension = "FileExtension";
        private const string FileTransformExtensions = "FileTransforms";

        private const string DateTimeFormat = "yyyyMMddHHmmss";

        private static readonly string FileComponentsRegex =
            @"^" 
            + @".+" // ignoring partner info for now since it is derived by folder and not file
            + $@"_(?<{Frequency}>[DdWwMmQqYyRr]{{1}})" // Daily, Weekly, Monthly, Quarterly, Yearly, On Request
            + $@"_(?<{DataCategory}>[a-zA-Z][a-zA-Z0-9]+)" // Allow digits in case we add numeric versioning later
            + $@"_(?<{EffectiveDate}>\d{{4,8}})"
            + $@"_(?<{TransmissionTime}>\d{{8,14}})" 
            + $@"\.(?<{FileTypeExtension}>[0-9a-zA-Z]+)"
            + $@"(?<{FileTransformExtensions}>[\.0-9a-zA-Z]*)"
            + @"$"
            ;

        public Task<QueryResponse<FileBatchInfo>> Handle(GetFileBatchInfoQuery query, CancellationToken cancellationToken)
        {
            var result = new FileBatchInfo();

            var validationMessages = new List<ValidationMessage>();
            result.Files = query.FilePaths
                .Select(s => GetFileInfo(s, validationMessages))
                .ToList();

            if (validationMessages.Any())
            {
                return Task.FromResult(new QueryResponse<FileBatchInfo>
                {
                    IsValid = false,
                    ValidationMessages = validationMessages
                });
            }

            return Task.FromResult(QueryResponse.Succeeded(result));
        }

        private FileInfo GetFileInfo(string path, List<ValidationMessage> validationMessages)
        {
            var fileUri = new Uri(path);
            var filename = fileUri.Segments.LastOrDefault();

            if (string.IsNullOrEmpty(filename) || filename.EndsWith("/"))
            {
                validationMessages.Add(new ValidationMessage(nameof(filename), $"Filename not found in path: {fileUri.AbsoluteUri}"));
                return null;
            }

            var partnerId = GetPartnerId(fileUri);
            if (string.IsNullOrEmpty(partnerId))
            {
                validationMessages.Add(new ValidationMessage(nameof(path), $"Partner ID not found in path: {path}"));
                return null;
            }

            var match = Regex.Match(filename, FileComponentsRegex);
            if (!match.Success)
            {
                validationMessages.Add(new ValidationMessage(nameof(filename), $"Filename format is invalid: {filename}"));
                return null;
            }

            var fileInfo = new FileInfo
            {
                Path = path,
                Filename = filename,
                PartnerId = partnerId,
                Frequency = match.Groups[Frequency].Value.ToUpperInvariant(),
                DataCategory = match.Groups[DataCategory].Value,
                EffectiveDate = GetDateTime(match.Groups[EffectiveDate].Value, validationMessages),
                TransmissionTime = GetDateTime(match.Groups[TransmissionTime].Value, validationMessages)
            };

            return fileInfo;
        }

        private static string GetPartnerId(Uri fileUri)
        {
            var partnerId = fileUri.Segments
                .Select(s => s.Replace("/", string.Empty))
                .SingleOrDefault(x => Guid.TryParse(x, out _));

            return partnerId?.ToLowerInvariant();
        }

        private static DateTimeOffset GetDateTime(string value, List<ValidationMessage> validationMessages)
        {
            if (value.Length % 2 != 0) // Each date/time component should be an even number of digits
            {
                validationMessages.Add(new ValidationMessage("datetime", $"Date and/or time could not be parsed from value: {value}"));
            }

            var format = DateTimeFormat.Substring(0, value.Length);
            return DateTimeOffset.ParseExact(value, format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
        }
    }
}