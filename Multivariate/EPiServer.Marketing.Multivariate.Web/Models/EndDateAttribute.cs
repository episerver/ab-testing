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
                DateTime dateStart;
                if (!DateTime.TryParse(HttpContext.Current.Request[StartDate], out dateStart))
                {
                    return false;
                }

                var end = (DateTime)value;

                return dateStart.Date < end.Date;
            }

            return false;
        }
    }
}