using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using EPiServer.ServiceLocation;
using EPiServer.Core;

namespace EPiServer.Marketing.Multivariate.Web.Models
{
    public class VisiblePageAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value != null)
            {
                var contentrepo = ServiceLocator.Current.GetInstance<IContentRepository>();

                var itemValue = int.Parse(value.ToString());
                // need to get guid for pages from the page picker content id's we get
                var itemRef = contentrepo.Get<PageData>(new ContentReference(itemValue));

                return PageDataExtensions.IsVisibleOnSite(itemRef);
            }
            return false;
        }
    }
}
