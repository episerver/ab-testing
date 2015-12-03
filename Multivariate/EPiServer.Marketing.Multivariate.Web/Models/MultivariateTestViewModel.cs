using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Web;

namespace EPiServer.Marketing.Multivariate.Web.Models
{
    public class MultivariateTestViewModel
    {
        [Required]
        [Display(Name="Test Title")]
        public string TestTitle { get; set; }
        [Required(ErrorMessage = "A start Date and Time is required")]
        [Display(Name="Test Start")]
        [StartDate(ErrorMessage="Start date cannot be in the past")]
        public DateTime TestStart { get; set; }
        [Required(ErrorMessage = "A start Date and Time is required")]
        [Display(Name="Test Stop")]
        [EndDate(StartDate= "TestStart",ErrorMessage = "End date cannot be before Start Date")]
        public DateTime TestStop { get; set; }
        public int OriginalItemId { get; set; }
        public int VariantItemId { get; set; }
        public int ConversionItemId { get; set; }
    }

    
}
