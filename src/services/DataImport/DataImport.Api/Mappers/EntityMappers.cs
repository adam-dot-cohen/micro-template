using System;
using System.Collections.Generic;
using System.Linq;
using Laso.DataImport.Api.Extensions;
using Laso.DataImport.Core.Extensions;
using Laso.DataImport.Domain.Entities;

namespace Laso.DataImport.Api.Mappers
{
    // net core DI doesn't do convention based DI, so in order to be able to sweep all 
    // of the converters up in a single query (as opposed to adding each implementation 
    // manually), we do a little dance here and add a few types. To add a new converter,
    // just implement one at the end of the file.
    public interface IEntityMapperFactory
    {
        IEntityMapper<Tapi, Tentity> Create<Tapi, Tentity>() where Tentity : TableStorageEntity;
    }

    public class EntityMapperFactory : IEntityMapperFactory
    {
        private readonly IEnumerable<IEntityMapper> _mappers;

        public EntityMapperFactory(IEnumerable<IEntityMapper> mappers)
        {
            _mappers = mappers;
        }

        public IEntityMapper<Tapi, Tentity> Create<Tapi, Tentity>() where Tentity : TableStorageEntity
        {

            var mapper = _mappers.SingleOrDefault(m => m.GetType().GetInterfaces().Any(i => i == typeof(IEntityMapper<Tapi, Tentity>)));

            if (mapper == null)
                throw new NotImplementedException($"No mapping exists from {typeof(Tapi)} to {typeof(Tentity)}");

            return (IEntityMapper<Tapi, Tentity>)mapper;
        }
    }

    // base interface to allow gathering all of these up in a single collection
    public interface IEntityMapper
    {
    }

    public interface IEntityMapper<in Tapi, out Tentity> : IEntityMapper
        where Tentity : TableStorageEntity
    {
        Tentity Map(Tapi obj);
    }

    public class ImportSubscriptionMapper : IEntityMapper<GetImportSubscriptionReply, ImportSubscription>
    {
        public ImportSubscription Map(GetImportSubscriptionReply obj)
        {
            return new ImportSubscription
            {
                PartnerId = obj.PartnerId,
                EncryptionType = obj.EncryptionType.MapByName<EncryptionType>(),
                Frequency = obj.Frequency.MapByName<ImportFrequency>(),
                Imports = obj.Imports.Select(i => i.MapByName<Domain.Entities.ImportType>().ToString()),
                OutputFileType = obj.OutputFileFormat.MapByName<FileType>(),
                IncomingStorageLocation = obj.IncomingStorageLocation,
                IncomingFilePath = obj.IncomingFilePath,
                LastSuccessfulImport = obj.LastSuccessfulImport?.ToDateTime(),
                NextScheduledImport = obj.NextScheduledImport?.ToDateTime()
            };
        }
    }

    public class ImportHistoryMapper : IEntityMapper<CreateImportHistoryRequest, ImportHistory>
    {
        public ImportHistory Map(CreateImportHistoryRequest request)
        {
            return new ImportHistory
            {
                Completed = request.Completed.ToDateTime(),
                SubscriptionId = request.SubscriptionId,
                Success = request.Success,
                FailReasons = request.FailReasons,
                Imports = request.Imports.Select(i => i.MapByName<Domain.Entities.ImportType>().ToString())
            };
        }
    }
}
