using System;
using System.ComponentModel.DataAnnotations;

namespace DataImport.Domain.Api
{
    public class ImportSubscription
    {
        public string Id { get; set; }
        [Required]
        public string PartnerId { get; set; }
        [Required]
        public string Frequency { get; set; }
        [Required]
        // todo: https://stackoverflow.com/questions/59371429/serializing-enum-as-string-using-attribute-in-azure-functions-3-0
        // enum conversion to string is broken currently. Use ImportType (and ImportFrequency above) once fixed
        public string[] Imports { get; set; }
        public DateTime? LastSuccessfulImport { get; set; }
        // if left null, initialize via the current date + frequency
        public DateTime? NextScheduledImport { get; set; }
    }
}
