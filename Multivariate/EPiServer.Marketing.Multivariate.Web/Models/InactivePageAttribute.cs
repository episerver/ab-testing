using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Model.Enums;
using EPiServer.Marketing.Testing;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Multivariate.Web.Models
{
    internal class UnassignedTestAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if (value != null)
            {
                var contentrepo = ServiceLocator.Current.GetInstance<IContentRepository>();
                var testManagerRepo = ServiceLocator.Current.GetInstance<IMultivariateTestManager>();
                var itemValue = int.Parse(value.ToString());

                // need to get guid for pages from the page picker content id's we get
                var contentGuid = contentrepo.Get<PageData>(new ContentReference(itemValue)).ContentGuid;
                if (testManagerRepo.GetTestByItemId(contentGuid).All(x => x.TestState != TestState.Active))
                {
                    return ValidationResult.Success;
                }

            }
            return new ValidationResult("This item is already assigned to an active test.");

        }
    }
}
