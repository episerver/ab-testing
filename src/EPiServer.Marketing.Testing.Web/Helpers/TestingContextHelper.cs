
using System.Web;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public class TestingContextHelper
    {
        public bool IsInSystemFolder()
        {
            return HttpContext.Current.Request.RawUrl.ToLower()
                .Contains(EPiServer.Shell.Paths.ProtectedRootPath.ToLower());
        }
    }
}
