using Laso.Identity.Core.Partners.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using System.Threading;
using Laso.Identity.Domain.Entities;
using Laso.TableStorage;
using Shouldly;

namespace Laso.Identity.UnitTests.Core.Partners
{
    public class DeletePartnerHandlerTests
    {
        [Fact]
        public async Task When_Partner_Not_Found_Should_Succeed()
        {
            var sut = new DeletePartnerHandler(Substitute.For<ITableStorageService>());
            var command = new DeletePartnerCommand {PartnerId = Guid.NewGuid().ToString()};
            var response = await sut.Handle(command, CancellationToken.None);
            response.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task When_Partner_Found_Should_Delete()
        {
            var partner = new Partner {Id = Guid.NewGuid().ToString()};
            var command = new DeletePartnerCommand {PartnerId = partner.Id};
            var storage = Substitute.For<ITableStorageService>();
            storage.GetAsync<Partner>(Arg.Any<string>()).Returns(Task.FromResult(partner));
            storage.DeleteAsync<Partner>(Arg.Any<Partner>()).Returns(Task.CompletedTask);
            var sut = new DeletePartnerHandler(storage);
            var response = await sut.Handle(command, CancellationToken.None);
            response.Success.ShouldBeTrue();
        }
    }

    public class DeletePartnerInputValidatorTests
    {
        [Fact]
        public void When_Empty_Should_Fail()
        {
            var sut = new DeletePartnerCommand();
            var result = sut.ValidateInput();

            result.Success.ShouldBeFalse();
            result.ValidationMessages.Single().Key.ShouldBe(nameof(sut.PartnerId));
        }

        [Fact]
        public void When_Not_Valid_Guid_Should_Fail()
        {
            var sut = new DeletePartnerCommand{ PartnerId = "lol"};
            var result = sut.ValidateInput();

            result.Success.ShouldBeFalse();
            result.ValidationMessages.Single().Key.ShouldBe(nameof(sut.PartnerId));
        }

        [Fact]
        public void When_Valid_Should_Succeed()
        {
            var id = Guid.NewGuid();
            var sut = new DeletePartnerCommand {PartnerId = id.ToString()};
            var result = sut.ValidateInput();

            result.Success.ShouldBeTrue();
        }
    }
}
