using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public interface IKeyResult
    {
        Guid Id { get; set; }

        Guid KpiId { get; set; }
    }
}
