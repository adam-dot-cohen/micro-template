using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.AdminPortal.Infrastructure.Partners.Queries;
using Laso.Identity.Api.V1;
using Laso.Mediation;
using Laso.Provisioning.Api.V1;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Laso.AdminPortal.UnitTests.Partners
{
    public abstract class GetPartnerConfigurationViewModelHandlerTests
    {
        protected GetPartnerResourcesReply _resourceReply;
        protected IHostEnvironment _hostEnvironment;
        private readonly GetPartnerConfigurationViewModelQuery _query;
        private readonly GetPartnerConfigurationViewModelHandler _handler;

        protected GetPartnerConfigurationViewModelHandlerTests()
        {
            // Arrange
            var partnersClient = Substitute.For<Identity.Api.V1.Partners.PartnersClient>();
            var provisioningClient = Substitute.For<Provisioning.Api.V1.Partners.PartnersClient>();

            var reply = new GetPartnerReply
            {
                Partner = new PartnerView()
            };

            partnersClient.GetPartnerAsync(Arg.Any<GetPartnerRequest>())
                .Returns(reply.AsGrpcCall());

            _resourceReply = new GetPartnerResourcesReply();

            provisioningClient.GetPartnerResourcesAsync(Arg.Any<GetPartnerResourcesRequest>())
                .Returns(_resourceReply.AsGrpcCall());

            _hostEnvironment = Substitute.For<IHostEnvironment>();
            _query = new GetPartnerConfigurationViewModelQuery { Id = Guid.NewGuid().ToString() };
            _handler = new GetPartnerConfigurationViewModelHandler(partnersClient, provisioningClient, _hostEnvironment);
        }

        public class WhenCalledWithMissingConfiguration : GetPartnerConfigurationViewModelHandlerTests
        {
            private readonly QueryResponse<PartnerConfigurationViewModel> _response;

            public WhenCalledWithMissingConfiguration()
            {
                _hostEnvironment.EnvironmentName = "Test";
                // Act
                _response = _handler.Handle(_query, CancellationToken.None).Result;
            }

            [Fact]
            public void Should_succeed()
            {
                _response.Success.ShouldBeTrue();
            }

            [Fact]
            public void Should_return_model()
            {
                var result = _response.Result;
                result.ShouldNotBe(null);
                result.Id.ShouldNotBeNull();
                result.Name.ShouldBeNullOrEmpty();
                result.CanDelete.ShouldBeTrue();
                result.Settings.Count.ShouldBe(0);
            }
        }

        public class WhenCalledWithConfiguration : GetPartnerConfigurationViewModelHandlerTests
        {
            private readonly QueryResponse<PartnerConfigurationViewModel> _response;

            public WhenCalledWithConfiguration()
            {
                // Arrange
                _hostEnvironment.EnvironmentName = "Test";
                var resources = new List<PartnerResourceView>
                {
                    new PartnerResourceView { Name = "Test", DisplayValue = "test" },
                    new PartnerResourceView { Name = "Test1", DisplayValue = "test1" },
                    new PartnerResourceView { Name = "Test2", DisplayValue = "test2" },
                };
                _resourceReply.Resources.AddRange(resources);
                
                // Act
                _response = _handler.Handle(_query, CancellationToken.None).Result;
            }

            [Fact]
            public void Should_succeed()
            {
                _response.Success.ShouldBeTrue();
            }

            [Fact]
            public void Should_return_model()
            {
                var result = _response.Result;
                result.ShouldNotBe(null);
                result.Id.ShouldNotBeNull();
                result.Name.ShouldBeNullOrEmpty();
                result.CanDelete.ShouldBeTrue();
                result.Settings.Count(s => s.Value != null)
                    .ShouldBe(_resourceReply.Resources.Count);
            }

            [Fact]
            public void Should_return_setting_values()
            {
                _response.Result.Settings.Select(s => s.Value).ShouldAllBe(v => v != null);
            }
        }

        public class WhenCalledInProduction : GetPartnerConfigurationViewModelHandlerTests
        {
            private readonly QueryResponse<PartnerConfigurationViewModel> _response;

            public WhenCalledInProduction()
            {
                _hostEnvironment.EnvironmentName = "Production";
                // Act
                _response = _handler.Handle(_query, CancellationToken.None).Result;
            }

            [Fact]
            public void Should_succeed()
            {
                _response.Success.ShouldBeTrue();
            }

            [Fact]
            public void Should_return_model()
            {
                var result = _response.Result;
                result.ShouldNotBe(null);
                result.Id.ShouldNotBeNull();
                result.Name.ShouldBeNullOrEmpty();
                result.CanDelete.ShouldBeFalse();
                result.Settings.Count.ShouldBe(0);
            }

        }
    }
}
