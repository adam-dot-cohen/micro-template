using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Laso.DataImport.Core.Configuration;
using Laso.DataImport.Core.Matching;
using Laso.DataImport.Data.Quarterspot;
using Laso.DataImport.Domain.Models;
using Laso.DataImport.Domain.Quarterspot.Enumerations;
using Laso.DataImport.Domain.Quarterspot.Models;
using Laso.DataImport.Services.IO;
using Laso.DataImport.Services.IO.Storage.Blob.Azure;
using Laso.DataImport.Core.Extensions;
using Laso.DataImport.Services.DTOs;
using Laso.DataImport.Services.Encryption;
using Laso.DataImport.Services.Security;

namespace Laso.DataImport.Services
{
    using ImportMap = Dictionary<ImportType, Func<QsRepositoryDataImporter, ImportSubscription, Task>>;

    public class QsRepositoryDataImporter : IDataImporter
    {
        public PartnerIdentifier Partner => PartnerIdentifier.Quarterspot;

        private readonly IEncryptionConfiguration _config;
        private readonly IQuarterspotRepository _qsRepo;
        private readonly IDelimitedFileWriter _writer;
        private readonly IBlobStorageService _storage;
        private readonly IEncryptionFactory _encryptionFactory;
        private readonly ISecureStore _secureStore;

        // ! if you add a new import function, map it here
        private readonly ImportMap ImportMap = new ImportMap
        {
            [ImportType.Demographic] = (x, s) => x.ImportDemographicsAsync(s),
            [ImportType.Firmographic] = (x, s) => x.ImportFirmographicsAsync(s),
            [ImportType.Account] = (x, s) => x.ImportAccountsAsync(s),
            [ImportType.AccountTransaction] = (x, s) => x.ImportAccountTransactionsAsync(s),
            [ImportType.LoanAccount] = (x, s) => x.ImportLoanAccountsAsync(s),
            [ImportType.LoanTransaction] = (x, s) => x.ImportLoanTransactionsAsync(s),
            [ImportType.LoanApplication] = (x, s) => x.ImportLoanApplicationsAsync(s),
            [ImportType.LoanCollateral] = (x, s) => x.ImportLoanCollateralAsync(s),
            [ImportType.LoanAttribute] = (x, s) => x.ImportLoanAttributesAsync(s)
        };        

        public QsRepositoryDataImporter(
            IEncryptionConfiguration config,
            IQuarterspotRepository qsRepository, 
            IDelimitedFileWriter writer,
            IBlobStorageService storage,
            IEncryptionFactory encryptionFactory,
            ISecureStore secureStore)
        {
            _config = config;
            _qsRepo = qsRepository;
            _writer = writer;
            _storage = storage;
            _encryptionFactory = encryptionFactory;
            _secureStore = secureStore;

            _writer.Configuration = new DelimitedFileConfiguration
            {
                BufferSize = 1024 * 16,
                Delimiter = ",",
                HasHeaderRecord = true,
                IgnoreExtraColumns = false,
                TypeConverterOptions = new List<TypeConverterOption>
                {
                    new TypeConverterOption
                    {
                        Type = typeof(DateTime),
                        Format = "M/d/yyyy"
                    }
                }
            };            
        }       

        public async Task ImportAsync(ImportSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));
            if (subscription.Imports.IsNullOrEmpty())
                throw new ArgumentException("No imports specified");

            var exceptions = new List<Exception>();

            foreach (var import in subscription.Imports)
            {
                try
                {
                    if (ImportMap.TryGetValue(import, out var importFunc))
                        await importFunc(this, subscription);
                    else
                        throw new ArgumentException($"value {import} ({(int) import}) has no mapping or is not defined", nameof(import));
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        public Task ImportAccountsAsync(ImportSubscription subscription)
        {
            throw new NotImplementedException();
        }

        public Task ImportAccountTransactionsAsync(ImportSubscription subscription)
        {
            throw new NotImplementedException();
        }       

        public async Task ImportDemographicsAsync(ImportSubscription subscription)
        {
            static string GetCustomerId(string encryptedId, ISecureStore secureStore, RSA crypto)
            {
                if (string.IsNullOrEmpty(encryptedId))
                    return string.Empty;

                var buffer = new byte[encryptedId.Length / 2];
                for (var cnt = 0; cnt < encryptedId.Length; cnt += 2)
                    buffer[cnt / 2] = Convert.ToByte(encryptedId.Substring(cnt, 2), 16);

                var decrypted = crypto.Decrypt(buffer, RSAEncryptionPadding.OaepSHA1);
                using var hash = new SHA256Managed();

                return BitConverter.ToString(hash.ComputeHash(decrypted)).Replace("-", string.Empty);
            };

            static Demographic transform(IGrouping<string, QsCustomer> c, ISecureStore secureStore, RSA crypto)
            {
                var latest = c.OrderByDescending(s => s.CreditScoreEffectiveTime).First();

                return new Demographic
                {
                    CustomerId = GetCustomerId(latest.SsnEncrypted, secureStore, crypto),
                    BranchId = null,
                    CreditScore = (int)latest.CreditScore,
                    EffectiveDate = latest.CreditScoreEffectiveTime.Date
                };
            };

            var encrypter = _encryptionFactory.Create(subscription.EncryptionType);
            var fileName = GetFileName(subscription, ImportType.Demographic, DateTime.UtcNow, encrypter.FileExtension);
            var fullFileName = subscription.IncomingFilePath + fileName;

            using var stream = _storage.OpenWrite(subscription.IncomingStorageLocation, fullFileName);

            await encrypter.Encrypt(stream);
            _writer.Open(stream.Stream, Encoding.UTF8);

            var customers = await _qsRepo.GetCustomersAsync();

            // GroupBy here because we may have multiple results returned
            // for the same person (same person tied to two businesses, duplicate
            // records, multiple credit score results, etc.)
            var certBytes = await _secureStore.GetPrivateCertificateAsync(_config.QsPrivateCertificateName);
            System.IO.File.WriteAllBytes(@"C:\Users\e.swangren\Documents\certs\from-az.pfx", certBytes);
            var cert = new X509Certificate2(certBytes, _config.QsPrivateCertificatePassPhrase);
            var crypto = cert.GetRSAPrivateKey();

            var demos = customers.GroupBy(c => c.SsnEncrypted).Select(c => transform(c, _secureStore, crypto));
            _writer.WriteRecords(demos);
        }

        public async Task ImportFirmographicsAsync(ImportSubscription subscription)
        {
            var asOfDate = DateTime.UtcNow;

            Firmographic transform(QsBusiness r) => new Firmographic
            {
                // todo(ed): need unique customer ID
                CustomerId = null,
                BusinessId = r.Id.ToString(),
                EffectiveDate = asOfDate,
                DateStarted = r.Established,
                IndustryNaics = r.IndustryNaicsCode.ToString(),
                IndustrySic = r.IndustrySicCode.ToString(),
                BusinessType = r.BusinessEntityType != null ? BusinessEntityType.FromValue(r.BusinessEntityType.Value).DisplayName : null,
                LegalBusinessName = r.LegalName,
                BusinessPhone = NormalizationMethod.Phone10(r.Phone),
                BusinessEin = NormalizationMethod.TaxId(r.TaxId),
                PostalCode = NormalizationMethod.Zip5(r.Zip)
            };

            var encrypter = _encryptionFactory.Create(subscription.EncryptionType);
            var fileName = GetFileName(subscription, ImportType.Firmographic, DateTime.UtcNow, encrypter.FileExtension);
            var fullFileName = subscription.IncomingFilePath + fileName;

            using var stream = _storage.OpenWrite(subscription.IncomingStorageLocation, fullFileName);
            
            await encrypter.Encrypt(stream);
            _writer.Open(stream.Stream, Encoding.UTF8);

            var offset = 0;
            var businesses = await _qsRepo.GetBusinessesAsync(offset, BatchSize);

            while (businesses.Count() > 0)
            {
                var firmographics = businesses.Select(transform);

                _writer.WriteRecords(firmographics);

                offset += businesses.Count();
                businesses = await _qsRepo.GetBusinessesAsync(offset, BatchSize);
            }
        }

        public Task ImportLoanApplicationsAsync(ImportSubscription subscription)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanAttributesAsync(ImportSubscription subscription)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanCollateralAsync(ImportSubscription subscription)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanAccountsAsync(ImportSubscription subscription)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanTransactionsAsync(ImportSubscription subscription)
        {
            throw new NotImplementedException();
        }

        private static readonly int BatchSize = 10_000;

        public string GetFileName(ImportSubscription sub, ImportType type, DateTime effectiveDate, string encryptionExtension = "")
        {
            // hopefully we'll just be creating a manifest down the road
            return $"{PartnerIdentifier.Quarterspot}_{PartnerIdentifier.Laso}_{sub.Frequency.ShortName()}_{type}_{effectiveDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMdd}.csv{encryptionExtension}";
        }
    }
}
