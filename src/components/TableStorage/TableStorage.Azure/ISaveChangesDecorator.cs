using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables.Models;

namespace Laso.TableStorage.Azure
{
    public interface ISaveChangesDecorator
    {
        Task<ICollection<TableItem>> DecorateAsync(SaveChangesContext context);
    }

    public class SaveChangesContext
    {
        public SaveChangesContext(ITableStorageContext context, Func<Task<ICollection<TableItem>>> saveChanges)
        {
            Context = context;
            SaveChanges = saveChanges;
        }

        public ITableStorageContext Context { get; set; }
        public Func<Task<ICollection<TableItem>>> SaveChanges { get; set; }
    }
}