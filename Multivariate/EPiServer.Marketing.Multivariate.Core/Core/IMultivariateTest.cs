using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;

namespace EPiServer.Marketing.Multivariate
{
    public interface IMultivariateTest
    {
        Guid Id { get; set; }

        string Title { get; set; }
        string Owner { get; set; }
        TestState State { get; set; }

        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }

        Guid OriginalItemId { get; set; }
        Guid VariantItemId { get; set; }
        Guid ConversionItemId { get; set; }

        int Views { get; set; }
        int Conversions { get; set; }
    }
}
