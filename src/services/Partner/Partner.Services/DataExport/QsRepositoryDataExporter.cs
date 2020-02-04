using System;
using System.Linq;
using System.Threading.Tasks;
using Laso.Domain.Models;
using Partner.Core.Matching;
using Partner.Domain.Quarterspot.Enumerations;
using Partner.Data.Quarterspot;
using LasoBusiness = Laso.Domain.Models.Business;
using Partner.Domain.Common;
using Partner.Domain.Quarterspot.Models;
using System.Collections.Generic;
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

        public QsRepositoryDataExporter(IQuarterspotRepository qsRepository)
        {
            _qsRepo = qsRepository;
        }

        public async Task ExportAsync(ExportType exports)
        {            
            foreach(var value in Enum.GetValues(typeof(ExportType)).Cast<ExportType>())
            {
                if (exports.HasFlag(value))
                    await ExportMap[value](this);
            }           
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
            var asOfDate = DateTime.UtcNow;
            IEnumerable<QsCustomer> customers;
            int offset = 0;

            do
            {
                customers = await _qsRepo.GetCustomersAsync(offset, BatchSize);

                var transform = customers
                .GroupBy(c => c.Id)
                .Select(c =>
                {
                    // todo: only here because customer IDs not currently unique. Fix once fixed in the repo.
                    var latest = c.OrderByDescending(s => s.CreditScoreEffectiveTime).First();
                    return new Demographic
                    {
                        Customer = new Customer { Id = latest.Id },
                        BranchId = null,
                        CreditScore = (int)latest.CreditScore,
                        EffectiveDate = latest.CreditScoreEffectiveTime.Date
                    };
                });

                // write some stuff out

                offset += customers.Count();
            }
            while (customers.Count() == BatchSize);
        }

        public async Task ExportFirmographicsAsync()
        {
            var asOfDate = DateTime.UtcNow;
            var businesses = await _qsRepo.GetBusinessesAsync(100, 500);

            var transform = businesses.Select(r => new Firmographic
            {
                Customer = null,
                Business = new LasoBusiness { Id = r.Id.ToString() },
                EffectiveDate = asOfDate,
                DateStarted = r.Established,
                IndustryNaics = r.IndustryNaicsCode.ToString(),
                IndustrySic = r.IndustrySicCode.ToString(),
                BusinessType = r.BusinessEntityType != null ? BusinessEntityType.FromValue(r.BusinessEntityType.Value).DisplayName : null,
                LegalBusinessName = r.LegalName,
                BusinessPhone = NormalizationMethod.Phone10(r.Phone),
                BusinessEin = NormalizationMethod.TaxId(r.TaxId),
                PostalCode = NormalizationMethod.Zip5(r.Zip)
            });
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

        private static readonly int BatchSize = 5000;
    }
}
