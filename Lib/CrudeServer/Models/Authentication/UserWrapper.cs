using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace CrudeServer.Models.Authentication
{
    public class UserWrapper
    {
        private readonly IPrincipal principal;

        public bool IsAuthenticated
        {
            get { return this.principal.Identity.IsAuthenticated; }
        }

        public string UserName
        {
            get { return this.principal.Identity.Name; }
        }

        public ClaimsPrincipal Principal
        {
            get { return this.principal as ClaimsPrincipal; }
        }

        public List<string> Roles
        {
            get
            {
                List<string> roles = new List<string>();

                if (Principal == null)
                {
                    return roles;
                }

                foreach (Claim claim in Principal.Claims)
                {
                    if (claim.Type == ClaimTypes.Role)
                    {
                        roles.Add(claim.Value);
                    }
                }

                return roles;
            }
        }

        public string Id
        {
            get
            {
                if (!IsAuthenticated)
                {
                    return null;
                }

                Claim claim = Principal.FindFirst(ClaimTypes.Sid);
                if (claim == null)
                {
                    return null;
                }

                return claim.Value;
            }
        }

        public UserWrapper(IPrincipal principal)
        {
            this.principal = principal;
        }

        public bool IsInRole(string role)
        {
            if (Principal == null)
            {
                return false;
            }

            return Principal.IsInRole(role);
        }

    }
}
