using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using EPiServer.ServiceLocation;
using EPiServer.Core;

namespace EPiServer.Marketing.Testing.Web.Models
{
    public class VisiblePageAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if (value != null)
            {
                if ((int)value == 0)
                {
                    return new ValidationResult("A valid page is required.");
                }

                var contentrepo = ServiceLocator.Current.GetInstance<IContentRepository>();

                var itemValue = int.Parse(value.ToString());

                // need to get guid for pages from the page picker content id's we get
                var itemRef = contentrepo.Get<PageData>(new ContentReference(itemValue));
                
                if (itemRef.IsVisibleOnSite())
                {
                    return ValidationResult.Success;    
                }
                
            }
            return new ValidationResult("The selected item cannot be displayed on the site");




        }


        //public override bool IsValid(object value)
        //{

        //    if (value != null && (int)value != 0)
        //    {
        //        var contentrepo = ServiceLocator.Current.GetInstance<IContentRepository>();

        //        var itemValue = int.Parse(value.ToString());

        //        // need to get guid for pages from the page picker content id's we get
        //        var itemRef = contentrepo.Get<PageData>(new ContentReference(itemValue));

        //        return PageDataExtensions.IsVisibleOnSite(itemRef);
        //    }
        //    return false;
        //}
    }
}
