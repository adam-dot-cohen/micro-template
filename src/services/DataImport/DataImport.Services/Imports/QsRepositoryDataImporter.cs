﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
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
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;

namespace Laso.DataImport.Services
{
    using ImportMap = Dictionary<ImportType, Func<QsRepositoryDataImporter, ImportSubscription, Task>>;

    public class QsRepositoryDataImporter : IDataImporter
    {
        public PartnerIdentifier Partner => PartnerIdentifier.Quarterspot;

        private readonly IQuarterspotRepository _qsRepo;
        private readonly IDelimitedFileWriter _writer;
        private readonly IBlobStorageService _storage;
        private readonly IEncryptionFactory _encryptionFactory;

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
            IQuarterspotRepository qsRepository,
            IDelimitedFileWriter writer,
            IBlobStorageService storage,
            IEncryptionFactory encryptionFactory)
        {
            _qsRepo = qsRepository;
            _writer = writer;
            _storage = storage;
            _encryptionFactory = encryptionFactory;

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

        public async Task ImportAccountsAsync(ImportSubscription subscription)
        {
            static Account transform(QsAccount a)
            {
                return new Account
                {
                    AccountId = a.AccountId.ToString(),
                    BusinessId = a.BusinessId.ToString(),
                    EffectiveDate = DateTime.UtcNow.Date,
                    AccountType = BankAccountCategory.FromValue(a.BankAccountCategoryValue).DisplayName,
                    AccountOpenDate = a.OpenDate,
                    CurrentBalance = a.CurrentBalance?.ToString(),
                    CurrentBalanceDate = a.CurrentBalanceDate
                };
            };

            await ExportRecordsAsync(subscription, ImportType.Account, _qsRepo.GetAccountsAsync, transform);          
        }        

        public Task ImportAccountTransactionsAsync(ImportSubscription subscription)
        {
            throw new NotImplementedException();
        }

        public async Task ImportDemographicsAsync(ImportSubscription subscription)
        {
            static Demographic transform(QsCustomer c, Func<QsCustomer, string> idGenerator)
            {
                return new Demographic
                {
                    CustomerId = idGenerator(c),
                    BranchId = null,
                    CreditScore = (int)c.CreditScore,
                    EffectiveDate = c.CreditScoreEffectiveTime.Date
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
            // for the same person (same real life person with > 1 BusinessPrincial
            // record) and we want the latest data.
            var demos = customers.Select(c => transform(c, GenerateCustomerId))
                .Where(c => c.CustomerId != null)
                .GroupBy(d => d.CustomerId)
                .Select(g => g.OrderByDescending(d => d.EffectiveDate).First());
            
            _writer.WriteRecords(demos);
        }

        public async Task ImportFirmographicsAsync(ImportSubscription subscription)
        {
            var asOfDate = DateTime.UtcNow;

            Firmographic transform(QsBusiness r) => new Firmographic
            {
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

            await ExportRecordsAsync(subscription, ImportType.Firmographic, _qsRepo.GetBusinessesAsync, transform);
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

        private const int BatchSize = 10_000;

        private async Task ExportRecordsAsync<IN, OUT>(
            ImportSubscription subscription,
            ImportType type,
            Func<int, int, Task<IEnumerable<IN>>> aggregator,
            Func<IN, OUT> transform, int batchSize = BatchSize)
        {
            var encrypter = _encryptionFactory.Create(subscription.EncryptionType);
            var fileName = GetFileName(subscription, type, DateTime.UtcNow, encrypter.FileExtension);
            var fullFileName = subscription.IncomingFilePath + fileName;

            using var stream = _storage.OpenWrite(subscription.IncomingStorageLocation, fullFileName);

            await encrypter.Encrypt(stream);
            _writer.Open(stream.Stream, Encoding.UTF8);

            var offset = 0;
            var qsEntities = await aggregator(offset, batchSize);

            while (qsEntities.Count() > 0)
            {
                var lasoEntities = qsEntities.Select(transform);

                _writer.WriteRecords(lasoEntities);

                offset += qsEntities.Count();
                qsEntities = await aggregator(offset, batchSize);
            }
        }

        public string GetFileName(ImportSubscription sub, ImportType type, DateTime effectiveDate, string encryptionExtension = "")
        {
            // hopefully we'll just be creating a manifest down the road
            return $"{PartnerIdentifier.Quarterspot}_{PartnerIdentifier.Laso}_{sub.Frequency.ShortName()}_{type}_{effectiveDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMdd}.csv{encryptionExtension}";
        }

        private static Dictionary<Guid, string> _customerIdLookup;

        private static Dictionary<Guid, string> CustomerIdLookup
        {
            get
            {
                if (_customerIdLookup == null)
                {
                    //_customerIdLookup = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(File.ReadAllText(Path.Combine("Resources", "customer-id-lookup.json")));
                    var assembly = typeof(QsRepositoryDataImporter).GetTypeInfo().Assembly;
                    using var stream = assembly.GetManifestResourceStream("Laso.DataImport.Services.Resources.customer-id-lookup.json");
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();

                    _customerIdLookup = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(json);
                }

                return _customerIdLookup;
            }
        }

        private string GenerateCustomerId(QsCustomer c)
        {
            CustomerIdLookup.TryGetValue(c.PrincipalId, out var id);
            return id;

            // this is the 'old' method which decrypted and hashed the SSN.
            // we now operate off of a lookup table with pre-hashed SSNs.

            //if (string.IsNullOrEmpty(encryptedId))
            //    return string.Empty;

            //var buffer = new byte[encryptedId.Length / 2];
            //for (var cnt = 0; cnt < encryptedId.Length; cnt += 2)
            //    buffer[cnt / 2] = Convert.ToByte(encryptedId.Substring(cnt, 2), 16);

            //var decrypted = crypto.Decrypt(buffer, RSAEncryptionPadding.OaepSHA1);
            //using var hash = new SHA256Managed();

            //return BitConverter.ToString(hash.ComputeHash(decrypted)).Replace("-", string.Empty);
        }
    }
}
