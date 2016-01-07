using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Marketing.Multivariate.Web.Models
{
    public class EndDateAttribute : ValidationAttribute
    {
        public string StartDate { get; set; }

        public override bool IsValid(object value)
        {
            if (value != null)
            {
                string dateStart = HttpContext.Current.Request[StartDate];
                DateTime end = (DateTime) value;
                DateTime start = DateTime.Parse(dateStart);

                return start < end;
            }
            return false;
        }
    }
}
