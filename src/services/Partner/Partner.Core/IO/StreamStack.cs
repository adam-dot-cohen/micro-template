using System;
using System.IO;
using System.Collections.Generic;

namespace Partner.Core.IO
{
    public class StreamStack : IDisposable
    {
        private readonly Stack<IDisposable> _disposers = new Stack<IDisposable>();

        public Stream Stream { get; private set; }

        public StreamStack(IDisposable disposer)
        {
            Push(disposer);
        }

        public void Push(params IDisposable[] disposers)
        {
            foreach (var disposer in disposers)
            {
                if (disposer is Stream stream)
                    Stream = stream;

                _disposers.Push(disposer);
            }
        }

        public void Dispose()
        {
            while (_disposers.Count > 0)
                _disposers.Pop().Dispose();
        }
    }
}
