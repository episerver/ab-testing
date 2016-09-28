using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public interface IDalKeyResult
    {
        Guid Id { get; set; }

        Guid KpiId { get; set; }
    }
}
