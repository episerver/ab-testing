using System;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    public interface ICacheSignal
    {
        void Monitor(Action onInvalidation);
        void Reset();
        void Set();
    }
}
