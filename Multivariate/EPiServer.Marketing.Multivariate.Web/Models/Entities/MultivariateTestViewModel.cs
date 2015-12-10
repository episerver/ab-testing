using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Multivariate.Web.Models.Entities
{
    public class MultivariateTestViewModel2
    {
        [Required]
        [Display(Name = "Test Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "A start Date and Time is required")]
        [Display(Name = "Test Start")]
        [StartDate(ErrorMessage = "Start date cannot be in the past")]
        public DateTime StartDate { get; set; }

        public string Owner { get; set; }

        public int TestState { get; set; }

        [Required(ErrorMessage = "A start Date and Time is required")]
        [Display(Name = "Test Stop")]
        [EndDate(StartDate = "TestStart", ErrorMessage = "End date cannot be before Start Date")]
        public DateTime EndDate { get; set; }

        public Guid OriginalItemId { get; set; }
    }
}
