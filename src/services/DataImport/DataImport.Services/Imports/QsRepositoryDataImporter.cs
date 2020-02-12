using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using DataImport.Core.Matching;
using DataImport.Data.Quarterspot;
using DataImport.Domain.Api.Models;
using DataImport.Domain.Api.Quarterspot.Enumerations;
using DataImport.Domain.Api.Quarterspot.Models;
using DataImport.Services.IO;
using DataImport.Services.IO.Storage.Blob.Azure;
using DataImport.Core.Extensions;
using DataImport.Domain.Api;
using DataImport.Services.Partners;

namespace DataImport.Services.Imports
{
    using ImportMap = Dictionary<ImportType, Func<QsRepositoryDataImporter, ImportSubscription, Partner, Task>>;

    public class QsRepositoryDataImporter : IDataImporter
    {
        public PartnerIdentifier Partner => PartnerIdentifier.Quarterspot;

        private readonly IQuarterspotRepository _qsRepo;
        private readonly IDelimitedFileWriter _writer;
        private readonly IBlobStorageService _storage;        
        private readonly IPartnerService _partnerService;

        // ! if you add a new import function, map it here
        private readonly ImportMap ImportMap = new ImportMap
        {
            [ImportType.Demographic] = (x, s, p) => x.ImportDemographicsAsync(s, p),
            [ImportType.Firmographic] = (x, s, p) => x.ImportFirmographicsAsync(s, p),
            [ImportType.Account] = (x, s, p) => x.ImportAccountsAsync(s, p),
            [ImportType.AccountTransaction] = (x, s, p) => x.ImportAccountTransactionsAsync(s, p),
            [ImportType.LoanAccount] = (x, s, p) => x.ImportLoanAccountsAsync(s, p),
            [ImportType.LoanTransaction] = (x, s, p) => x.ImportLoanTransactionsAsync(s, p),
            [ImportType.LoanApplication] = (x, s, p) => x.ImportLoanApplicationsAsync(s, p),
            [ImportType.LoanCollateral] = (x, s, p) => x.ImportLoanCollateralAsync(s, p),
            [ImportType.LoanAttribute] = (x, s, p) => x.ImportLoanAttributesAsync(s, p)
        };        

        public QsRepositoryDataImporter(
            IQuarterspotRepository qsRepository, 
            IDelimitedFileWriter writer,
            IBlobStorageService storage,            
            IPartnerService partnerService)
        {
            _qsRepo = qsRepository;
            _writer = writer;
            _storage = storage;            
            _partnerService = partnerService;

            _writer.Configuration = new DelimitedFileConfiguration
            {
                BufferSize = 1024 * 16,
                Delimiter = ",",
                HasHeaderRecord = true,
                IgnoreExtraColumns = false,
                TypeConverterOptions = new List<TypeConverterOption>
                {
                    new TypeConverterOption
                    {
                        Type = typeof(DateTime),
                        Format = "M/d/yyyy"
                    }
                }
            };            
        }       

        public async Task ImportAsync(ImportSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));
            if (subscription.Imports.IsNullOrEmpty())
                throw new ArgumentException("No imports specified");

            var imports = subscription.Imports.Select(i => Enum.Parse<ImportType>(i));
            var partner = await _partnerService.GetAsync(subscription.PartnerId);
            var exceptions = new List<Exception>();

            foreach (var import in imports)
            {
                try
                {
                    if (ImportMap.TryGetValue(import, out var importFunc))
                        await importFunc(this, subscription, partner);
                    else
                        throw new ArgumentException($"value {import} ({(int) import}) has no mapping or is not defined", nameof(imports));
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        public Task ImportAccountsAsync(ImportSubscription subscription, Partner partner)
        {
            throw new NotImplementedException();
        }

        public Task ImportAccountTransactionsAsync(ImportSubscription subscription, Partner partner)
        {
            throw new NotImplementedException();
        }       

        public async Task ImportDemographicsAsync(ImportSubscription subscription, Partner partner)
        {
            static Demographic transform(IGrouping<string, QsCustomer> c)
            {
                var latest = c.OrderByDescending(s => s.CreditScoreEffectiveTime).First();
                return new Demographic
                {
                    CustomerId = latest.Id,
                    BranchId = null,
                    CreditScore = (int)latest.CreditScore,
                    EffectiveDate = latest.CreditScoreEffectiveTime.Date
                };
            };
            
            var fileName = GetFileName(subscription, partner.InternalIdentifier, ImportType.Demographic, DateTime.UtcNow);

            using var stream = _storage.OpenWrite(subscription.IncomingStorageLocation, fileName);
            _writer.Open(stream.Stream, Encoding.UTF8);

            var customers = await _qsRepo.GetCustomersAsync();

            // todo: GroupBy here because customer ID is not currently unique.
            // We need to be able to read encrypted strings and use the SSN to
            // generate a unique ID (or something else entirely). Also means
            // we can't use the paged interface yet.
            var demos = customers.GroupBy(c => c.Id).Select(transform);
            _writer.WriteRecords(demos);

            //var offset = 0;
            //var customers = await _qsRepo.GetCustomersAsync(offset, BatchSize);

            //while (customers.Count() > 0)
            //{
            //    var demos = customers.Select(transform);

            //    _writer.WriteRecords(demos);

            //    offset += customers.Count();
            //    customers = await _qsRepo.GetCustomersAsync(offset, BatchSize);                
            //}                       
        }

        public async Task ImportFirmographicsAsync(ImportSubscription subscription, Partner partner)
        {
            var asOfDate = DateTime.UtcNow;

            Firmographic transform(QsBusiness r) => new Firmographic
            {
                // todo(ed): need unique customer ID
                CustomerId = null,
                BusinessId = r.Id.ToString(),
                EffectiveDate = asOfDate,
                DateStarted = r.Established,
                IndustryNaics = r.IndustryNaicsCode.ToString(),
                IndustrySic = r.IndustrySicCode.ToString(),
                BusinessType = r.BusinessEntityType != null ? BusinessEntityType.FromValue(r.BusinessEntityType.Value).DisplayName : null,
                LegalBusinessName = r.LegalName,
                BusinessPhone = NormalizationMethod.Phone10(r.Phone),
                BusinessEin = NormalizationMethod.TaxId(r.TaxId),
                PostalCode = NormalizationMethod.Zip5(r.Zip)
            };

            var offset = 0;
            var businesses = await _qsRepo.GetBusinessesAsync(offset, BatchSize);

            var container = subscription.IncomingStorageLocation;
            var fileName = GetFileName(subscription, partner.InternalIdentifier, ImportType.Firmographic, DateTime.UtcNow);

            using var stream = _storage.OpenWrite(container, fileName);
            _writer.Open(stream.Stream, Encoding.UTF8);

            while (businesses.Count() > 0)
            {
                var firmographics = businesses.Select(transform);

                _writer.WriteRecords(firmographics);

                offset += businesses.Count();
                businesses = await _qsRepo.GetBusinessesAsync(offset, BatchSize);
            }
        }

        public Task ImportLoanApplicationsAsync(ImportSubscription subscription, Partner partner)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanAttributesAsync(ImportSubscription subscription, Partner partner)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanCollateralAsync(ImportSubscription subscription, Partner partner)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanAccountsAsync(ImportSubscription subscription, Partner partner)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanTransactionsAsync(ImportSubscription subscription, Partner partner)
        {
            throw new NotImplementedException();
        }

        private static readonly int BatchSize = 10_000;

        public string GetFileName(ImportSubscription sub, PartnerIdentifier exportedFrom, ImportType type, DateTime effectiveDate)
        {
            // hopefully we'll just be creating a manifest down the road
            var frequency = Enum.Parse<ImportFrequency>(sub.Frequency);
            return $"{exportedFrom}_{PartnerIdentifier.Laso}_{frequency.ShortName()}_{type}_{effectiveDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMdd}.csv";
        }
    }
}
