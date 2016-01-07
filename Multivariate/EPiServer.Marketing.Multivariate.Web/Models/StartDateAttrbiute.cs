using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Multivariate.Web.Models
{
    class StartDateAttribute : ValidationAttribute
    {
        public DateTime currentDate { get; set; }

        public StartDateAttribute()
        {
            currentDate = DateTime.Now;
        }

        public override bool IsValid(object value)
        {
            if (value != null)
            {
                DateTime date = (DateTime) value;
                if (date >= currentDate)
                {
                    return true;
                }

                return false;
            }
            return false;
        }
    }
}
