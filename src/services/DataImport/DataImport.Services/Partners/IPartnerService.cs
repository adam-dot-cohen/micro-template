using System;
using System.Collections.Generic;
using System.Text;
using DataImport.Core.Configuration;
using DataImport.Domain.Api;

namespace DataImport.Services.Partners
{
    public interface IPartnerService
    {
        Partner Get(string id);
        Partner GetByInternalId(PartnerIdentifier internalIdentifier);
        IEnumerable<Partner> GetAll();
        void Create(Partner partner);
        void Update(Partner partner);
        void Delete(Partner partner);
        void Delete(string id);
    }

    public class PartnerService : IPartnerService
    {
        private readonly IConnectionStringsConfiguration _config;

        public PartnerService(IConnectionStringsConfiguration config)
        {
            _config = config;
        }

        public Partner Get(string id)
        {
            throw new NotImplementedException();
        }

        public Partner GetByInternalId(PartnerIdentifier internalIdentifier)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Partner> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Create(Partner partner)
        {
            throw new NotImplementedException();
        }

        public void Delete(Partner partner)
        {
            throw new NotImplementedException();
        }

        public void Delete(string id)
        {
            throw new NotImplementedException();
        }      

        public void Update(Partner partner)
        {
            throw new NotImplementedException();
        }
    }
}
