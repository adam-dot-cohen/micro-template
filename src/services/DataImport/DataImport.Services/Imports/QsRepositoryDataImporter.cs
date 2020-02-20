using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Laso.DataImport.Core.Matching;
using Laso.DataImport.Data.Quarterspot;
using Laso.DataImport.Domain.Models;
using Laso.DataImport.Domain.Quarterspot.Enumerations;
using Laso.DataImport.Domain.Quarterspot.Models;
using Laso.DataImport.Services.IO;
using Laso.DataImport.Services.IO.Storage.Blob.Azure;
using Laso.DataImport.Core.Extensions;
using Laso.DataImport.Services.DTOs;

namespace Laso.DataImport.Services
{
    using ImportMap = Dictionary<ImportTypeDto, Func<QsRepositoryDataImporter, ImportSubscriptionDto, PartnerDto, Task>>;

    public class QsRepositoryDataImporter : IDataImporter
    {
        public PartnerIdentifierDto Partner => PartnerIdentifierDto.Quarterspot;

        private readonly IQuarterspotRepository _qsRepo;
        private readonly IDelimitedFileWriter _writer;
        private readonly IBlobStorageService _storage;        
        private readonly IPartnerService _partnerService;

        // ! if you add a new import function, map it here
        private readonly ImportMap ImportMap = new ImportMap
        {
            [ImportTypeDto.Demographic] = (x, s, p) => x.ImportDemographicsAsync(s, p),
            [ImportTypeDto.Firmographic] = (x, s, p) => x.ImportFirmographicsAsync(s, p),
            [ImportTypeDto.Account] = (x, s, p) => x.ImportAccountsAsync(s, p),
            [ImportTypeDto.AccountTransaction] = (x, s, p) => x.ImportAccountTransactionsAsync(s, p),
            [ImportTypeDto.LoanAccount] = (x, s, p) => x.ImportLoanAccountsAsync(s, p),
            [ImportTypeDto.LoanTransaction] = (x, s, p) => x.ImportLoanTransactionsAsync(s, p),
            [ImportTypeDto.LoanApplication] = (x, s, p) => x.ImportLoanApplicationsAsync(s, p),
            [ImportTypeDto.LoanCollateral] = (x, s, p) => x.ImportLoanCollateralAsync(s, p),
            [ImportTypeDto.LoanAttribute] = (x, s, p) => x.ImportLoanAttributesAsync(s, p)
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

        public async Task ImportAsync(ImportSubscriptionDto subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));
            if (subscription.Imports.IsNullOrEmpty())
                throw new ArgumentException("No imports specified");

            var partner = await _partnerService.GetAsync(subscription.PartnerId);
            var exceptions = new List<Exception>();

            foreach (var import in subscription.Imports)
            {
                try
                {
                    if (ImportMap.TryGetValue(import, out var importFunc))
                        await importFunc(this, subscription, partner);
                    else
                        throw new ArgumentException($"value {import} ({(int) import}) has no mapping or is not defined", nameof(import));
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        public Task ImportAccountsAsync(ImportSubscriptionDto subscription, PartnerDto partner)
        {
            throw new NotImplementedException();
        }

        public Task ImportAccountTransactionsAsync(ImportSubscriptionDto subscription, PartnerDto partner)
        {
            throw new NotImplementedException();
        }       

        public async Task ImportDemographicsAsync(ImportSubscriptionDto subscription, PartnerDto partner)
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
            
            var fileName = GetFileName(subscription, partner.InternalIdentifier, ImportTypeDto.Demographic, DateTime.UtcNow);
            var fullFileName = subscription.IncomingFilePath + fileName;

            using var stream = _storage.OpenWrite(subscription.IncomingStorageLocation, fullFileName);
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

        public async Task ImportFirmographicsAsync(ImportSubscriptionDto subscription, PartnerDto partner)
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

            var fileName = GetFileName(subscription, partner.InternalIdentifier, ImportTypeDto.Firmographic, DateTime.UtcNow);
            var fullFileName = subscription.IncomingFilePath + fileName;

            using var stream = _storage.OpenWrite(subscription.IncomingStorageLocation, fullFileName);
            _writer.Open(stream.Stream, Encoding.UTF8);

            var offset = 0;
            var businesses = await _qsRepo.GetBusinessesAsync(offset, BatchSize);

            while (businesses.Count() > 0)
            {
                var firmographics = businesses.Select(transform);

                _writer.WriteRecords(firmographics);

                offset += businesses.Count();
                businesses = await _qsRepo.GetBusinessesAsync(offset, BatchSize);
            }
        }

        public Task ImportLoanApplicationsAsync(ImportSubscriptionDto subscription, PartnerDto partner)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanAttributesAsync(ImportSubscriptionDto subscription, PartnerDto partner)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanCollateralAsync(ImportSubscriptionDto subscription, PartnerDto partner)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanAccountsAsync(ImportSubscriptionDto subscription, PartnerDto partner)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanTransactionsAsync(ImportSubscriptionDto subscription, PartnerDto partner)
        {
            throw new NotImplementedException();
        }

        private static readonly int BatchSize = 10_000;

        public string GetFileName(ImportSubscriptionDto sub, PartnerIdentifierDto exportedFrom, ImportTypeDto type, DateTime effectiveDate)
        {
            // hopefully we'll just be creating a manifest down the road
            return $"{exportedFrom}_{PartnerIdentifierDto.Laso}_{sub.Frequency.ShortName()}_{type}_{effectiveDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMdd}.csv";
        }
    }
}
