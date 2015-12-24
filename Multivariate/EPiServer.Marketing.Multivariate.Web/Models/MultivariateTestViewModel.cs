using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;

namespace EPiServer.Marketing.Multivariate.Web.Models
{
    public class MultivariateTestViewModel
    {
        public Guid id { get; set; }

        [Required]
        [Display(Name = "Test Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "A start Date and Time is required")]
        [Display(Name = "Test Start")]
        [StartDate(ErrorMessage = "Start date cannot be in the past")]
        public DateTime StartDate { get; set; }

        public string Owner { get; set; }

        public TestState testState { get; set; }

        [Required(ErrorMessage = "A start Date and Time is required")]
        [Display(Name = "Test Stop")]
        [EndDate(StartDate = "StartDate", ErrorMessage = "End date cannot be before Start Date")]
        public DateTime? EndDate { get; set; }

        public Guid OriginalItemId { get; set; }

        public IList<Variant> VariantItems { get; set; }

        public IList<KeyPerformanceIndicator> Conversions { get; set; }

        public IList<MultivariateTestResult> TestResults { get; set; } 
    }
}
