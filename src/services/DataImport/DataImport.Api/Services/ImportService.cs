using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Api.Mappers;
using DataImport.Services.DTOs;
using DataImport.Services.Imports;
using DataImport.Services.Partners;
using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace DataImport.Api.Services
{
    public class ImportService : Importer.ImporterBase
    {
        private readonly ILogger<ImportService> _logger;
        private readonly IDataImporterFactory _importerFactory;
        private readonly IPartnerService _partnerService;
        private readonly IDtoMapperFactory _mapperFactory;

        public ImportService(
            ILogger<ImportService> logger, 
            IDataImporterFactory importerFactory, 
            IPartnerService partnerService,
            IDtoMapperFactory mapperFactory)
        {
            _logger = logger;
            _importerFactory = importerFactory;
            _partnerService = partnerService;
            _mapperFactory = mapperFactory;
        }

        public override async Task<ImportReply> BeginImport(ImportRequest request, ServerCallContext context)
        {
            var partner = await _partnerService.GetAsync(request.PartnerId);
            var importer = _importerFactory.Create(partner.InternalIdentifier);
            var response = await GetImportSubscriptionsByPartnerId(new GetImportSubscriptionsByPartnerIdRequest { PartnerId = partner.Id }, context);
            var mapper = _mapperFactory.Create<ImportSubscription, ImportSubscriptionDto>();
            var errors = new List<Exception>();

            foreach (var sub in response.Subscriptions)
            {
                string[] failReasons = null;
                var dto = mapper.Map(sub);

                try
                {
                    await importer.ImportAsync(dto);
                }
                catch (AggregateException ex)
                {
                    failReasons = ex.InnerExceptions.Select(e => e.Message).ToArray();
                    errors.AddRange(ex.InnerExceptions);
                }

                //await CreateImportHistory(new ImportHistory
                //{
                //    Completed = DateTime.UtcNow,
                //    SubscriptionId = sub.Id,
                //    Success = failReasons.Any(),
                //    FailReasons = failReasons
                //});
            }

            if (errors.Any())
                throw new RpcException(new Status(StatusCode.Internal, string.Join(", ", errors.Select(ex => ex.Message))));

            return new ImportReply();
        }

        public override Task<ImportSubscription> GetImportSubscription(GetImportSubscriptionRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<ImportSubscriptions> GetImportSubscriptionsByPartnerId(GetImportSubscriptionsByPartnerIdRequest request, ServerCallContext context)
        {
            var response = new ImportSubscriptions();

            var subscription = new ImportSubscription
            {
                Id = "1",
                PartnerId = request.PartnerId,
                Frequency = ImportSubscription.Types.ImportFrequency.Weekly,
                OutputFileFormat = ImportSubscription.Types.FileType.Csv,
                EncryptionType = ImportSubscription.Types.EncryptionType.Pgp,
                IncomingStorageLocation = "insights",
                IncomingFilePath = "partner-Quarterspot/incoming/"
            };

            subscription.Imports.Add(ImportSubscription.Types.ImportType.Demographic);
            subscription.Imports.Add(ImportSubscription.Types.ImportType.Firmographic);

            response.Subscriptions.Add(subscription);

            return Task.FromResult(response);
        }
    }
}
