using System;
using System.Linq;
using System.Threading.Tasks;
using Partner.Domain.Laso.Models;
using Partner.Core.Matching;
using Partner.Domain.Laso.Quarterspot.Enumerations;
using Partner.Data.Quarterspot;
using Partner.Domain.Laso.Common;
using Partner.Domain.Laso.Quarterspot.Models;
using System.Collections.Generic;
using Partner.Services.IO;
using System.Text;
using Partner.Core.Extensions;
using Partner.Services.IO.Storage;
// todo:
// - need an output formatter
// - need something to actually write to the output location

namespace Partner.Services.DataExport
{
    using ExportMap = Dictionary<ExportType, Func<QsRepositoryDataExporter, Task>>;

    public class QsRepositoryDataExporter : IDataExporter
    {
        public PartnerIdentifier Partner => PartnerIdentifier.Quarterspot;
        
        private readonly IQuarterspotRepository _qsRepo;
        private readonly IDelimitedFileWriter _writer;
        private readonly IBlobStorageService _storage;
        private readonly IStorageMonikerFactory _storageMonikerFactory;

        // ! if you add a new export function, map it here
        private readonly ExportMap ExportMap = new ExportMap
        {
            [ExportType.Demographic] = x => x.ExportDemographicsAsync(),
            [ExportType.Firmographic] = x => x.ExportFirmographicsAsync(),
            [ExportType.Account] = x => x.ExportAccountsAsync(),
            [ExportType.AccountTransaction] = x => x.ExportAccountTransactionsAsync(),
            [ExportType.LoanAccount] = x => x.ExportLoanAccountsAsync(),
            [ExportType.LoanTransaction] = x => x.ExportLoanTransactionsAsync(),
            [ExportType.LoanApplication] = x => x.ExportLoanApplicationsAsync(),
            [ExportType.LoanCollateral] = x => x.ExportLoanCollateralAsync(),
            [ExportType.LoanAttribute] = x => x.ExportLoanAttributesAsync()
        };

        public QsRepositoryDataExporter(
            IQuarterspotRepository qsRepository, 
            IDelimitedFileWriter writer,
            IBlobStorageService storage,
            IStorageMonikerFactory storageMonikerFactory)
        {
            _qsRepo = qsRepository;
            _writer = writer;
            _storage = storage;
            _storageMonikerFactory = storageMonikerFactory;

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

        public async Task ExportAsync(ExportType exports)
        {
            var exportFlags = Enum.GetValues(typeof(ExportType))
                .Cast<ExportType>()
                .Where(v => exports.HasFlag(v));
            
            foreach(var export in exportFlags)
            {                
                if (ExportMap.TryGetValue(export, out var exportFunc))
                    await exportFunc(this);
                else
                    throw new ArgumentException($"value {export} ({(int)export}) has no mapping or is not defined", nameof(exports));
            }
        }

        public async Task ExportAsync(ExportType[] exports)
        {
            if (exports.IsNullOrEmpty())
                throw new ArgumentException("No exports specified");

            var first = exports.First();
            var asFlags = exports.Skip(1).Aggregate(first, (result, next) => result |= next);

            await ExportAsync(asFlags);
        }

        public Task ExportAccountsAsync()
        {
            throw new NotImplementedException();
        }

        public Task ExportAccountTransactionsAsync()
        {
            throw new NotImplementedException();
        }       

        public async Task ExportDemographicsAsync()
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

            using var stream = _storage.OpenWrite("", GetFileName(ExportType.Demographic, DateTime.UtcNow, ".csv"));
            _writer.Open(stream, Encoding.UTF8);

            var customers = await _qsRepo.GetCustomersAsync();

            // todo: GroupBy here (and in the paged version) because customer ID
            // is not currently unique. We need to be able to read encrypted strings
            // and use the SSN to generate a unique ID (or something else entirely).
            // also means we can't use the paged interface yet.
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

        public async Task ExportFirmographicsAsync()
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

            // todo(ed s): container name from somewhere
            // todo(ed s): need a file naming interface
            using var stream = _storage.OpenWrite("", GetFileName(ExportType.Firmographic, asOfDate, ".csv"));
            _writer.Open(stream, Encoding.UTF8);

            while (businesses.Count() > 0)
            {
                var firmographics = businesses.Select(transform);

                _writer.WriteRecords(firmographics);

                offset += businesses.Count();
                businesses = await _qsRepo.GetBusinessesAsync(offset, BatchSize);                
            }
        }

        public Task ExportLoanApplicationsAsync()
        {
            throw new NotImplementedException();
        }

        public Task ExportLoanAttributesAsync()
        {
            throw new NotImplementedException();
        }

        public Task ExportLoanCollateralAsync()
        {
            throw new NotImplementedException();
        }

        public Task ExportLoanAccountsAsync()
        {
            throw new NotImplementedException();
        }

        public Task ExportLoanTransactionsAsync()
        {
            throw new NotImplementedException();
        }

        // todo: need an interface for generating file names.
        // Currently we have nowhere to store the frequency part of the name.
        private string GetFileName(ExportType type, DateTime effectiveDate, string extension, string transformExtension = default)
        {
            var additionalExtension = String.IsNullOrEmpty(transformExtension) ? "" : $".{transformExtension}";
            return $"{PartnerIdentifier.Quarterspot}_{PartnerIdentifier.Laso}_W_{type}_{effectiveDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMdd}.{extension}{additionalExtension}";
        }

        private static readonly int BatchSize = 10_000;
    }
}
