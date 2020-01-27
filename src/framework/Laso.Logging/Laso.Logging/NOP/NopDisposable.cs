using System;

namespace Laso.Logging.NOP
{
    public class NopDisposable:IDisposable
    {
        bool _disposed = false;
   
        public void Dispose()
        { 
            Dispose(true);
            GC.SuppressFinalize(this);           
        }
   
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return; 
      
            _disposed = true;
        }

        ~NopDisposable()
        {
            Dispose(false);
        }
    }
}
