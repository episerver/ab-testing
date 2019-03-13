using System;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// The ICacheSignal interface defines a component that is capable
    /// of signaling a cache's validity.
    /// </summary>
    public interface ICacheSignal
    {
        /// <summary>
        /// Registers a callback which will be invoked when a
        /// the cache has been invalidated.
        /// </summary>
        /// <param name="onInvalidation"></param>
        void Monitor(Action onInvalidation);

        /// <summary>
        /// Signals that the cache is invalid.
        /// </summary>
        void Reset();

        /// <summary>
        /// Signals that the cache is now valid.
        /// </summary>
        void Set();
    }
}
