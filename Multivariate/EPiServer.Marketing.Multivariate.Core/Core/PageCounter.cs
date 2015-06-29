using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Multivariate.Core
{
    public class PageResult
    {
        public Guid PageId { get; set; }
        public int Views { get; set; }
        public int Conversions { get; set; }
    }
}
