using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPiServer.Cmo.Core.Entities;

namespace EPiServer.Cmo.Tests.MockTypes
{
    public class TestLpoHelper : ILpoHelper
    {
        public List<LpoTestLight> Tests { get; set; }

        public TestLpoHelper()
        {
            Tests = new List<LpoTestLight>();
        }

        #region ILpoHelper Members

        public LpoTestLight GetTestByOriginal(string reference)
        {
            return Tests.FirstOrDefault(test => test.Original.Reference == reference);
        }

        public LpoTestLight GetTestByConversion(string reference)
        {
            return Tests.FirstOrDefault(test => test.Conversion.Reference == reference);
        }

        public void Flush()
        {
            Tests.Clear();
        }

        public List<string> GetAllPageReferencesInTests()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
