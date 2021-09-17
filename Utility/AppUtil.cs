using System.Linq;
using System.Security.Principal;

namespace Utility
{
    public class AppUtil
    {
        public static bool IsRunAsAdmin()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var claims = principal.Claims;
            return (claims.FirstOrDefault(c => c.Value == "S-1-5-32-544") != null);
        }
    }
}
