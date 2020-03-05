using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Laso.DataImport.Domain.Entities;

namespace Laso.DataImport.Services
{
    public class ImportOperation
    {
        public string ContainerName { get; set; }
        public string BlobPath { get; set; }
        public IEnumerable<ImportType> Imports { get; set; }
        public EncryptionType Encryption { get; set; }
        /// <summary>
        /// optional filter used to exclude records created or updated before a given time
        /// </summary>
        public DateTime? DateFilter { get; set; }
    }

    public interface IDataImporter
    {
        PartnerIdentifier Partner { get; }

        /// <summary>
        /// Begin a bulk import operation. Retrieves all available data.
        /// </summary>
        /// <returns>A task which completes once the import process has completed</returns>
        Task ImportAsync(ImportOperation request);
    }
}
