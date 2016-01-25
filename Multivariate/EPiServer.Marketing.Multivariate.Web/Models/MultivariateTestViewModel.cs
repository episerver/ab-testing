using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;

namespace EPiServer.Marketing.Multivariate.Web.Models
{
    public class MultivariateTestViewModel
    {

        public Guid id { get; set; }

        [Required(ErrorMessage = "A title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "A start date and time is required")]
        [StartDate(ErrorMessage = "Start date cannot be in the past")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "An end date and time is required")]
        [EndDate(StartDate = "StartDate", ErrorMessage = "End date cannot be before Start Date")]
        public DateTime? EndDate { get; set; }

        public string Owner { get; set; }

        public TestState testState { get; set; }

        [Required(ErrorMessage = "An origin item is required")]
        [VisiblePage(ErrorMessage = "The original item is not a page that can be displayed on the site.")]
        public int OriginalItem { get; set; }
        public Guid OriginalItemId { get; set; }
        public string OriginalItemDisplay { get; set; }
    

        [Required(ErrorMessage = "A variant item is required")]
        [VisiblePage(ErrorMessage = "The variant item is not a page that can be displayed on the site.")]
        public int VariantItem { get; set; }
        public Guid VariantItemId { get; set; }
        public string VariantItemDisplay { get; set; }

        public string LastModifiedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }


        public IList<MultivariateTestResult> TestResults { get; set; }
    }
}
