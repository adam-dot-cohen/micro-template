using System;
using System.Collections.Generic;
using System.Text;

namespace Laso.Logging
{
    public interface ILogContext
    {
        Guid CorrelationId { get; set; }
        Guid ScopeId { get; set; }
        bool IsNestedScope { get; }
        IEnumerable<AssociatedEntity> AssociatedEntities { get; }
        void AddAssociatedEntity(AssociatedEntity associatedEntity);
        void AddAssociatedEntities(IEnumerable<AssociatedEntity> associatedEntities);
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> Properties { get; }
        void AddProperty(string key, string value);
    }
}
