using System;
using System.Collections.Generic;
using System.Linq;
using DataImport.Core.Extensions;
using DataImport.Services.DTOs;

namespace DataImport.Api.Mappers
{
    // net core DI doesn't do convention based DI, so in order to be able to sweep all 
    // of the converters up in a single query (as opposed to adding each implementation 
    // manually), we do a little dance here and add a few types. To add a new converter,
    // just implement one at the end of the file.
    public interface IDtoMapperFactory
    {
        IDtoMapper<TApi, TDto> Create<TApi, TDto>() where TDto : Dto<string>;
    }

    public class DtoMapperFactory : IDtoMapperFactory
    {
        private readonly IEnumerable<IDtoMapper> _mappers;

        public DtoMapperFactory(IEnumerable<IDtoMapper> mappers)
        {
            _mappers = mappers;
        }

        public IDtoMapper<TApi, TDto> Create<TApi, TDto>() where TDto : Dto<string>
        {

            var mapper = _mappers.SingleOrDefault(m => m.GetType().GetInterfaces().Any(i => i == typeof(IDtoMapper<TApi, TDto>)));

            if (mapper == null)
                throw new NotImplementedException($"No mapping exists from {typeof(TApi)} to {typeof(TDto)}");

            return (IDtoMapper<TApi, TDto>)mapper;
        }
    }

    // base interface to allow gathering all of these up in a single collection
    public interface IDtoMapper
    {
    }

    public interface IDtoMapper<TApi, TDto> : IDtoMapper
        where TDto : Dto<string>
    {
        TDto Map(TApi obj);
    }

    public class ImportSubscriptionMapper : IDtoMapper<ImportSubscription, ImportSubscriptionDto>
    {
        public ImportSubscriptionDto Map(ImportSubscription obj)
        {
            return new ImportSubscriptionDto
            {
                Id = obj.Id,
                PartnerId = obj.PartnerId,
                EncryptionType = obj.EncryptionType.MapByName<EncryptionTypeDto>(),
                Frequency = obj.Frequency.MapByName<ImportFrequencyDto>(),
                Imports = obj.Imports.Select(i => i.MapByName<ImportTypeDto>()),
                OutputFileType = obj.OutputFileFormat.MapByName<FileTypeDto>(),
                IncomingStorageLocation = obj.IncomingStorageLocation,
                IncomingFilePath = obj.IncomingFilePath,
                LastSuccessfulImport = obj.LastSuccessfulImport?.ToDateTime(),
                NextScheduledImport = obj.NextScheduledImport?.ToDateTime()
            };
        }
    }
}
