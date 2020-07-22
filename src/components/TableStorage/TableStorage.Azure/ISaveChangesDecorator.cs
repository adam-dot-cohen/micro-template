using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Laso.TableStorage.Azure
{
    public interface ISaveChangesDecorator
    {
        Task<ICollection<TableResult>> DecorateAsync(SaveChangesContext context);
    }

    public class SaveChangesContext
    {
        public SaveChangesContext(ITableStorageContext context, Func<Task<ICollection<TableResult>>> saveChanges)
        {
            Context = context;
            SaveChanges = saveChanges;
        }

        public ITableStorageContext Context { get; set; }
        public Func<Task<ICollection<TableResult>>> SaveChanges { get; set; }
    }
}