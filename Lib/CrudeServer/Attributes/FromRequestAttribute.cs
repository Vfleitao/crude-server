using System;

namespace CrudeServer.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromRequestAttribute : Attribute
    {
    }
}
