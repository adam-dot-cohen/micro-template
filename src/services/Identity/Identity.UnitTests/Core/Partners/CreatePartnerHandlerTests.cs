using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.Identity.Core.IntegrationEvents;
using Laso.Identity.Core.Partners.Commands;
using Laso.Identity.Core.Persistence;
using Laso.Identity.Domain.Entities;
using Laso.IntegrationEvents;
using NSubstitute;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Laso.Identity.UnitTests.Core.Partners
{
    public class CreatePartnerHandlerTests
    {
        [Fact]
        public async Task When_partner_exists_Should_fail()
        {
            // Arrange
            var tableStorageService = Substitute.For<ITableStorageService>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var handler = new CreatePartnerHandler(tableStorageService, eventPublisher);
            var input = new CreatePartnerCommand
            {
                Name = "Partner Name"
            };
            tableStorageService.FindAllAsync<Partner>(null, 1)
                .ReturnsForAnyArgs(Task.FromResult<ICollection<Partner>>(new [] { new Partner { Name = input.Name } }));

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.Success().ShouldBeFalse();
            response.GetAllMessages().ShouldContain("already exists");
            await eventPublisher.DidNotReceiveWithAnyArgs().Publish<IIntegrationEvent>(null);
        }

        [Fact]
        public async Task When_partner_is_new_Should_create()
        {
            // Arrange
            const string partnerName = "Partner Name";
            const string partnerId = "Partner ID";
            var tableStorageService = Substitute.For<ITableStorageService>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var handler = new CreatePartnerHandler(tableStorageService, eventPublisher);
            var input = new CreatePartnerCommand
            {
                Name = partnerName
            };
            tableStorageService.FindAllAsync<Partner>(null, 1)
                .ReturnsForAnyArgs(Task.FromResult<ICollection<Partner>>(new Partner[0]));
            tableStorageService.When(x => x.InsertAsync(Arg.Any<Partner>()))
                .Do(ctx => ctx.Arg<Partner>().Id = partnerId);

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.Success().ShouldBeTrue();
            await tableStorageService.Received().InsertAsync(Arg.Any<Partner>());
            await eventPublisher.Received().Publish(Arg.Is<PartnerCreatedEventV1>(e => e.Id == partnerId));
        }

        [Fact]
        public async Task When_partner_is_created_Should_create_normalized_name()
        {
            // Arrange
            const string partnerName = "123 Partner Name #1, Inc";
            const string expectedNormalizedName = "partnername1inc";
            var tableStorageService = Substitute.For<ITableStorageService>();
            var eventPublisher = Substitute.For<IEventPublisher>();
            var handler = new CreatePartnerHandler(tableStorageService, eventPublisher);
            var input = new CreatePartnerCommand
            {
                Name = partnerName
            };
            tableStorageService.FindAllAsync<Partner>(null, 1)
                .ReturnsForAnyArgs(Task.FromResult<ICollection<Partner>>(new Partner[0]));

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.Success().ShouldBeTrue();
            await tableStorageService.Received().InsertAsync(Arg.Is<Partner>(p => p.NormalizedName == expectedNormalizedName));
        }
    }

    public class CreatePartnerInputValidatorTests
    {
        [Fact]
        public void When_valid_Should_succeed()
        {
            var command = new CreatePartnerCommand {Name = "Partner"};
            var result = command.ValidateInput();
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void When_invalid_Should_fail()
        {
            var command = new CreatePartnerCommand();
            var result = command.ValidateInput();
            result.IsValid.ShouldBeFalse();
            result.ValidationMessages.Single().Key.ShouldBe(nameof(command.Name));
        }
    }
}
