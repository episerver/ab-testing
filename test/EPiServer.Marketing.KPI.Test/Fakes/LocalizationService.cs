using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using EPiServer.Framework.Localization;

namespace EPiServer.Marketing.KPI.Test.Fakes
{
    /// <summary>
    /// Can't mock this, so make our own mocked version of it for unit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FakeLocalizationService : LocalizationService
    {
        public static readonly FakeLocalizationService Instance = new FakeLocalizationService(null);
        private readonly string _constantReturnString;


        public FakeLocalizationService(string constantReturnString) : base(null)
        {
            _constantReturnString = constantReturnString;
        }

        public override IEnumerable<CultureInfo> AvailableLocalizations => Enumerable.Empty<CultureInfo>();

        protected override IEnumerable<ResourceItem> GetAllStringsByCulture(string originalKey, string[] normalizedKey, CultureInfo culture)
        {
            return Enumerable.Empty<ResourceItem>();
        }

        protected override string LoadString(string[] normalizedKey, string originalKey, CultureInfo culture)
        {
            return _constantReturnString;
        }
    }
}
