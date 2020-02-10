using System.Threading.Tasks;
using DataImport.Domain.Api;

namespace DataImport.Services.DataImport
{
    public interface IDataImporter
    {
        PartnerIdentifier Partner { get; }        

        /// <summary>
        /// Begin a bulk import operation. Retrieves all available data.
        /// </summary>        
        /// <returns>A task which completes once the import process has begun</returns>
        Task ImportAsync(ImportType[] imports);

        /// <summary>
        /// Begin a bulk import operation. Retrieves all available data.
        /// </summary>
        /// <param name="imports">A bit field which determines which data to import</param>
        /// <returns>A task which completes once the import process has begun</returns>
        Task ImportAsync(ImportType imports);
    }
}
