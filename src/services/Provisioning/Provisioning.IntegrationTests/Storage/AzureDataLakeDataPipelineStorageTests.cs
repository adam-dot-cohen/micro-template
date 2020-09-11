using System;
using System.Threading;
using Laso.Provisioning.Infrastructure;
using Laso.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Laso.Provisioning.IntegrationTests.Storage
{
    [Trait("Capability", "Managed Identity")]   // NOTE: Currently, this is required via configuration.
    public abstract class AzureDataLakeDataPipelineStorageTests : FunctionalTestBase<Laso.Provisioning.Api.Program>
    {
        public class When_CreateFileSystem_Called : AzureDataLakeDataPipelineStorageTests
        {
            private readonly AzureDataLakeDataPipelineStorage _storage;

            private readonly string _fileSystemName;
            private readonly bool _fileSystemCreated;

            public When_CreateFileSystem_Called()
            {
                _storage = Services.GetRequiredService<AzureDataLakeDataPipelineStorage>();

                _fileSystemName = Guid.NewGuid().ToString();
                _fileSystemCreated = false;

                _storage.CreateFileSystem(_fileSystemName, CancellationToken.None).Wait();
                _fileSystemCreated = true;
            }

            ~When_CreateFileSystem_Called()
            {
                if (_fileSystemCreated)
                    _storage.DeleteFileSystem(_fileSystemName, CancellationToken.None).Wait();
            }

            [Fact]
            public void Should_Create()
            {
                _fileSystemCreated.ShouldBeTrue();
            }
        }

        public class When_CreateDirectory_Called : AzureDataLakeDataPipelineStorageTests
        {
            private readonly AzureDataLakeDataPipelineStorage _storage;

            private readonly string _fileSystemName = Guid.NewGuid().ToString();
            private readonly bool _fileSystemCreated;
            private readonly string _directoryName = Guid.NewGuid().ToString();
            private readonly bool _directoryCreated;

            public When_CreateDirectory_Called()
            {
                _storage = Services.GetRequiredService<AzureDataLakeDataPipelineStorage>();

                _fileSystemCreated = false;
                _directoryCreated = false;

                _storage.CreateFileSystem(_fileSystemName, CancellationToken.None).Wait();
                _fileSystemCreated = true;

                _storage.CreateDirectory(_fileSystemName, _directoryName, CancellationToken.None).Wait();
                _directoryCreated = true;
            }

            ~When_CreateDirectory_Called()
            {
                if (_fileSystemCreated)
                    _storage.DeleteFileSystem(_fileSystemName, CancellationToken.None).Wait();
            }

            [Fact]
            public void Should_Create()
            {
                _directoryCreated.ShouldBeTrue();
                _fileSystemCreated.ShouldBeTrue();
            }
        }
    }
}
