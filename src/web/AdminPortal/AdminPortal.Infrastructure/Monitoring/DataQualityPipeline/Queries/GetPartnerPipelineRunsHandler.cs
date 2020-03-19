using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Queries
{
    public class GetPartnerPipelineRunsHandler : IQueryHandler<GetPartnerPipelineRunsQuery, PartnerPipelineRunsViewModel>
    {
        public Task<QueryResponse<PartnerPipelineRunsViewModel>> Handle(GetPartnerPipelineRunsQuery query, CancellationToken cancellationToken)
        {
            var model = new PartnerPipelineRunsViewModel
            {
                PartnerId = query.PartnerId,
                PartnerName = "Partner Name",
                PipelineRuns = new []
                {
                    new PipelineRunViewModel
                    {
                        RunId = Guid.NewGuid().ToString(),
                        Statuses = new []
                        {
                            new PipelineRunStatusViewModel
                            {
                                Status = "Status1",
                                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30)
                            },
                            new PipelineRunStatusViewModel
                            {
                                Status = "Status2",
                                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-20)
                            }
                        }.OrderByDescending(s => s.Timestamp).ToList()
                    },
                    new PipelineRunViewModel
                    {
                        RunId = Guid.NewGuid().ToString(),
                        Statuses = new []
                        {
                            new PipelineRunStatusViewModel
                            {
                                Status = "Status1",
                                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-90)
                            },
                            new PipelineRunStatusViewModel
                            {
                                Status = "Status2",
                                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-80)
                            },
                        }.OrderByDescending(s => s.Timestamp).ToList()
                    }
                }.ToList()
            };

            return Task.FromResult(QueryResponse.Succeeded(model));
        }
    }
}