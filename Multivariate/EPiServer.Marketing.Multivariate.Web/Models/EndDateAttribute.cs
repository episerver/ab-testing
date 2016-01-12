using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace EPiServer.Marketing.Multivariate.Web.Models
{
    /// <summary>
    ///     Validation attribute for multivariate test Start Date which
    ///     verifies the start date is not set to a value later than the end date.
    /// </summary>
    public class EndDateAttribute : ValidationAttribute
    {
        public string StartDate { get; set; }

        public override bool IsValid(object value)
        {
            if (value != null)
            {
                var dateStart = HttpContext.Current.Request[StartDate];
                var end = (DateTime) value;
                var start = DateTime.Parse(dateStart);

                return start.Date < end.Date;
            }
            return false;
        }
    }
}