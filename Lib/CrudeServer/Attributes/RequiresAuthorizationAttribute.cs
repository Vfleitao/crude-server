using System;

namespace CrudeServer.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RequiresAuthorizationAttribute : Attribute
    {
        public string[] Roles { get; }

        public RequiresAuthorizationAttribute(params string[] roles)
        {
            Roles = roles;
        }

    }
}
