using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laso.DataImport.Api.Mappers;
using Laso.DataImport.Services;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Laso.DataImport.Core.Extensions;
using Laso.DataImport.Domain.Entities;
using Laso.DataImport.Services.Persistence;
using Microsoft.Extensions.Logging;

namespace Laso.DataImport.Api.Services
{
    public class ImportService : Importer.ImporterBase
    {
        private readonly ILogger<ImportService> _logger;
        private readonly IDataImporterFactory _importerFactory;
        private readonly IPartnerService _partnerService;
        private readonly IEntityMapperFactory _mapperFactory;
        private readonly ITableStorageService _tableStorage;

        public ImportService(
            ILogger<ImportService> logger, 
            IDataImporterFactory importerFactory, 
            IPartnerService partnerService,
            IEntityMapperFactory mapperFactory,
            ITableStorageService tableStorage)
        {
            _logger = logger;
            _importerFactory = importerFactory;
            _partnerService = partnerService;
            _mapperFactory = mapperFactory;
            _tableStorage = tableStorage;
        }

        public override async Task<BeginImportReply> BeginImport(BeginImportRequest request, ServerCallContext context)
        {
            var partner = await _partnerService.GetAsync(request.PartnerId);
            var importer = _importerFactory.Create(partner.InternalIdentifier);

            //var historyRequest = new CreateImportHistoryRequest { SubscriptionId = sub.Id };
            
            try
            {
                var op = new ImportOperation
                {
                    Imports = request.Imports.Select(i => i.MapByName<Domain.Entities.ImportType>()).ToList(),
                    BlobPath = request.OutputFilePath,
                    ContainerName = request.OutputContainerName,
                    Encryption = request.Encryption.MapByName<Domain.Entities.EncryptionType>(),
                    DateFilter = request.UpdatedAfter?.ToDateTime()
                };

                await importer.ImportAsync(op);
            }
            catch (AggregateException ex)
            {
                //foreach (var message in ex.InnerExceptions.Select(e => e.Message))
                //{
                //    historyRequest.FailReasons.Add(message);
                //}
                var status = new Status(StatusCode.Internal, string.Join(", ", ex.InnerExceptions.Select(e => e.Message)));
                throw new RpcException(status);
            }

            //historyRequest.Completed = Timestamp.FromDateTime(DateTime.UtcNow);
            //historyRequest.Success = historyRequest.FailReasons.Any();
            //sub.Imports.ForEach(i => historyRequest.Imports.Add(i));

            //await CreateImportHistory(historyRequest, context);

            return new BeginImportReply();
        }

        public override Task<GetImportSubscriptionReply> GetImportSubscription(GetImportSubscriptionRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<GetImportSubscriptionsByPartnerIdReply> GetImportSubscriptionsByPartnerId(GetImportSubscriptionsByPartnerIdRequest request, ServerCallContext context)
        {
            var response = new GetImportSubscriptionsByPartnerIdReply();

            var subscription = new GetImportSubscriptionReply
            {
                Id = "1",
                PartnerId = request.PartnerId,
                Frequency = GetImportSubscriptionReply.Types.ImportFrequency.Weekly,
                OutputFileFormat = GetImportSubscriptionReply.Types.FileType.Csv,
                EncryptionType = EncryptionType.Pgp,
                IncomingStorageLocation = "insights",
                IncomingFilePath = "partner-Quarterspot/incoming/"
            };

            subscription.Imports.Add(ImportType.Demographic);
            //subscription.Imports.Add(ImportType.Firmographic);
            //subscription.Imports.Add(ImportType.Account);
            //subscription.Imports.Add(ImportType.AccountTransaction);
            //subscription.Imports.Add(ImportType.LoanAccount);
            //subscription.Imports.Add(ImportType.LoanApplication);

            response.Subscriptions.Add(subscription);

            return Task.FromResult(response);
        }

        public override async Task<CreateImportHistoryReply> CreateImportHistory(CreateImportHistoryRequest request, ServerCallContext context)
        {
            var mapper = _mapperFactory.Create<CreateImportHistoryRequest, ImportHistory>();
            var history = mapper.Map(request);

            await _tableStorage.InsertAsync(history);

            return new CreateImportHistoryReply();
        }
    }
}
