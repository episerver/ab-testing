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

        [Required]
        public string Title { get; set; }

        [Required(ErrorMessage = "A start Date and Time is required")]
        [StartDate(ErrorMessage = "Start date cannot be in the past")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "A start Date and Time is required")]
        [EndDate(StartDate = "StartDate", ErrorMessage = "End date cannot be before Start Date")]
        public DateTime? EndDate { get; set; }

        public string Owner { get; set; }

        public TestState testState { get; set; }

        [Required]
        public int OriginalItem { get; set; }

        public Guid OriginalItemId { get; set; }

        [Required]
        public int VariantItem { get; set; }

        public Guid VariantItemId { get; set; }

        public IList<MultivariateTestResult> TestResults { get; set; } 
    }
}
