﻿using System.Threading.Tasks;
using Laso.DataImport.Services.DTOs;

namespace Laso.DataImport.Services
{
    public interface IDataImporter
    {
        PartnerIdentifier Partner { get; }

        /// <summary>
        /// Begin a bulk import operation. Retrieves all available data.
        /// </summary>        
        /// <returns>A task which completes once the import process has begun</returns>
        Task ImportAsync(ImportSubscription subscription);   
    }

    public class ImportContext
    {
        public string PartnerId { get; set; }
    }
}
