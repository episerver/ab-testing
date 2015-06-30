using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Multivariate
{
    public class TestResult
    {
        public Guid ItemId { get; set; }
        public int Views { get; set; }
        public int Conversions { get; set; }
    }
}
