﻿using System.Threading.Tasks;
using Laso.DataImport.Services.DTOs;

namespace Laso.DataImport.Services
{
    public interface IDataImporter
    {
        PartnerIdentifierDto Partner { get; }

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
