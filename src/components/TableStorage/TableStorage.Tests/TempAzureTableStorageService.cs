using System;
using System.Linq;
using System.Threading.Tasks;
using Laso.TableStorage.Azure;
using Laso.TableStorage.Azure.PropertyColumnMappers;
using Laso.TableStorage.Tests.Extensions;

namespace Laso.TableStorage.Tests
{
    public class TempAzureTableStorageService : AzureTableStorageService, IAsyncDisposable
    {
        private readonly TempAzureTableStorageContext _context;

        public TempAzureTableStorageService(ISaveChangesDecorator[] saveChangesDecorators = null) : this(new TempAzureTableStorageContext(saveChangesDecorators)) { }
        private TempAzureTableStorageService(TempAzureTableStorageContext context) : base(context)
        {
            _context = context;
        }

        public ValueTask DisposeAsync()
        {
            return _context.DisposeAsync();
        }

        private class TempAzureTableStorageContext : AzureTableStorageContext, IAsyncDisposable
        {
            public TempAzureTableStorageContext(ISaveChangesDecorator[] saveChangesDecorators = null) : base(
                "DefaultEndpointsProtocol=http;AccountName=localhost;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;TableEndpoint=http://localhost:8902/;",
                //"AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                //"DefaultEndpointsProtocol=https;AccountName=uedevstorage;AccountKey=K0eMUJoAG5MmTigJX2NTYrRw3k0M6T9qrOIDZQBKOnmt+eTzCcdWoMkd6oUeP6yYriE1M5H6yMzzHo86KXcunQ==",
                Guid.NewGuid().Encode(IntegerEncoding.Base26),
                saveChangesDecorators,
                new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() }) { }

            public async ValueTask DisposeAsync()
            {
               GetTables().ToList().ForEach(DeleteTable);
               await Task.CompletedTask;
            }
        }
    }
}