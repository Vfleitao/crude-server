using System;

using CrudeServer.Enums;

namespace CrudeServer.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        public string PathRegex { get; }
        public HttpMethod Method { get; }

        public CommandAttribute(string pathRegex,HttpMethod method = HttpMethod.GET)
        {
            PathRegex = pathRegex;
            Method = method;
        }
    }
}
