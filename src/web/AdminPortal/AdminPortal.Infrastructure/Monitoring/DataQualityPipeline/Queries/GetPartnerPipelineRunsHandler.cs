using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Domain;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;
using Repo = Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence.DataQualityPipelineRepository;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Queries
{
    public class GetPartnerPipelineRunsHandler : IQueryHandler<GetPartnerPipelineRunsQuery, PartnerPipelineRunsViewModel>
    {
        public async Task<QueryResponse<PartnerPipelineRunsViewModel>> Handle(GetPartnerPipelineRunsQuery query, CancellationToken cancellationToken)
        {
            var fileBatches = await Repo.GetFileBatches();
            fileBatches = fileBatches.Where(b => b.PartnerId == query.PartnerId).ToList();

            var pipelineRuns = await Repo.GetPipelineRuns();
            pipelineRuns = pipelineRuns.Where(r => r.PartnerId == query.PartnerId).ToList();

            var pipelineStatusEvents = new Dictionary<string, ICollection<DataPipelineStatus>>();
            foreach (var run in pipelineRuns)
            {
                // var pipelineEvents = await Repo.GetPipelineEvents(run.Id);
                // pipelineStatusEvents.Add(run.Id, pipelineEvents);
            }

            if (!pipelineRuns.Any())
            {
                // Dummy data for testing the UI
                (fileBatches, pipelineRuns, pipelineStatusEvents) = GetTestData(query.PartnerId);
            }

            // var partnerRuns =
            //     pipelineRuns.Join(fileBatches, r => r.PartnerId, b => b.PartnerId,
            //             (r, b) => new
            //             {
            //                 Detail = r,
            //                 Batch = b
            //             })
            //         .ToList();
            var partnerRuns =
                pipelineRuns.Join(fileBatches, r => r.FileBatchId, b => b.Id,
                        (r, b) => new
                        {
                            Detail = r,
                            Batch = b
                        })
                    .ToList();

            var model = new PartnerPipelineRunsViewModel
            {
                PartnerId = query.PartnerId,
                PartnerName = partnerRuns.FirstOrDefault()?.Batch?.PartnerName ?? "Partner",
                PipelineRuns = partnerRuns.Select(r =>
                    new PipelineRunViewModel
                    {
                        RunId = r.Detail.Id,
                        Statuses = pipelineStatusEvents[r.Detail.Id].Select(s =>
                                new PipelineRunStatusViewModel
                                {
                                    Status = s.Stage,
                                    Timestamp = s.Timestamp,
                                    FileDataCategory = r.Batch.Files.FirstOrDefault()?.DataCategory
                                }
                            )
                        .OrderByDescending(s => s.Timestamp).ToList()
                    }
                    ).ToList()
            };

            return QueryResponse.Succeeded(model);
        }

        private (ICollection<FileBatch> fileBatches, ICollection<PipelineRun> pipelines, Dictionary<string, ICollection<DataPipelineStatus>> pipelineStatusEvents)
            GetTestData(string partnerId)
        {
            var fileBatches = new[]
            {
                new FileBatch
                {
                    PartnerId = partnerId,
                    PartnerName = "Test Partner Name",
                    Files = new[]
                    {
                        new BlobFile
                        {
                            DataCategory = "AccountTransaction"
                        }
                    }.ToList()
                },
                new FileBatch
                {
                    PartnerId = partnerId,
                    PartnerName = "Test Partner Name",
                    Files = new[]
                    {
                        new BlobFile
                        {
                            DataCategory = "Demographic"
                        }
                    }.ToList()
                }
            };

            var pipelines = new[]
            {
                new PipelineRun
                {
                    Id = Guid.NewGuid().ToString(),
                    PartnerId = partnerId,
                    FileBatchId = fileBatches[0].Id
                },
                new PipelineRun
                {
                    Id = Guid.NewGuid().ToString(),
                    PartnerId = partnerId,
                    FileBatchId = fileBatches[0].Id
                },
                new PipelineRun
                {
                    Id = Guid.NewGuid().ToString(),
                    PartnerId = partnerId,
                    FileBatchId = fileBatches[1].Id
                },
                new PipelineRun
                {
                    Id = Guid.NewGuid().ToString(),
                    PartnerId = partnerId,
                    FileBatchId = fileBatches[1].Id
                }
            };

            var pipelineStatusEvents = new Dictionary<string, ICollection<DataPipelineStatus>>();
            foreach (var run in pipelines)
            {
                var pipelineEvents = new[]
                {
                    new DataPipelineStatus
                    {
                        PartnerId = partnerId,
                        EventType = "DataPipelineStatus",
                        OrchestrationId = run.Id,
                        PartnerName = "Test Partner Name",
                        Stage = "Processing",
                        Timestamp = DateTimeOffset.UtcNow.AddMinutes(-40)
                    },
                    new DataPipelineStatus
                    {
                        PartnerId = partnerId,
                        EventType = "DataPipelineStatus",
                        OrchestrationId = run.Id,
                        PartnerName = "Test Partner Name",
                        Stage = "Still Processing",
                        Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30)
                    }
                };
                pipelineStatusEvents.Add(run.Id, pipelineEvents);
            }

            return (fileBatches, pipelines, pipelineStatusEvents);

        }
    }
}