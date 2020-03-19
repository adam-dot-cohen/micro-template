using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Domain;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence
{
    public static class DataQualityPipelineRepository
    {
        private static readonly ConcurrentDictionary<string, FileBatch> FileBatches = new ConcurrentDictionary<string, FileBatch>();
        private static readonly ConcurrentDictionary<string, PipelineRun> PipelineRuns = new ConcurrentDictionary<string, PipelineRun>();

        public static Task AddFileBatch(FileBatch fileBatch)
        {
            if (!FileBatches.TryAdd(fileBatch.Id, fileBatch))
                throw new Exception();

            return Task.CompletedTask;
        }

        public static Task<FileBatch> GetFileBatch(string id)
        {
            if (FileBatches.TryGetValue(id, out var fileBatch))
                return Task.FromResult(fileBatch);

            return Task.FromResult(default(FileBatch));
        }

        public static Task AddPipelineRun(PipelineRun pipelineRun)
        {
            if (!PipelineRuns.TryAdd(pipelineRun.Id, pipelineRun))
                throw new Exception();

            return Task.CompletedTask;
        }
    }
}
