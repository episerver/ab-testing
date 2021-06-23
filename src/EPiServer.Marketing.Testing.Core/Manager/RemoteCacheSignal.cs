using EPiServer.Framework.Cache;
using EPiServer.Logging;
using System;
using System.Threading;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// The RemoteCacheSignal is a component that is capable of signaling
    /// the validity of a cache which must be synchronized with other 
    /// remote nodes.
    /// </summary>
    public class RemoteCacheSignal : ICacheSignal
    {
        private readonly ISynchronizedObjectInstanceCache _cacheToMonitor;
        private readonly ILogger _logger;
        private readonly string _keyToMonitor;
        private readonly int _frequencyInMilliseconds;

        private Timer _timer;
        private Action _onInvalidation;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheToMonitor">The cache to monitor for validity</param>
        /// <param name="keyToMonitor">The key whose presence indicates validity</param>
        /// <param name="frequency">The frequency which which the cache should check for validity</param>
        public RemoteCacheSignal(ISynchronizedObjectInstanceCache cacheToMonitor, ILogger logger, string keyToMonitor, TimeSpan frequency)
        {
            _cacheToMonitor = cacheToMonitor;
            _logger = logger;
            _keyToMonitor = keyToMonitor;
            _frequencyInMilliseconds = Convert.ToInt32(frequency.TotalMilliseconds);
        }

        /// <summary>
        /// Registers a callback which will be invoked if a node has indicated that caches
        /// should be invalidated.
        /// </summary>
        /// <param name="onInvalidation">Callback to execute</param>
        public void Monitor(Action onInvalidation)
        {
            if (_timer != null)
            {
                throw new InvalidOperationException("The monitor has already been started.");
            }

            _onInvalidation = onInvalidation;

            _timer = new Timer(PollValidity);
            _timer.Change(0, _frequencyInMilliseconds);
        }

        /// <summary>
        /// Signals to remote nodes that caches should be invalidated.
        /// </summary>
        public void Reset()
        {
            _cacheToMonitor.RemoveRemote(_keyToMonitor);
            _logger.Trace("Signaling remote nodes of cache invalidation.");
        }

        /// <summary>
        /// Signals that this node's cache is in a valid state.
        /// </summary>
        public void Set()
        {
            _cacheToMonitor.Insert(_keyToMonitor, true, CacheEvictionPolicy.Empty);
            _logger.Trace("Signaling cache validity.");
        }

        /// <summary>
        /// Polls for the validity of the cache. If the cache is found to
        /// be invalid, the registered callback will be invoked.
        /// </summary>
        private void PollValidity(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                if (IsCacheInvalid())
                {
                    _logger.Trace("Remote cache has signalled invalidation.");
                    _onInvalidation();
                }
            }
            catch(Exception ex)
            {
                _logger.Error("An error occurred while polling cache validity.", ex);
            }
            finally
            {
                _timer.Change(_frequencyInMilliseconds, _frequencyInMilliseconds);
            }
        }

        /// <summary>
        /// Determines whether the cache has been invalidated.
        /// </summary>
        /// <returns>True if the cache is invalid, false otherwise</returns>
        private bool IsCacheInvalid()
        {
            return _cacheToMonitor.Get(_keyToMonitor) == null;
        }
    }
}
