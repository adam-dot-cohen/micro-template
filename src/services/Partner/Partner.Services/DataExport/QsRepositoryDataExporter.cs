using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Laso.Domain.Models;
using Partner.Core.Matching;
using Partner.Domain.Quarterspot.Enumerations;
using Partner.Domain.Quarterspot.Models;
using Partner.Data.Quarterspot;
using QsBusiness = Partner.Domain.Quarterspot.Models.Business;
using LasoBusiness = Laso.Domain.Models.Business;

// [Ed S] This all needs to be split apart (data repo instead of SQL here, likely move 
// services up a level to a new project, etc.), just need something to play around with for now

namespace Partner.Services.DataExport
{
    public class QsRepositoryDataExporter : IDataExporter
    {

        public QsRepositoryDataExporter(QuarterspotRepository businessRepo)
        {

        }

        public async Task ExportAsync()
        {
            await ExportDemographicsAsync();
            await ExportFirmographicsAsync();
            await ExportAccountsAsync();
            await ExportLoansAsync();
        }

        public Task ExportAccountsAsync(params string[] accountIds)
        {
            throw new NotImplementedException();
        }

        public Task ExportAccountTransactionsAsync(params string[] accountIds)
        {
            throw new NotImplementedException();
        }       

        public Task ExportDemographicsAsync(params string[] customerIds)
        {
            throw new NotImplementedException();
        }

        public /*async*/ Task ExportFirmographicsAsync(params string[] x)
        {
            throw new NotImplementedException();
            //var asOfDate = DateTime.UtcNow;

            //var transform = result.Select(r => new Firmographic
            //{
            //    Customer = null,
            //    Business = new LasoBusiness { Id = r.Id.ToString() },
            //    EffectiveDate = asOfDate,
            //    DateStarted = r.Established,
            //    IndustryNaics = r.IndustryNaicsCode.ToString(),
            //    IndustrySic = r.IndustrySicCode.ToString(),
            //    BusinessType = r.BusinessEntityType != null ? BusinessEntityType.FromValue(r.BusinessEntityType.Value).DisplayName : null,
            //    LegalBusinessName = r.LegalName,
            //    BusinessPhone = NormalizationMethod.Phone10(r.Phone),
            //    BusinessEin = NormalizationMethod.TaxId(r.TaxId),
            //    PostalCode = NormalizationMethod.Zip5(r.Zip)
            //});
        }

        public Task ExportLoanApplicationsAsync(params string[] loanIds)
        {
            throw new NotImplementedException();
        }

        public Task ExportLoanAttributesAsync(params string[] loanIds)
        {
            throw new NotImplementedException();
        }

        public Task ExportLoanCollateralAsync(params string[] loanIds)
        {
            throw new NotImplementedException();
        }

        public Task ExportLoansAsync(params string[] loanIds)
        {
            throw new NotImplementedException();
        }

        public Task ExportLoanTransactionsAsync(params string[] loanIds)
        {
            throw new NotImplementedException();
        }
    }
}
