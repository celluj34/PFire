using System;

namespace PFire.Core.Util
{
    public abstract class Disposable : IDisposable
    {
        protected bool Disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeManagedResources();
            }

            Disposed = true;
        }

        protected abstract void DisposeManagedResources();
    }
}
