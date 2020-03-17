using System;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.Persistence.Azure;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;
using Laso.Logging.Extensions;

namespace Laso.Identity.IntegrationTests.Infrastructure.Persistence.Azure
{
    public class TempAzureTableStorageService : AzureTableStorageService, IDisposable
    {
        private readonly TempAzureTableStorageContext _context;

        public TempAzureTableStorageService(ISaveChangesDecorator[] saveChangesDecorators = null) : this(new TempAzureTableStorageContext(saveChangesDecorators)) { }
        private TempAzureTableStorageService(TempAzureTableStorageContext context) : base(context)
        {
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private class TempAzureTableStorageContext : AzureTableStorageContext, IDisposable
        {
            public TempAzureTableStorageContext(ISaveChangesDecorator[] saveChangesDecorators = null) : base(
                "DefaultEndpointsProtocol=https;AccountName=uedevstorage;AccountKey=K0eMUJoAG5MmTigJX2NTYrRw3k0M6T9qrOIDZQBKOnmt+eTzCcdWoMkd6oUeP6yYriE1M5H6yMzzHo86KXcunQ==",
                Guid.NewGuid().ToBytes().Encode(Encoding.Base26),
                saveChangesDecorators,
                new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() }) { }

            public void Dispose()
            {
                GetTables().ForEach(x => x.Delete());
            }
        }
    }
}