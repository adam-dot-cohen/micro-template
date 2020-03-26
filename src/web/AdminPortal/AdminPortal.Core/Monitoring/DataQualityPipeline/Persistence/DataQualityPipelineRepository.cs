using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Domain;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence
{
    public interface IDataQualityPipelineRepository
    {
        Task AddFileBatch(FileBatch fileBatch);
        Task<FileBatch> GetFileBatch(string fileBatchId);
        Task<ICollection<FileBatch>> GetFileBatches(string partnerId = null);
        Task AddPipelineRun(PipelineRun pipelineRun);
        Task<ICollection<PipelineRun>> GetPipelineRuns(string fileBatchId = null);
        Task AddPipelineEvent(DataPipelineStatus @event);
        Task AddFileBatchEvent(DataPipelineStatus @event);
        Task<ICollection<DataPipelineStatus>> GetPipelineEvents(string pipelineRunId = null);
        Task<ICollection<DataPipelineStatus>> GetFileBatchEvents(string fileBatchId = null);
    }

    public class InMemoryDataQualityPipelineRepository : IDataQualityPipelineRepository
    {
        private readonly ConcurrentDictionary<string, FileBatch> _fileBatches = new ConcurrentDictionary<string, FileBatch>();
        private readonly ConcurrentDictionary<string, PipelineRun> _pipelineRuns = new ConcurrentDictionary<string, PipelineRun>();
        private readonly ConcurrentDictionary<string, DataPipelineStatus> _pipelineEvents = new ConcurrentDictionary<string, DataPipelineStatus>();
        private readonly ConcurrentDictionary<string, DataPipelineStatus> _fileBatchEvents = new ConcurrentDictionary<string, DataPipelineStatus>();

        public Task AddFileBatch(FileBatch fileBatch)
        {
            if (!_fileBatches.TryAdd(fileBatch.Id, fileBatch))
                throw new Exception();

            return Task.CompletedTask;
        }

        public Task<FileBatch> GetFileBatch(string fileBatchId)
        {
            if (_fileBatches.TryGetValue(fileBatchId, out var fileBatch))
                return Task.FromResult(fileBatch);

            return Task.FromResult(default(FileBatch));
        }

        public Task<ICollection<FileBatch>> GetFileBatches(string partnerId = null)
        {
            var fileBatches = (ICollection<FileBatch>) _fileBatches.Values
                .Where(x => partnerId == null || x.PartnerId == partnerId)
                .ToList();

            return Task.FromResult(fileBatches);
        }

        public Task AddPipelineRun(PipelineRun pipelineRun)
        {
            if (!_pipelineRuns.TryAdd(pipelineRun.Id, pipelineRun))
                throw new Exception();

            return Task.CompletedTask;
        }

        public Task<ICollection<PipelineRun>> GetPipelineRuns(string fileBatchId = null)
        {
            var pipelineRuns = (ICollection<PipelineRun>) _pipelineRuns.Values
                .Where(x => fileBatchId == null || x.FileBatchId == fileBatchId)
                .ToList();

            return Task.FromResult(pipelineRuns);
        }

        public Task AddPipelineEvent(DataPipelineStatus @event)
        {
            if (!_pipelineEvents.TryAdd(Guid.NewGuid().ToString(), @event))
                throw new Exception();

            return Task.CompletedTask;
        }

        public Task AddFileBatchEvent(DataPipelineStatus @event)
        {
            if (!_fileBatchEvents.TryAdd(Guid.NewGuid().ToString(), @event))
                throw new Exception();

            return Task.CompletedTask;
        }

        public Task<ICollection<DataPipelineStatus>> GetPipelineEvents(string pipelineRunId = null)
        {
            var pipelineEvents = (ICollection<DataPipelineStatus>) _pipelineEvents.Values
                .Where(x => pipelineRunId == null || x.PipelineRunId == pipelineRunId)
                .ToList();

            return Task.FromResult(pipelineEvents);
        }

        public Task<ICollection<DataPipelineStatus>> GetFileBatchEvents(string fileBatchId = null)
        {
            var pipelineEvents = (ICollection<DataPipelineStatus>) _fileBatchEvents.Values
                .Where(x => fileBatchId == null || x.FileBatchId == fileBatchId)
                .ToList();

            return Task.FromResult(pipelineEvents);
        }
    }
}
