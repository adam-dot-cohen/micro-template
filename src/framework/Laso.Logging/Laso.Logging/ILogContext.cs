using System;
using System.Collections.Generic;
using System.Linq;
using Laso.Logging.Collections;
using Laso.Logging.Extensions;

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


    public class LogContext : ILogContext
    {
        private readonly Func<bool> _nestedScopeResolver;

        public LogContext(Func<bool> nestedScopeResolver = null)
        {
            _nestedScopeResolver = nestedScopeResolver ?? (() => false);
        }

        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public Guid ScopeId { get; set; } = Guid.NewGuid();
        public bool IsNestedScope => _nestedScopeResolver();

        private readonly HashSet<AssociatedEntity> _associatedEntities = new HashSet<AssociatedEntity>();
        private readonly MultiValueDictionary <string, string> _properties = MultiValueDictionary <string, string>.Create<HashSet<string>>();

        public IEnumerable<AssociatedEntity> AssociatedEntities => _associatedEntities;

        public LogContext() { }

        public void AddAssociatedEntity(AssociatedEntity associatedEntity)
        {
            // Discard when already more than 50 associated entities. Have seen 1,000+ OperationStatus entities.
            if (_associatedEntities.Count > 50)
                return;

            _associatedEntities.Add(associatedEntity);
        }

        public void AddAssociatedEntities(IEnumerable<AssociatedEntity> associatedEntities)
        {
            // Discard when already more than 50 associated entities. Have seen 1,000+ OperationStatus entities.
            if (associatedEntities == null || _associatedEntities.Count > 50)
                return;

            // Only continue adding when less than 10 of each associated entity entity type and only add up to 10 at a time.
            // This is to account for scenarios like when 1,000+ bank transactions are saved.
            var grouped = associatedEntities.GroupBy(e => e.Entity, e => e);
            grouped.ForEach(g =>
            {
                var existingEntitiesCount = _associatedEntities.Count(e => e.Entity == g.Key);
                if (existingEntitiesCount < 10)
                {
                    _associatedEntities.AddRange(g.Take(10));
                }
                else
                {
                    // If >= 10, then remove all of that type and add up to 10 more.
                    // This will help when processing large sets of data in a single handler if logging is done along the way.
                    // If logging is done only after all the processing is complete, this won't help.
                    _associatedEntities.RemoveWhere(e => e.Entity == g.Key);
                    _associatedEntities.AddRange(g.Take(10));
                }
            });
        }

        public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Properties => _properties;

        public void AddProperty(string key, string value)
        {
            _properties.Add(key, value);
        }
    }

}
