using System;
using System.Linq;
using System.Threading.Tasks;
using Laso.Domain.Models;
using Partner.Core.Matching;
using Partner.Domain.Quarterspot.Enumerations;
using Partner.Data.Quarterspot;
using LasoBusiness = Laso.Domain.Models.Business;

namespace Partner.Services.DataExport
{
    public class QsRepositoryDataExporter : IDataExporter
    {
        private readonly IQuarterspotRepository _qsRepo;

        public QsRepositoryDataExporter(IQuarterspotRepository qsRepository)
        {
            _qsRepo = qsRepository;
        }

        public async Task ExportAsync()
        {
            await ExportDemographicsAsync();
            await ExportFirmographicsAsync();
            await ExportAccountsAsync();
            await ExportLoansAsync();
        }

        public Task ExportAccountsAsync()
        {
            throw new NotImplementedException();
        }

        public Task ExportAccountTransactionsAsync()
        {
            throw new NotImplementedException();
        }       

        public Task ExportDemographicsAsync()
        {
            throw new NotImplementedException();
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
