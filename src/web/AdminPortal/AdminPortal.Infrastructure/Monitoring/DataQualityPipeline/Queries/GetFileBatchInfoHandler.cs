using System;
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
        private const string DataCategoryName = "category";

        // Data category immediately precedes date. Allow digits in case we add numeric versioning later
        private static readonly string DataCategoryRegex = $@"^.+_(?<{DataCategoryName}>[a-zA-Z][a-zA-Z0-9]{{2,}})_\d{{8}}_\d{{9,}}\.[\.a-zA-Z]+$";

        public Task<QueryResponse<FileBatchInfo>> Handle(GetFileBatchInfoQuery query, CancellationToken cancellationToken)
        {
            var result = new FileBatchInfo();

            result.Files = query.FilePaths.Select(GetFileInfo).ToList();

            return Task.FromResult(QueryResponse.Succeeded(result));
        }

        private FileInfo GetFileInfo(string path)
        {
            var fileUri = new Uri(path);

            var fileInfo = new FileInfo
            {
                Path = path,
                PartnerId = GetPartnerId(fileUri),
                DataCategory = GetDataCategory(fileUri)
            };

            return fileInfo;
        }

        private static string GetPartnerId(Uri fileUri)
        {
            var partnerId = fileUri.Segments
                .SingleOrDefault(x => Guid.TryParse(x, out _));

            return partnerId?.ToLowerInvariant();
        }

        private string GetDataCategory(Uri fileUri)
        {
            var fileName = fileUri.Segments.LastOrDefault();

            if (string.IsNullOrEmpty(fileName) || fileName.EndsWith("/"))
            {
                throw new Exception($"Filename is not valid: {fileUri.AbsoluteUri}");
            }

            var match = Regex.Match(fileName, DataCategoryRegex);

            if (!match.Success)
            {
                throw new Exception($"Data category could not be extracted from file: {fileName}");
            }

            return match.Groups[DataCategoryName].Value;
        }
    }
}