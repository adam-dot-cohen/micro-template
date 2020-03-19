using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Domain;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence
{
    public static class DataQualityPipelineRepository
    {
        private static readonly ConcurrentDictionary<string, FileBatch> FileBatches = new ConcurrentDictionary<string, FileBatch>();
        private static readonly ConcurrentDictionary<string, PipelineRun> PipelineRuns = new ConcurrentDictionary<string, PipelineRun>();
        private static readonly ConcurrentDictionary<string, DataPipelineStatus> PipelineEvents = new ConcurrentDictionary<string, DataPipelineStatus>();

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

        public static Task<ICollection<PipelineRun>> GetPipelineRuns(string fileBatchId = null)
        {
            var pipelineRuns = (ICollection<PipelineRun>) PipelineRuns.Values
                .Where(x => fileBatchId == null || x.FileBatchId == fileBatchId)
                .ToList();

            return Task.FromResult(pipelineRuns);
        }

        public static Task AddPipelineEvent(DataPipelineStatus @event)
        {
            if (!PipelineEvents.TryAdd(Guid.NewGuid().ToString(), @event))
                throw new Exception();

            return Task.CompletedTask;
        }

        public static Task<ICollection<DataPipelineStatus>> GetPipelineEvents(string pipelineRunId = null)
        {
            var pipelineEvents = (ICollection<DataPipelineStatus>) PipelineEvents.Values
                .Where(x => pipelineRunId == null || x.OrchestrationId == pipelineRunId)
                .ToList();

            return Task.FromResult(pipelineEvents);
        }
    }
}
