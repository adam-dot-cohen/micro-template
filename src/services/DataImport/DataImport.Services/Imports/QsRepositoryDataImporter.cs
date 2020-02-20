using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Laso.DataImport.Core.Encryption;
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
    using ImportMap = Dictionary<ImportTypeDto, Func<QsRepositoryDataImporter, ImportSubscriptionDto, Task>>;

    public class QsRepositoryDataImporter : IDataImporter
    {
        public PartnerIdentifier Partner => PartnerIdentifier.Quarterspot;

        private readonly IQuarterspotRepository _qsRepo;
        private readonly IDelimitedFileWriter _writer;
        private readonly IBlobStorageService _storage;
        private readonly IPgpEncryption _encryption;

        // ! if you add a new import function, map it here
        private readonly ImportMap ImportMap = new ImportMap
        {
            [ImportTypeDto.Demographic] = (x, s) => x.ImportDemographicsAsync(s),
            [ImportTypeDto.Firmographic] = (x, s) => x.ImportFirmographicsAsync(s),
            [ImportTypeDto.Account] = (x, s) => x.ImportAccountsAsync(s),
            [ImportTypeDto.AccountTransaction] = (x, s) => x.ImportAccountTransactionsAsync(s),
            [ImportTypeDto.LoanAccount] = (x, s) => x.ImportLoanAccountsAsync(s),
            [ImportTypeDto.LoanTransaction] = (x, s) => x.ImportLoanTransactionsAsync(s),
            [ImportTypeDto.LoanApplication] = (x, s) => x.ImportLoanApplicationsAsync(s),
            [ImportTypeDto.LoanCollateral] = (x, s) => x.ImportLoanCollateralAsync(s),
            [ImportTypeDto.LoanAttribute] = (x, s) => x.ImportLoanAttributesAsync(s)
        };        

        public QsRepositoryDataImporter(
            IQuarterspotRepository qsRepository, 
            IDelimitedFileWriter writer,
            IBlobStorageService storage,
            IPgpEncryption encryption)
        {
            _qsRepo = qsRepository;
            _writer = writer;
            _storage = storage;
            _encryption = encryption;

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

            var exceptions = new List<Exception>();

            foreach (var import in subscription.Imports)
            {
                try
                {
                    if (ImportMap.TryGetValue(import, out var importFunc))
                        await importFunc(this, subscription);
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

        public Task ImportAccountsAsync(ImportSubscriptionDto subscription)
        {
            throw new NotImplementedException();
        }

        public Task ImportAccountTransactionsAsync(ImportSubscriptionDto subscription)
        {
            throw new NotImplementedException();
        }       

        public async Task ImportDemographicsAsync(ImportSubscriptionDto subscription)
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
            
            var fileName = GetFileName(subscription, ImportTypeDto.Demographic, DateTime.UtcNow);
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

        public async Task ImportFirmographicsAsync(ImportSubscriptionDto subscription)
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

            var fileName = GetFileName(subscription, ImportTypeDto.Firmographic, DateTime.UtcNow);
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

        public Task ImportLoanApplicationsAsync(ImportSubscriptionDto subscription)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanAttributesAsync(ImportSubscriptionDto subscription)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanCollateralAsync(ImportSubscriptionDto subscription)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanAccountsAsync(ImportSubscriptionDto subscription)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanTransactionsAsync(ImportSubscriptionDto subscription)
        {
            throw new NotImplementedException();
        }

        private static readonly int BatchSize = 10_000;

        public string GetFileName(ImportSubscriptionDto sub, ImportTypeDto type, DateTime effectiveDate)
        {
            // hopefully we'll just be creating a manifest down the road
            return $"{PartnerIdentifier.Quarterspot}_{PartnerIdentifier.Laso}_{sub.Frequency.ShortName()}_{type}_{effectiveDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMdd}.csv";
        }
    }
}
