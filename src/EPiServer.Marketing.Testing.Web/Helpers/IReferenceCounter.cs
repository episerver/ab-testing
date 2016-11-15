using System;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    internal interface IReferenceCounter
    {
        void AddReference(object src);
        void RemoveReference(object src);
        Boolean hasReference(object src);
        int getReferenceCount(object src);
    }
}
