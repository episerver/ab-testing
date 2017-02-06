using System;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public interface IKeyResult
    {
        Guid Id { get; set; }

        Guid KpiId { get; set; }

        DateTime CreatedDate { get; set; }

        DateTime ModifiedDate { get; set; }
    }
}
