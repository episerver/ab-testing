using System;
using System.Collections.Generic;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Cms.Shell.UI.ObjectEditing;
using EPiServer.Cms.Shell.Extensions;

namespace EPiServer.Marketing.Testing.Web.MetadataExtender
{
    /// <summary>
    /// Sets Episerver content properties to be read only when that content is currently in a pending, active, or completed test
    /// </summary>
    public class MarketingTestMetadataExtender : IMetadataExtender
    {
        public void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            var aContentMetadata = metadata as ContentDataMetadata;

            if (aContentMetadata == null)
                return;

            var content = aContentMetadata.FindOwnerContent();
        }
    }
}
