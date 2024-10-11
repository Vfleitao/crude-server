using System;
using System.Collections;
using System.Text;

using HandlebarsDotNet;

namespace CrudeServer.Providers.Utilities
{
    public static class HandleBarsViewExtensions
    {
        public static void AddToArrayHelper(this IHandlebars handlebarsContext)
        {
            handlebarsContext.RegisterHelper("ToArray", (output, options, context, arguments) =>
            {
                if (arguments.Length != 1)
                {
                    Console.WriteLine("ToArray helper requires exactly one argument.");
                    return;
                }

                if (arguments[0] is not IEnumerable)
                {
                    Console.WriteLine("ToArray helper requires an enumerable argument.");
                    return;
                }

                IEnumerable items = arguments[0] as IEnumerable;
                int count = 0;

                StringBuilder sb = new StringBuilder();
                sb.Append("[");

                foreach (object item in items)
                {
                    if (count > 0)
                    {
                        sb.Append(",");
                    }

                    sb.AppendFormat("'{0}'", item.ToString());

                    count++;
                }

                sb.Append("]");

                output.Write(sb.ToString());
            });
        }

    }
}
