using System;
using System.Linq;
using System.Threading.Tasks;
using Laso.Domain.Models;
using Partner.Core.Matching;
using Partner.Domain.Quarterspot.Enumerations;
using Partner.Data.Quarterspot;
using Partner.Domain.Common;
using Partner.Domain.Quarterspot.Models;
using System.Collections.Generic;
using Partner.Services.IO;
using System.Text;
using Partner.Core.Extensions;
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

        // ! if you add a new export function, map it here
        private readonly ExportMap ExportMap = new ExportMap
        {
            [ExportType.Demographics] = x => x.ExportDemographicsAsync(),
            [ExportType.Firmographics] = x => x.ExportFirmographicsAsync(),
            [ExportType.Accounts] = x => x.ExportAccountsAsync(),
            [ExportType.AccountTransactions] = x => x.ExportAccountTransactionsAsync(),
            [ExportType.LoanAccounts] = x => x.ExportLoanAccountsAsync(),
            [ExportType.LoanTransactions] = x => x.ExportLoanTransactionsAsync(),
            [ExportType.LoanApplications] = x => x.ExportLoanApplicationsAsync(),
            [ExportType.LoanCollateral] = x => x.ExportLoanCollateralAsync(),
            [ExportType.LoanAttributes] = x => x.ExportLoanAttributesAsync()
        };

        public QsRepositoryDataExporter(IQuarterspotRepository qsRepository, IDelimitedFileWriter writer)
        {
            _qsRepo = qsRepository;
            _writer = writer;

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

            // todo(ed s): need an output interface
            using var stream = System.IO.File.Create(@"D:\demos.csv");
            _writer.Open(stream, Encoding.UTF8);

            var offset = 0;
            var customers = await _qsRepo.GetCustomersAsync(offset, BatchSize);

            while (customers.Count() > 0)
            {
                // todo: GroupBy here because customer ID is not currently unique. Fix once fixed in the repo.
                var demos = customers.GroupBy(c => c.Id).Select(transform);

                _writer.WriteRecords(demos);

                offset += customers.Count();
                customers = await _qsRepo.GetCustomersAsync(offset, BatchSize);                
            }                       
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

            // todo(ed s): need an output interface
            // todo(ed s): need a file naming interface
            using var stream = System.IO.File.Create(@"D:\firmos.csv");
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

        private IEnumerable<T2> GetPaged<T1, T2>(Func<int, int, IEnumerable<T1>> query, int offset, int take, Func<T1, T2> transform)
        {
            return query(offset, take).Select(transform);
        }

        private static readonly int BatchSize = 10_000;
    }
}
