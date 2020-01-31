using System;
using System.Linq;
using System.Threading.Tasks;
using Laso.Domain.Models;
using Partner.Core.Matching;
using Partner.Domain.Quarterspot.Enumerations;
using Partner.Data.Quarterspot;
using LasoBusiness = Laso.Domain.Models.Business;
using Partner.Domain.Common;

// todo:
// - need an output formatter
// - need something to actually write to the output location

namespace Partner.Services.DataExport
{
    public class QsRepositoryDataExporter : IDataExporter
    {
        public PartnerIdentifier Partner => PartnerIdentifier.Quarterspot;

        private readonly IQuarterspotRepository _qsRepo;

        public QsRepositoryDataExporter(IQuarterspotRepository qsRepository)
        {
            _qsRepo = qsRepository;
        }

        public async Task ExportAsync(ExportType exports)
        {            
            if (exports.HasFlag(ExportType.Demographics))
                await ExportDemographicsAsync();

            if (exports.HasFlag(ExportType.Firmographics))
                await ExportFirmographicsAsync();

            if (exports.HasFlag(ExportType.Accounts))
            {
                await ExportAccountsAsync();
            }
            else if (exports.HasFlag(ExportType.AccountTransactions))
            {
                // if we're exporting accounts we implicitly grab transactions. Same for loans.
                await ExportAccountTransactionsAsync();
            }

            if (exports.HasFlag(ExportType.LoanAccounts))
            {
                await ExportLoansAsync();
            }
            else
            {
                if (exports.HasFlag(ExportType.LoanTransactions))
                    await ExportLoanTransactionsAsync();

                if (exports.HasFlag(ExportType.LoanCollateral))
                    await ExportLoanCollateralAsync();

                if (exports.HasFlag(ExportType.LoanAttributes))
                    await ExportLoanAttributesAsync();

                if (exports.HasFlag(ExportType.LoanApplications))
                    await ExportLoanApplicationsAsync();
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
            var customers = await _qsRepo.GetCustomersAsync();

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
        }

        public async Task ExportFirmographicsAsync()
        {
            var asOfDate = DateTime.UtcNow;
            var businesses = await _qsRepo.GetBusinessesAsync();

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

        public Task ExportLoansAsync()
        {
            throw new NotImplementedException();
        }

        public Task ExportLoanTransactionsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
