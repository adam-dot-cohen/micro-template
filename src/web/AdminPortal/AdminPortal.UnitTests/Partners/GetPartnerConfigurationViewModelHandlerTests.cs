using System;
using System.Linq;
using System.Threading;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.AdminPortal.Infrastructure.KeyVault;
using Laso.AdminPortal.Infrastructure.Partners.Queries;
using Shouldly;
using Xunit;

namespace Laso.AdminPortal.UnitTests.Partners
{
    public abstract class GetPartnerConfigurationViewModelHandlerTests
    {
        private readonly InMemoryApplicationSecrets _applicationSecrets;
        private readonly GetPartnerConfigurationViewModelQuery _query;
        private readonly GetPartnerConfigurationViewModelHandler _handler;

        protected GetPartnerConfigurationViewModelHandlerTests()
        {
            // Arrange
            _applicationSecrets = new InMemoryApplicationSecrets();
            
            _query = new GetPartnerConfigurationViewModelQuery { Id = Guid.NewGuid().ToString() };
            _handler = new GetPartnerConfigurationViewModelHandler(_applicationSecrets);
        }

        public class WhenCalledWithMissingConfiguration : GetPartnerConfigurationViewModelHandlerTests
        {
            private readonly QueryResponse<PartnerConfigurationViewModel> _response;

            public WhenCalledWithMissingConfiguration()
            {
                // Act
                _response = _handler.Handle(_query, CancellationToken.None).Result;
            }

            [Fact]
            public void Should_succeed()
            {
                _response.Success().ShouldBeTrue();
            }

            [Fact]
            public void Should_return_model()
            {
                var result = _response.Result;
                result.ShouldNotBe(null);
                result.Id.ShouldNotBeNull();
                result.Name.ShouldBeNullOrEmpty();
                result.Settings.Count.ShouldBe(6);
            }

            [Fact]
            public void Should_return_null_setting_values()
            {
                _response.Result.Settings.Select(s => s.Value).ShouldAllBe(v => v == null);
            }
        }

        public class WhenCalledWithConfiguration : GetPartnerConfigurationViewModelHandlerTests
        {
            private readonly QueryResponse<PartnerConfigurationViewModel> _response;

            public WhenCalledWithConfiguration()
            {
                // Arrange
                GetPartnerConfigurationViewModelHandler.PartnerConfigurationSettings.ForEach(
                    s => _applicationSecrets.Secrets
                        .TryAdd(
                            string.Format(s.KeyNameFormat, _query.Id),
                            new KeyVaultSecret
                            {
                                Version = "1",
                                Value = Guid.NewGuid().ToString()
                            })
                        .ShouldBeTrue());
                
                // Act
                _response = _handler.Handle(_query, CancellationToken.None).Result;
            }

            [Fact]
            public void Should_succeed()
            {
                _response.Success().ShouldBeTrue();
            }

            [Fact]
            public void Should_return_model()
            {
                var result = _response.Result;
                result.ShouldNotBe(null);
                result.Id.ShouldNotBeNull();
                result.Name.ShouldBeNullOrEmpty();
                result.Settings.Count(s => s.Value != null)
                    .ShouldBe(GetPartnerConfigurationViewModelHandler.PartnerConfigurationSettings.Count);
            }

            [Fact]
            public void Should_return_setting_values()
            {
                _response.Result.Settings.Select(s => s.Value).ShouldAllBe(v => v != null);
            }
        }
    }
}
