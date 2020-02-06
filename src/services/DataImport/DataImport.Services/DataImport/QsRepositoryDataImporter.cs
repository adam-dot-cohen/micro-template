using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using DataImport.Core.Matching;
using DataImport.Data.Quarterspot;
using DataImport.Domain.Laso.Common;
using DataImport.Domain.Laso.Models;
using DataImport.Domain.Laso.Quarterspot.Enumerations;
using DataImport.Domain.Laso.Quarterspot.Models;
using DataImport.Services.IO;
using DataImport.Services.IO.Storage.Blob.Azure;
using DataImport.Core.Extensions;

// todo:
// - need an output formatter
// - need something to actually write to the output location

namespace DataImport.Services.DataImport
{
    using ImportMap = Dictionary<ImportType, Func<QsRepositoryDataImporter, Task>>;

    public class QsRepositoryDataImporter : IDataImporter
    {
        public PartnerIdentifier ExportFrom => PartnerIdentifier.Quarterspot;
        public PartnerIdentifier ImportTo => PartnerIdentifier.Laso;

        private readonly IQuarterspotRepository _qsRepo;
        private readonly IDelimitedFileWriter _writer;
        private readonly IBlobStorageService _storage;
        private readonly IImportPathResolver _fileNamer;

        // ! if you add a new import function, map it here
        private readonly ImportMap ImportMap = new ImportMap
        {
            [ImportType.Demographic] = x => x.ImportDemographicsAsync(),
            [ImportType.Firmographic] = x => x.ImportFirmographicsAsync(),
            [ImportType.Account] = x => x.ImportAccountsAsync(),
            [ImportType.AccountTransaction] = x => x.ImportAccountTransactionsAsync(),
            [ImportType.LoanAccount] = x => x.ImportLoanAccountsAsync(),
            [ImportType.LoanTransaction] = x => x.ImportLoanTransactionsAsync(),
            [ImportType.LoanApplication] = x => x.ImportLoanApplicationsAsync(),
            [ImportType.LoanCollateral] = x => x.ImportLoanCollateralAsync(),
            [ImportType.LoanAttribute] = x => x.ImportLoanAttributesAsync()
        };

        public QsRepositoryDataImporter(
            IQuarterspotRepository qsRepository, 
            IDelimitedFileWriter writer,
            IBlobStorageService storage,
            IImportPathResolver fileNamer)
        {
            _qsRepo = qsRepository;
            _writer = writer;
            _storage = storage;
            _fileNamer = fileNamer;

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

        public async Task ImportAsync(ImportType imports)
        {
            var importFlags = Enum.GetValues(typeof(ImportType))
                .Cast<ImportType>()
                .Where(v => imports.HasFlag(v));
            
            foreach(var importType in importFlags)
            {                
                if (ImportMap.TryGetValue(importType, out var importFunc))
                    await importFunc(this);
                else
                    throw new ArgumentException($"value {importType} ({(int)importType}) has no mapping or is not defined", nameof(imports));
            }
        }

        public async Task ImportAsync(ImportType[] imports)
        {
            if (imports.IsNullOrEmpty())
                throw new ArgumentException("No imports specified");

            var first = imports.First();
            var asFlags = imports.Skip(1).Aggregate(first, (result, next) => result |= next);

            await ImportAsync(asFlags);
        }

        public Task ImportAccountsAsync()
        {
            throw new NotImplementedException();
        }

        public Task ImportAccountTransactionsAsync()
        {
            throw new NotImplementedException();
        }       

        public async Task ImportDemographicsAsync()
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

            var container = _fileNamer.GetIncomingContainerName(PartnerIdentifier.Quarterspot);
            var fileName = _fileNamer.GetName(PartnerIdentifier.Quarterspot, ImportType.Demographic, DateTime.UtcNow);

            using var stream = _storage.OpenWrite(container, fileName);
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

        public async Task ImportFirmographicsAsync()
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

            var container = _fileNamer.GetIncomingContainerName(PartnerIdentifier.Quarterspot);
            var fileName = _fileNamer.GetName(PartnerIdentifier.Quarterspot, ImportType.Demographic, DateTime.UtcNow);

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

        public Task ImportLoanApplicationsAsync()
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanAttributesAsync()
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanCollateralAsync()
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanAccountsAsync()
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanTransactionsAsync()
        {
            throw new NotImplementedException();
        }

        private static readonly int BatchSize = 10_000;
    }
}
