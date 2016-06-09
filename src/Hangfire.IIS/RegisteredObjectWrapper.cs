using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;

namespace Hangfire.IIS
{
    internal class RegisteredObjectWrapper : IRegisteredObject
    {
        private readonly object _syncRoot = new object();
        private bool _started;

        private IDisposable[] _disposables;

        public void Start(Func<IEnumerable<IDisposable>> configuration)
        {
            lock (_syncRoot)
            {
                if (_started) return;
                _started = true;

                // TODO: Where to call it, before or after configuration?
                HostingEnvironment.RegisterObject(this);

                // TODO: What to do in case of an exception?
                // TODO: We need to iterate it here.
                _disposables = configuration().ToArray();
            }
        }

        public void Stop(bool immediate)
        {
            // TODO: Should work correctly even if Initialize method was not called
            // TODO: Check for immediate or not?
            lock (_syncRoot)
            {
                // TODO: Check _started or not?

                // TODO: What to do with exceptions on Dispose? ObjectDisposed may be thrown.
                // TODO: Other disposables may be also here with own exceptions
                if (_disposables != null)
                {
                    foreach (var disposable in _disposables)
                    {
                        disposable.Dispose();
                    }
                }

                // TODO: Shoult it be called in immediate only?
                // TODO: Where to call it, before or after disposal?
                HostingEnvironment.UnregisterObject(this);
            }
        }
    }
}