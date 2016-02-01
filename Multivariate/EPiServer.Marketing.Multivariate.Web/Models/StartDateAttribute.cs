using System;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Marketing.Testing.Web.Models
{
    public class StartDateAttribute : ValidationAttribute
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
                DateTime dateStart;
                if (!DateTime.TryParse(value.ToString(), out dateStart))
                {
                    return false;
                }

                return DateTime.Compare(dateStart, currentDate) > 0;
            }

            return false;
        }
    }
}
