using System;
using System.Linq;
using System.Threading.Tasks;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.Persistence.Azure;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;

namespace Laso.Identity.IntegrationTests.Infrastructure.Persistence.Azure
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
                "DefaultEndpointsProtocol=https;AccountName=uedevstorage;AccountKey=K0eMUJoAG5MmTigJX2NTYrRw3k0M6T9qrOIDZQBKOnmt+eTzCcdWoMkd6oUeP6yYriE1M5H6yMzzHo86KXcunQ==",
                Guid.NewGuid().Encode(IntegerEncoding.Base26),
                saveChangesDecorators,
                new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() }) { }

            public async ValueTask DisposeAsync()
            {
                await Task.WhenAll(GetTables().Select(x => x.DeleteAsync()));
            }
        }
    }
}