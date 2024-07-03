using System;

using CrudeServer.Enums;

namespace CrudeServer.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RequiresAntiforgeryAttribute : Attribute
    {
        public RequiresAntiforgeryAttribute()
        {
        }
    }
}
