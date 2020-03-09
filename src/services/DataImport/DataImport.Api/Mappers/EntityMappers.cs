using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Laso.DataImport.Core.Extensions;
using Laso.DataImport.Domain.Entities;
using Enum = System.Enum;

namespace Laso.DataImport.Api.Mappers
{
    // net core DI doesn't do convention based DI, so in order to be able to sweep all 
    // of the converters up in a single query (as opposed to adding each implementation 
    // manually), we do a little dance here and add a few types. To add a new converter,
    // just implement one at the end of the file.
    public interface IEntityMapperFactory
    {
        IEntityMapper<T1, T2> Create<T1, T2>();
    }

    public class EntityMapperFactory : IEntityMapperFactory
    {
        private readonly IEnumerable<IEntityMapper> _mappers;

        public EntityMapperFactory(IEnumerable<IEntityMapper> mappers)
        {
            _mappers = mappers;
        }

        public IEntityMapper<T1, T2> Create<T1, T2>()
        {
            var mapper = _mappers.SingleOrDefault(m => m.GetType().GetInterfaces().Any(i => i == typeof(IEntityMapper<T1, T2>)));

            if (mapper == null)
                throw new NotImplementedException($"No mapping exists from {typeof(T1)} to {typeof(T2)}");

            return (IEntityMapper<T1, T2>)mapper;
        }
    }

    // base interface to allow gathering all of these up in a single collection
    public interface IEntityMapper
    {
    }

    public interface IEntityMapper<in TFrom, out TTo> : IEntityMapper
    {
        TTo Map(TFrom obj);
    }

    public class ImportSubscriptionEntityMapper : IEntityMapper<ImportSubscriptionModel, ImportSubscription>
    {
        public ImportSubscription Map(ImportSubscriptionModel model)
        {
            var subscription = new ImportSubscription
            {
                PartnerId = model.PartnerId,
                EncryptionType = model.EncryptionType.MapByName<Domain.Entities.EncryptionType>(),
                Frequency = model.Frequency.MapByName<Domain.Entities.ImportFrequency>(),
                Imports = model.Imports.Select(i => i.MapByName<Domain.Entities.ImportType>()),
                OutputFileType = model.OutputFileFormat.MapByName<Domain.Entities.FileType>(),
                IncomingStorageLocation = model.IncomingStorageLocation,
                IncomingFilePath = model.IncomingFilePath,
                LastSuccessfulImport = model.LastSuccessfulImport?.ToDateTime(),
                NextScheduledImport = model.NextScheduledImport?.ToDateTime()
            };

            if (model.Id != null)
                subscription.Id = model.Id;

            return subscription;
        }
    }

    public class ImportSubscriptionApiMapper : IEntityMapper<ImportSubscription, ImportSubscriptionModel>
    {
        public ImportSubscriptionModel Map(ImportSubscription entity)
        {
            var model = new ImportSubscriptionModel
            {
                Id = entity.Id,
                PartnerId = entity.PartnerId,
                EncryptionType = entity.EncryptionType.MapByName<EncryptionType>(),
                Frequency = entity.Frequency.MapByName<ImportFrequency>(),
                OutputFileFormat = entity.OutputFileType.MapByName<FileType>(),
                IncomingStorageLocation = entity.IncomingStorageLocation,
                IncomingFilePath = entity.IncomingFilePath,
                LastSuccessfulImport = entity.LastSuccessfulImport.HasValue ? Timestamp.FromDateTime(entity.LastSuccessfulImport.Value) : null,
                NextScheduledImport = entity.NextScheduledImport.HasValue ? Timestamp.FromDateTime(entity.NextScheduledImport.Value) : null
            };

            model.Imports.AddRange(entity.Imports.Select(i => i.MapByName<ImportType>()));

            return model;
        }
    }

    public class ImportHistoryEntityMapper : IEntityMapper<ImportHistoryModel, ImportHistory>
    {
        public ImportHistory Map(ImportHistoryModel model)
        {
            var history = new ImportHistory
            {
                Completed = model.Completed.ToDateTime(),
                SubscriptionId = model.SubscriptionId,
                Success = model.Success,
                FailReasons = model.FailReasons,
                Imports = model.Imports.Select(i => i.MapByName<Domain.Entities.ImportType>())
            };

            if (model.Id != null)
                history.Id = model.Id;

            return history;
        }
    }

    public class ImportHistoryApiMapper : IEntityMapper<ImportHistory, ImportHistoryModel>
    {
        public ImportHistoryModel Map(ImportHistory entity)
        {
            var model = new ImportHistoryModel
            {
                Id = entity.Id,
                Completed = Timestamp.FromDateTime(entity.Completed),
                SubscriptionId = entity.SubscriptionId,
                Success = entity.Success
            };

            model.FailReasons.AddRange(entity.FailReasons);
            model.Imports.AddRange(entity.Imports.Select(i => i.MapByName<ImportType>()));

            return model;
        }
    }
}
