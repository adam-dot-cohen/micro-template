using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.Mediation;
using Laso.Scheduling.Core.Experiments.Commands;
using Laso.Scheduling.Core.Experiments.Queries;
using Laso.Scheduling.Core.IntegrationEvents.Publish.Scheduling;
using Laso.Scheduling.Core.IntegrationEvents.Subscribe.CustomerData;
using MediatR;
using NSubstitute;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Laso.Scheduling.UnitTests.Core.Experiments.Commands
{
    public class SchedulePartnerExperimentHandlerTests
    {
        private static readonly InputBatchAcceptedEventV1 InputEvent = new InputBatchAcceptedEventV1("fileBatchId", DateTimeOffset.Now, "partnerId", "partnerName", new []
        {
            new BlobFileInfoV1("id", "uri", "contentType", 333, "eTag", "dataCategory", DateTimeOffset.Now, DateTimeOffset.Now) 
        });

        [Fact]
        public async Task When_getting_partner_config_fails_Should_fail()
        {
            // Arrange
            var mediator = Substitute.For<IMediator>();
            mediator.Send(Arg.Any<GetPartnerExperimentConfigurationQuery>())
                .Returns(QueryResponse.Failed<PartnerExperimentConfiguration>("badness"));
            var eventPublisher = Substitute.For<IEventPublisher>();
            var handler = new SchedulePartnerExperimentHandler(mediator, eventPublisher);
            var input = new SchedulePartnerExperimentCommand("partnerId", InputEvent);

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.Success.ShouldBeFalse();
            response.GetAllMessages().ShouldBe("badness");
            await eventPublisher.DidNotReceiveWithAnyArgs().Publish(Arg.Any<ExperimentRunScheduledEventV1>(), null);
        }

        [Fact(Skip="Disabled feature, for now.")]
        public async Task When_experiments_disabled_Should_not_publish_event()
        {
            // Arrange
            var mediator = Substitute.For<IMediator>();
            mediator.Send(Arg.Any<GetPartnerExperimentConfigurationQuery>())
                .Returns(QueryResponse.Succeeded(new PartnerExperimentConfiguration("partnerId") { ExperimentsEnabled = false }));
            var eventPublisher = Substitute.For<IEventPublisher>();
            var handler = new SchedulePartnerExperimentHandler(mediator, eventPublisher);
            var input = new SchedulePartnerExperimentCommand("partnerId", InputEvent);

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.Success.ShouldBeTrue();
            await eventPublisher.DidNotReceiveWithAnyArgs().Publish(Arg.Any<ExperimentRunScheduledEventV1>(), null);
        }

        [Fact]
        public async Task When_experiments_enabled_Should_publish_event()
        {
            // Arrange
            var mediator = Substitute.For<IMediator>();
            mediator.Send(Arg.Any<GetPartnerExperimentConfigurationQuery>())
                .Returns(QueryResponse.Succeeded(new PartnerExperimentConfiguration("partnerId") { ExperimentsEnabled = true }));
            var eventPublisher = Substitute.For<IEventPublisher>();
            var handler = new SchedulePartnerExperimentHandler(mediator, eventPublisher);
            var input = new SchedulePartnerExperimentCommand("partnerId", InputEvent);

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.Success.ShouldBeTrue();
            await eventPublisher.Received().Publish(Arg.Any<ExperimentRunScheduledEventV1>(), "scheduling");
        }
    }
}
