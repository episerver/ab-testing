using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Personalization;

namespace EPiServer.Marketing.Multivariate
{
    public class CurrentUser : ICurrentUser
    {
        public string GetDisplayName()
        {
            return EPiServerProfile.Current.DisplayName;
        }
    }
}
