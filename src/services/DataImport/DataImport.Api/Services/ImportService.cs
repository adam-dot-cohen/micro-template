using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Api.Mappers;
using DataImport.Services.DTOs;
using DataImport.Services;
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
            var mapper = _mapperFactory.Create<GetImportSubscriptionReply, ImportSubscriptionDto>();
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
                EncryptionType = GetImportSubscriptionReply.Types.EncryptionType.Pgp,
                IncomingStorageLocation = "insights",
                IncomingFilePath = "partner-Quarterspot/incoming/"
            };

            subscription.Imports.Add(GetImportSubscriptionReply.Types.ImportType.Demographic);
            subscription.Imports.Add(GetImportSubscriptionReply.Types.ImportType.Firmographic);

            response.Subscriptions.Add(subscription);

            return Task.FromResult(response);
        }
    }
}
