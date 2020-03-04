using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Laso.DataImport.Core.Matching;
using Laso.DataImport.Data.Quarterspot;
using Laso.DataImport.Domain.Models;
using Laso.DataImport.Domain.Quarterspot.Enumerations;
using Laso.DataImport.Domain.Quarterspot.Models;
using Laso.DataImport.Services.IO;
using Laso.DataImport.Services.IO.Storage.Blob.Azure;
using Laso.DataImport.Services.Encryption;
using Newtonsoft.Json;
using System.Reflection;
using Laso.DataImport.Core.Extensions;
using Laso.DataImport.Domain.Entities;

namespace Laso.DataImport.Services
{
    using ImportMap = Dictionary<ImportType, Func<QsRepositoryDataImporter, ImportOperation, Task>>;

    public class QsRepositoryDataImporter : IDataImporter
    {
        public PartnerIdentifier Partner => PartnerIdentifier.Quarterspot;

        private readonly IQuarterspotRepository _qsRepo;
        private readonly IDelimitedFileWriter _writer;
        private readonly IBlobStorageService _storage;
        private readonly IEncryptionFactory _encryptionFactory;

        // ! if you add a new import function, map it here
        private static readonly ImportMap ImportMap = new ImportMap
        {
            [ImportType.Demographic] = (x, o) => x.ImportDemographicsAsync(o),
            [ImportType.Firmographic] = (x, o) => x.ImportFirmographicsAsync(o),
            [ImportType.Account] = (x, o) => x.ImportAccountsAsync(o),
            [ImportType.AccountTransaction] = (x, o) => x.ImportAccountTransactionsAsync(o),
            [ImportType.LoanAccount] = (x, o) => x.ImportLoanAccountsAsync(o),
            [ImportType.LoanTransaction] = (x, o) => x.ImportLoanTransactionsAsync(o),
            [ImportType.LoanApplication] = (x, o) => x.ImportLoanApplicationsAsync(o),
            [ImportType.LoanCollateral] = (x, o) => x.ImportLoanCollateralAsync(o),
            [ImportType.LoanAttribute] = (x, o) => x.ImportLoanAttributesAsync(o)
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

        public async Task ImportAsync(ImportOperation request)
        {
            if (string.IsNullOrWhiteSpace(request.ContainerName))
                throw new ArgumentNullException(nameof(request.ContainerName));
            if (string.IsNullOrWhiteSpace(request.BlobPath))
                throw new ArgumentNullException(nameof(request.BlobPath));
            if (request.Imports == null)
                throw new ArgumentNullException(nameof(request.Imports));

            var exceptions = new List<Exception>();

            foreach (var import in request.Imports)
            {
                try
                {
                    if (ImportMap.TryGetValue(import, out var importFunc))
                        await importFunc(this, request);
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

        public async Task ImportAccountsAsync(ImportOperation request)
        {
            static Account Transform(QsAccount a)
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

            await ExportRecordsAsync(request, ImportType.Account, _qsRepo.GetAccountsAsync, Transform).ConfigureAwait(false);
        }        

        public async Task ImportAccountTransactionsAsync(ImportOperation request)
        {
            static AccountTransaction Transform(QsAccountTransaction t)
            {
                return new AccountTransaction
                {
                    TransactionId = t.TransactionId.ToString(),
                    AccountId = t.AccountId.ToString(),
                    Amount = t.Amount.ToString(),
                    PostDate = t.PostedDate,
                    TransactionDate = t.AvailableDate,
                    MemoField = t.Memo,
                    TransactionCategory = BankAccountTransactionCategory.FromValue(t.TransactionCategoryValue).DisplayName,
                    BalanceAfterTransaction = t.BalanceAfterTransaction.ToString(),
                    MccCode = t.MccCode
                };
            };

            const int transactionsBatchSize = 100_000;
            await ExportRecordsAsync(
                    request,
                    ImportType.AccountTransaction,
                    (a, b) => _qsRepo.GetAccountTransactionsAsync(a, b, request.DateFilter),
                    Transform,
                    transactionsBatchSize)
                .ConfigureAwait(false);
        }

        public async Task ImportDemographicsAsync(ImportOperation request)
        {
            static Demographic Transform(QsCustomer c, Func<QsCustomer, string> idGenerator)
            {
                return new Demographic
                {
                    CustomerId = idGenerator(c),
                    BranchId = null,
                    CreditScore = (int)c.CreditScore,
                    EffectiveDate = c.CreditScoreEffectiveTime.Date
                };
            };

            var encrypter = _encryptionFactory.Create(request.Encryption);
            var fileName = GetFileName(ImportType.Demographic, DateTime.UtcNow, encrypter.FileExtension);
            var fullFileName = request.BlobPath + fileName;

            using var stream = _storage.OpenWrite(request.ContainerName, fullFileName);

            await encrypter.Encrypt(stream);
            _writer.Open(stream.Stream, Encoding.UTF8);

            var customers = await _qsRepo.GetCustomersAsync();

            // GroupBy here because we may have multiple results returned
            // for the same person (same real life person with > 1 BusinessPrincial
            // record) and we want the latest data.
            var demos = customers.Select(c => Transform(c, GenerateCustomerId))
                .Where(c => c.CustomerId != null)
                .GroupBy(d => d.CustomerId)
                .Select(g => g.OrderByDescending(d => d.EffectiveDate).First());
            
            _writer.WriteRecords(demos);
        }

        public async Task ImportFirmographicsAsync(ImportOperation request)
        {
            static Firmographic Transform(QsBusiness r) => new Firmographic
            {
                BusinessId = r.Id.ToString(),
                EffectiveDate = DateTime.UtcNow.Date,
                DateStarted = r.Established,
                IndustryNaics = r.IndustryNaicsCode.ToString(),
                IndustrySic = r.IndustrySicCode.ToString(),
                BusinessType = r.BusinessEntityType != null ? BusinessEntityType.FromValue(r.BusinessEntityType.Value).DisplayName : null,
                LegalBusinessName = r.LegalName,
                BusinessPhone = NormalizationMethod.Phone10(r.Phone),
                BusinessEin = NormalizationMethod.TaxId(r.TaxId),
                PostalCode = NormalizationMethod.Zip5(r.Zip)
            };

            await ExportRecordsAsync(request, ImportType.Firmographic, _qsRepo.GetBusinessesAsync, Transform).ConfigureAwait(false);
        }

        public async Task ImportLoanApplicationsAsync(ImportOperation request)
        {
            static LoanApplication Transform(QsLoanMetadata m)
            {
                return new LoanApplication
                {
                    EffectiveDate = DateTime.UtcNow.Date,
                    ApplicationDate = m.LeadCreatedDate,
                    LoanApplicationId = m.LeadId.ToString(),
                    BusinessId = m.BusinessId?.ToString(),
                    LoanAccountId = m.GroupId?.ToString(),
                    ProductType = m.Product,
                    DeclineReason = m.DeclineReason,
                    ApplicationStatus = LeadTaskReportingGroup.FromValue(m.ReportingGroupValue).DisplayName,
                    RequestedAmount = m.RequestedAmount?.ToString("C"),
                    ApprovedTerm = m.MaxOfferedTerm.ToString(),
                    ApprovedAmount = m.MaxOfferedAmount?.ToString("C"),
                    AcceptedTerm = m.AcceptedTerm,
                    AcceptedInstallment = m.AcceptedInstallment?.ToString("C"),
                    AcceptedInstallmentFrequency = m.AcceptedInstallmentFrequency,
                    AcceptedAmount = m.AcceptedAmount?.ToString("C"),
                    AcceptedInterestRate = m.AcceptedInterestRate?.ToString("P2", new NumberFormatInfo { PercentPositivePattern = 1 }),
                    AcceptedInterestRateMethod = "Full"
                };
            };

            await ExportRecordsAsync(request, ImportType.LoanApplication, _qsRepo.GetLoanMetadataAsync, Transform).ConfigureAwait(false);
        }

        public Task ImportLoanAttributesAsync(ImportOperation request)
        {
            throw new NotImplementedException();
        }

        public Task ImportLoanCollateralAsync(ImportOperation request)
        {
            throw new NotImplementedException();
        }

        public async Task ImportLoanAccountsAsync(ImportOperation request)
        {
            static LoanAccount Transform(QsLoan l)
            {
                return new LoanAccount
                {
                    LoanAccountId = l.Id.ToString(),
                    BusinessId = l.BusinessId.ToString(),
                    ProductType = l.ProductType,
                    EffectiveDate = DateTime.UtcNow.Date,
                    IssueDate = l.IssueDate,
                    MaturityDate = l.MaturityDate,
                    InterestRateMethod = l.InterestRateMethod,
                    InterestRate = l.InterestRate,
                    AmortizationMethod = l.AmortizationMethod,
                    Term = l.Term.ToString(),
                    Installment = Math.Round(l.Installment, 2).ToString(),
                    InstallmentFrequency = l.InstallmentFrequency
                };
            };

            await ExportRecordsAsync(request, ImportType.LoanAccount, _qsRepo.GetLoansAsync, Transform).ConfigureAwait(false);
        }

        public Task ImportLoanTransactionsAsync(ImportOperation request)
        {
            throw new NotImplementedException();
        }

        private const int BatchSize = 10_000;

        private async Task ExportRecordsAsync<TIn, TOut>(
            ImportOperation request,
            ImportType type,
            Func<int, int, Task<IEnumerable<TIn>>> aggregator,
            Func<TIn, TOut> transform, int batchSize = BatchSize)
        {
            var encrypter = _encryptionFactory.Create(request.Encryption);
            var fileName = GetFileName(type, DateTime.UtcNow, encrypter.FileExtension);
            var fullFileName = request.BlobPath + fileName;

            using var stream = _storage.OpenWrite(request.ContainerName, fullFileName);

            await encrypter.Encrypt(stream);
            _writer.Open(stream.Stream, Encoding.UTF8);

            var offset = 0;
            var qsEntities = (await aggregator(offset, batchSize)).ToList();

            while (qsEntities.Count > 0)
            {
                var lasoEntities = qsEntities.Select(transform);

                _writer.WriteRecords(lasoEntities);

                if (qsEntities.Count < batchSize)
                    break;

                offset += qsEntities.Count;                
                qsEntities = (await aggregator(offset, batchSize)).ToList();
            }
        }

        public string GetFileName(ImportType type, DateTime effectiveDate, string encryptionExtension = "")
        {
            return $"{PartnerIdentifier.Quarterspot}_{PartnerIdentifier.Laso}_NA_{type}_{effectiveDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMdd}.csv{encryptionExtension}";
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
                    if (stream == null)
                        throw new Exception("Failed to load customer ID lookup from resource manifest");

                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();

                    _customerIdLookup = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(json);
                }

                return _customerIdLookup;
            }
        }

        private static string GenerateCustomerId(QsCustomer c)
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
