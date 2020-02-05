using System.Threading.Tasks;
using Partner.Domain.Laso.Common;

namespace Partner.Services.DataExport
{
    public interface IDataExporter
    {
        PartnerIdentifier Partner { get; }

        /// <summary>
        /// Begin a bulk export operation. Retrieves all available data.
        /// </summary>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportAsync(ExportType[] exports);

        /// <summary>
        /// Begin a bulk export operation. Retrieves all available data.
        /// </summary>
        /// <param name="exports">A bit field which determines which export functions to execute</param>
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportAsync(ExportType exports);

        /// <summary>
        /// Begin the export of demographic data
        /// </summary>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportDemographicsAsync();

        /// <summary>
        /// Begin the export of firmographic data
        /// </summary>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportFirmographicsAsync();

        /// <summary>
        /// Begin the export of customer accounts
        /// </summary>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportAccountsAsync();
        /// <summary>
        /// Begin the export of customer account transactions
        /// </summary>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportAccountTransactionsAsync();

        /// <summary>
        /// Begin the export of loan accounts
        /// </summary>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportLoanAccountsAsync();

        /// <summary>
        /// Begin the export of loan transactions
        /// </summary>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportLoanTransactionsAsync();

        /// <summary>
        /// Begin the export of loan collateral metadata
        /// </summary>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportLoanCollateralAsync();

        /// <summary>
        /// Begin the export of loan applications
        /// </summary>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportLoanApplicationsAsync();

        /// <summary>
        /// Begin the export of loan applications
        /// </summary>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportLoanAttributesAsync();
    }
}
