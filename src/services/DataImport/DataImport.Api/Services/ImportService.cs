using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laso.DataImport.Api.Mappers;
using Laso.DataImport.Services;
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
            var response = new BeginImportReply();

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
                foreach (var message in ex.InnerExceptions.Select(e => e.Message))
                {
                    response.FailReasons.Add(message);
                }
            }

            response.Success = !response.FailReasons.Any();

            return response;
        }

        public override async Task<GetImportSubscriptionReply> GetImportSubscription(GetImportSubscriptionRequest request, ServerCallContext context)
        {
            var sub = await _tableStorage.GetAsync<ImportSubscription>(request.Id);
            if (sub == null)
                throw new RpcException(new Status(StatusCode.NotFound, request.Id));

            var mapper = _mapperFactory.Create<ImportSubscription, ImportSubscriptionModel>();

            return new GetImportSubscriptionReply
            {
                Subscription = mapper.Map(sub)
            };
        }

        public override async Task<GetAllImportSubscriptionsReply> GetAllImportSubscriptions(GetAllImportSubscriptionsRequest request, ServerCallContext context)
        {
            var subs = await _tableStorage.GetAllAsync<ImportSubscription>();
            var mapper = _mapperFactory.Create<ImportSubscription, ImportSubscriptionModel>();

            var reply = new GetAllImportSubscriptionsReply();
            reply.Subscriptions.AddRange(subs.Select(mapper.Map));

            return reply;
        }

        public override Task<GetImportSubscriptionsByPartnerIdReply> GetImportSubscriptionsByPartnerId(GetImportSubscriptionsByPartnerIdRequest request, ServerCallContext context)
        {
            var response = new GetImportSubscriptionsByPartnerIdReply();

            var subscription = new ImportSubscriptionModel
            {
                Id = "1",
                PartnerId = request.PartnerId,
                Frequency = ImportFrequency.Weekly,
                OutputFileFormat = FileType.Csv,
                EncryptionType = EncryptionType.Pgp,
                IncomingStorageLocation = "insights",
                IncomingFilePath = "partner-Quarterspot/incoming/"
            };

            subscription.Imports.Add(ImportType.Demographic);
            subscription.Imports.Add(ImportType.Firmographic);
            subscription.Imports.Add(ImportType.Account);
            subscription.Imports.Add(ImportType.AccountTransaction);
            subscription.Imports.Add(ImportType.LoanAccount);
            subscription.Imports.Add(ImportType.LoanApplication);

            response.Subscriptions.Add(subscription);

            return Task.FromResult(response);
        }

        public override async Task<CreateImportSubscriptionReply> CreateImportSubscription(CreateImportSubscriptionRequest request, ServerCallContext context)
        {
            var mapper = _mapperFactory.Create<ImportSubscriptionModel, ImportSubscription>();
            var sub = mapper.Map(request.Subscription);
            
            await _tableStorage.InsertAsync(sub);

            return new CreateImportSubscriptionReply { Id = sub.Id };
        }

        public override async Task<CreateImportHistoryReply> CreateImportHistory(CreateImportHistoryRequest request, ServerCallContext context)
        {
            var mapper = _mapperFactory.Create<ImportHistoryModel, ImportHistory>();
            var history = mapper.Map(request.History);

            await _tableStorage.InsertAsync(history);

            return new CreateImportHistoryReply { Id = history.Id };
        }

        public override async Task<UpdateImportSubscriptionReply> UpdateImportSubscription(UpdateImportSubscriptionRequest request, ServerCallContext context)
        {
            var mapper = _mapperFactory.Create<ImportSubscriptionModel, ImportSubscription>();
            var sub = mapper.Map(request.Subscription);

            await _tableStorage.InsertOrReplaceAsync(sub);

            return new UpdateImportSubscriptionReply();
        }
    }
}
