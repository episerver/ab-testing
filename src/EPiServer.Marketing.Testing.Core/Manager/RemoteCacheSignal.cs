using EPiServer.Framework.Cache;
using System;
using System.Threading;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    public class RemoteCacheSignal : ICacheSignal, IDisposable
    {
        private readonly ISynchronizedObjectInstanceCache _cacheToMonitor;
        private readonly string _keyToMonitor;
        private readonly int _frequencyInMilliseconds;

        private Timer _timer;
        private Action _onInvalidation;


        public RemoteCacheSignal(ISynchronizedObjectInstanceCache cacheToMonitor, string keyToMonitor, TimeSpan frequency)
        {
            _cacheToMonitor = cacheToMonitor;
            _keyToMonitor = keyToMonitor;
            _frequencyInMilliseconds = Convert.ToInt32(frequency.TotalMilliseconds);
        }

        public void Monitor(Action onInvalidation)
        {
            if (_timer != null)
            {
                throw new InvalidOperationException("The monitor has already been started.");
            }

            _onInvalidation = onInvalidation;
            _timer = new Timer(PollValidity, null, 0, _frequencyInMilliseconds);
        }

        public void Reset()
        {
            _cacheToMonitor.RemoveRemote(_keyToMonitor);
        }

        public void Set()
        {
            _cacheToMonitor.Insert(_keyToMonitor, true, CacheEvictionPolicy.Empty);
        }

        private void PollValidity(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                if (IsCacheInvalid())
                {
                    _onInvalidation();
                }
            }
            catch
            {
                // Log this...
            }
            finally
            {
                _timer.Change(_frequencyInMilliseconds, _frequencyInMilliseconds);
            }
        }

        private bool IsCacheInvalid()
        {
            return _cacheToMonitor.Get(_keyToMonitor) == null;
        }

        public void Dispose()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
        }
    }
}
