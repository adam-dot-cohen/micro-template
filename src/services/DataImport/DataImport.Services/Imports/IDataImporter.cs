using System.Threading.Tasks;
using DataImport.Services.DTOs;

namespace DataImport.Services.Imports
{
    public interface IDataImporter
    {
        PartnerIdentifier Partner { get; }

        /// <summary>
        /// Begin a bulk import operation. Retrieves all available data.
        /// </summary>        
        /// <returns>A task which completes once the import process has begun</returns>
        Task ImportAsync(ImportSubscriptionDto subscription);   
    }

    public class ImportContext
    {
        public string PartnerId { get; set; }
    }
}
