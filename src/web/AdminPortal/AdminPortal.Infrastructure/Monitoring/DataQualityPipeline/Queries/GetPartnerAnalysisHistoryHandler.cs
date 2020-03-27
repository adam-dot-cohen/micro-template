using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Domain;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Queries
{
    public class GetPartnerAnalysisHistoryHandler : IQueryHandler<GetPartnerAnalysisHistoryQuery, PartnerAnalysisHistoryViewModel>
    {
        private IDataQualityPipelineRepository _repository;

        private static readonly IDictionary<string, string> ProductNameLookup =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["AccountTransaction"] = "Bank Transaction Analysis",
                ["Demographic"] = "Demographic Analysis"
            };

        public GetPartnerAnalysisHistoryHandler(IDataQualityPipelineRepository repository)
        {
            _repository = repository;
        }

        public async Task<QueryResponse<PartnerAnalysisHistoryViewModel>> Handle(GetPartnerAnalysisHistoryQuery query, CancellationToken cancellationToken)
        {
            var fileBatches = await _repository.GetFileBatches(query.PartnerId);

            // if (!fileBatches.Any())
            // {
            //     await UseFakeRepo(query.PartnerId);
            //     fileBatches = await _repository.GetFileBatches(query.PartnerId);
            // }

            var model = new PartnerAnalysisHistoryViewModel
            {
                PartnerId = query.PartnerId,
                PartnerName = "Partner"
            };

            foreach (var fileBatch in fileBatches)
            {
                var batchStatuses = await _repository.GetFileBatchEvents(fileBatch.Id);
                var lastStatus = batchStatuses.OrderBy(s => s.Timestamp).Last();
                var pipelineRuns = await _repository.GetPipelineRuns(fileBatch.Id);

                var pipelineRunEvents = new Dictionary<string, ICollection<DataPipelineStatus>>();
                foreach (var pipelineRun in pipelineRuns)
                {
                    var events = await _repository.GetPipelineEvents(pipelineRun.Id);
                    pipelineRunEvents.Add(pipelineRun.Id, events);
                }

                var batch = new FileBatchViewModel
                {
                    FileBatchId = fileBatch.Id,
                    Created = fileBatch.Created,
                    Updated = lastStatus.Timestamp,
                    Status = lastStatus.Stage ?? lastStatus.EventType,
                    Files = fileBatch.Files.Select(f =>
                        new FileBatchFileViewModel
                        {
                            Id = f.Id,
                            DataCategory = f.DataCategory,
                            ContentLength = f.ContentLength,
                            Filename = GetFileName(f.Uri)
                        })
                        .OrderBy(m => m.DataCategory)
                        .ToList(),
                    ProductAnalysisRuns = pipelineRuns.Select(r =>
                        new ProductAnalysisViewModel
                        {
                            PipelineRunId = r.Id,
                            ProductName = ProductNameLookup[r.Files.First().DataCategory],
                            Requested = r.Created,
                            Statuses = pipelineRunEvents[r.Id].Select(e =>
                                new AnalysisStatusViewModel
                                {
                                    CorrelationId = e.CorrelationId,
                                    Timestamp = e.Timestamp,
                                    Status = e.Stage ?? e.EventType,
                                    DataCategory = e.Body?.Document?.DataCategory ?? "N/A"
                                })
                                .OrderByDescending(e => e.Timestamp)
                                .ToList()
                        })
                        .OrderBy(m => m.ProductName)
                        .ToList()
                };
                model.FileBatches.Add(batch);
            }

            if (fileBatches.Any())
            {
                model.PartnerName = fileBatches.First().PartnerName;
            }

            return QueryResponse.Succeeded(model);
        }

        private string GetFileName(string url)
        {
            var uri = new Uri(url);
            return Path.GetFileName(uri.LocalPath);
        }

        private async Task UseFakeRepo(string partnerId)
        {
            var repo = new InMemoryDataQualityPipelineRepository();
            var blobFiles = new[]
            {
                new BlobFile
                {
                    DataCategory = "AccountTransaction",
                    Uri = "https://lasodevinsightsescrow.blob.core.windows.net/93383d2d-07fd-488f-938b-f9ce1960fee3/incoming/SterlingNational_Laso_R_AccountTransaction_11107019_11107019095900.csv",
                    ContentLength = 32232L
                },
                new BlobFile
                {
                    DataCategory = "Demographic",
                    Uri = "https://lasodevinsightsescrow.blob.core.windows.net/93383d2d-07fd-488f-938b-f9ce1960fee3/incoming/SterlingNational_Laso_R_Demographic_11107019_11107019095900.csv",
                    ContentLength = 23232L
                }
            }.ToList();

            var fileBatches = new[]
            {
                // Static ID's make UI work better on refresh
                new FileBatch
                {
                    Id = "078ecd53-d8ff-476e-8d79-1dca222caa05",
                    PartnerId = partnerId,
                    PartnerName = "Test Partner Name",
                    Files = blobFiles
                },
                new FileBatch
                {
                    Id = "d6589c89-4503-4331-b511-dc5656c3bbe8",
                    PartnerId = partnerId,
                    PartnerName = "Test Partner Name",
                    Files = blobFiles
                }
            };
            foreach (var fileBatch in fileBatches)
            {
                await repo.AddFileBatch(fileBatch);
            }

            var pipelineRuns = new[]
            {
                new PipelineRun
                {
                    Id = "098bb404-2bad-4a70-8faf-2a7060bf7ed3",
                    PartnerId = partnerId,
                    FileBatchId = fileBatches[0].Id,
                    Files = new[] { blobFiles[0] }.ToList()
                },
                new PipelineRun
                {
                    Id = "7533cda3-5db8-4085-b697-cd61d309ac50",
                    PartnerId = partnerId,
                    FileBatchId = fileBatches[0].Id,
                    Files = new[] { blobFiles[1] }.ToList()
                },
                new PipelineRun
                {
                    Id = "17828e41-2d54-490b-9140-bf5b4998fe5b",
                    PartnerId = partnerId,
                    FileBatchId = fileBatches[1].Id,
                    Files = new[] { blobFiles[0] }.ToList()
                },
                new PipelineRun
                {
                    Id = "004ea805-41db-4ea4-80ad-993e17c33425",
                    PartnerId = partnerId,
                    FileBatchId = fileBatches[1].Id,
                    Files = new[] { blobFiles[1] }.ToList()
                }
            };

            foreach (var pipelineRun in pipelineRuns)
            {
                await repo.AddPipelineRun(pipelineRun);

                var pipelineEvents = new[]
                {
                    new DataPipelineStatus
                    {
                        CorrelationId = pipelineRun.Id,
                        PartnerId = partnerId,
                        EventType = "DataPipelineStatus",
                        PartnerName = "Test Partner Name",
                        Stage = "Requested",
                        Timestamp = DateTimeOffset.UtcNow.AddMinutes(-85),
                    },
                    new DataPipelineStatus
                    {
                        CorrelationId = pipelineRun.Id,
                        PartnerId = partnerId,
                        EventType = "DataPipelineStatus",
                        OrchestrationId = pipelineRun.Id,
                        PartnerName = "Test Partner Name",
                        Stage = "Processing",
                        Timestamp = DateTimeOffset.UtcNow.AddMinutes(-40),
                        Body = new DataPipelineStatusBody
                        {
                            Document = new DataPipelineStatusDocument
                            {
                                Id = pipelineRun.Files[0].Id,
                                DataCategory = pipelineRun.Files[0].DataCategory,
                                Uri = pipelineRun.Files[0].Uri,
                                Etag = pipelineRun.Files[0].ETag
                            }
                        }
                    },
                    new DataPipelineStatus
                    {
                        CorrelationId = pipelineRun.Id,
                        PartnerId = partnerId,
                        EventType = "DataPipelineStatus",
                        OrchestrationId = pipelineRun.Id,
                        PartnerName = "Test Partner Name",
                        Stage = "Still Processing",
                        Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30),
                        Body = new DataPipelineStatusBody
                        {
                            Document = new DataPipelineStatusDocument
                            {
                                Id = pipelineRun.Files[0].Id,
                                DataCategory = pipelineRun.Files[0].DataCategory,
                                Uri = pipelineRun.Files[0].Uri,
                                Etag = pipelineRun.Files[0].ETag
                            }
                        }
                    }
                };
                foreach (var pipelineEvent in pipelineEvents)
                {
                    await repo.AddPipelineEvent(pipelineEvent);
                }
            }

            foreach (var fileBatch in fileBatches)
            {
                var fileBatchEvents = new[]
                {
                    new DataPipelineStatus
                    {
                        CorrelationId = fileBatch.Id,
                        PartnerId = partnerId,
                        EventType = "DataAccepted",
                        OrchestrationId = fileBatch.Id,
                        PartnerName = "Test Partner Name",
                        Stage = "PartnerFilesReceived",
                        Timestamp = DateTimeOffset.UtcNow.AddMinutes(-40)
                    },
                    new DataPipelineStatus
                    {
                        CorrelationId = fileBatch.Id,
                        PartnerId = partnerId,
                        EventType = "DataAccepted",
                        OrchestrationId = fileBatch.Id,
                        PartnerName = "Test Partner Name",
                        // Stage = "Still Processing",
                        Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30),
                        Body = new DataPipelineStatusBody
                        {
                            Document = new DataPipelineStatusDocument
                            {
                                Id = fileBatch.Files[0].Id,
                                DataCategory = fileBatch.Files[0].DataCategory,
                                Uri = fileBatch.Files[0].Uri,
                                Etag = fileBatch.Files[0].ETag
                            }
                        }
                    }
                };
                foreach (var fileBatchEvent in fileBatchEvents)
                {
                    await repo.AddFileBatchEvent(fileBatchEvent);
                }
            }

            _repository = repo;
        }
    }
}