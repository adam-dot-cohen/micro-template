using System.Threading.Tasks;

namespace Partner.Services.DataExport
{
    public interface IDataExporter
    {
        /// <summary>
        /// Begin a bulk export operation. Retrieves all available data.
        /// </summary>
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportAsync();

        /// <summary>
        /// Begin the export of demographic data
        /// </summary>
        /// <param name="businessIds">Export demographic data for the specified customers. If empty, export all demographic data.</param>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportDemographicsAsync(params string[] customerIds);

        /// <summary>
        /// Begin the export of firmographic data
        /// </summary>
        /// <param name="businessIds">Export firmographic data for the specified businesses. If empty, export all firmographic data.</param>        
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportFirmographicsAsync(params string[] businessIds);

        /// <summary>
        /// Begin the export of customer accounts
        /// </summary>
        /// <param name="accountIds">Export the specified accounts. If empty, export all accounts.</param>
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportAccountsAsync(params string[] accountIds);
        /// <summary>
        /// Begin the export of customer account transactions
        /// </summary>
        /// <param name="accountIds">Export transactions belonging to the specified accounts. If empty, export all transactions.</param>
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportAccountTransactionsAsync(params string[] accountIds);

        /// <summary>
        /// Begin the export of loan accounts
        /// </summary>
        /// <param name="accountIds">Export the specified loan accounts. If empty, export all loan accounts.</param>
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportLoansAsync(params string[] loanIds);

        /// <summary>
        /// Begin the export of loan transactions
        /// </summary>
        /// <param name="loanIds">Export loan transactions belonging to the specified loan accounts. If empty, export all loan transactions.</param>
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportLoanTransactionsAsync(params string[] loanIds);

        /// <summary>
        /// Begin the export of loan collateral metadata
        /// </summary>
        /// <param name="loanIds">Export loan collateral metadata belonging to the specified loan accounts. If empty, export all loan transactions.</param>
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportLoanCollateralAsync(params string[] loanIds);

        /// <summary>
        /// Begin the export of loan applications
        /// </summary>
        /// <param name="loanIds">Export loan applications belonging to the specified loan accounts. If empty, export all loan applications.</param>
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportLoanApplicationsAsync(params string[] loanIds);

        /// <summary>
        /// Begin the export of loan applications
        /// </summary>
        /// <param name="loanIds">Export loan attributes belonging to the specified loan accounts. If empty, export all loan attributes.</param>
        /// <returns>A task which completes once the export process has begun</returns>
        Task ExportLoanAttributesAsync(params string[] loanIds);
    }
}
