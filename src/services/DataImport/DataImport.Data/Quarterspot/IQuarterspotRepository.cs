using System.Collections.Generic;
using System.Threading.Tasks;
using Laso.DataImport.Domain.Quarterspot.Models;

namespace Laso.DataImport.Data.Quarterspot
{
    public interface IQuarterspotRepository
    {
        Task<IEnumerable<QsCustomer>> GetCustomersAsync();
        Task<IEnumerable<QsCustomer>> GetCustomersAsync(int offset, int take);
        Task<IEnumerable<QsBusiness>> GetBusinessesAsync();
        Task<IEnumerable<QsBusiness>> GetBusinessesAsync(int offset, int take);
        Task<IEnumerable<QsAccount>> GetAccountsAsync();
        Task<IEnumerable<QsAccount>> GetAccountsAsync(int offset, int take);
        Task<IEnumerable<QsAccountTransaction>> GetAccountTransactionsAsync();
        Task<IEnumerable<QsAccountTransaction>> GetAccountTransactionsAsync(int offset, int take);
        Task<IEnumerable<QsLoan>> GetLoansAsync();
        Task<IEnumerable<QsLoan>> GetLoansAsync(int offset, int take);
        Task<IEnumerable<QsLoanMetadata>> GetLoanMetadataAsync();
        Task<IEnumerable<QsLoanMetadata>> GetLoanMetadataAsync(int offset, int take);
    }
}
