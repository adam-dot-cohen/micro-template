using System;
using System.Collections.Generic;
using System.Text;

namespace Laso.DataImport.Core.Common
{
    public class DisposeAction : IDisposable
    {
        public static DisposeAction None => new DisposeAction(null);

        private readonly Action _disposeAction;
        private bool _isDisposed;

        public DisposeAction(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _disposeAction?.Invoke();
        }
    }

    public class DisposeAction<T> : DisposeAction
    {
        public DisposeAction(T item, Action disposeAction) : base(disposeAction)
        {
            Item = item;
        }

        public T Item { get; }
    }
}
