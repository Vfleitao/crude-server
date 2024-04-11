using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.MiddlewareRegistration.Contracts;
using CrudeServer.Models;

namespace CrudeServer.Middleware
{
    public class LoggerMiddleware : IMiddleware
    {
        private static int _counter = 0;

        public async Task Process(RequestContext context, Func<Task> next)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Request #{++_counter}");
            sb.AppendLine(context.Request.Url.ToString());
            sb.AppendLine(context.Request.HttpMethod);
            sb.AppendLine(context.Request.UserHostName);
            sb.AppendLine(context.Request.UserAgent);
            sb.AppendLine();

            Console.Write(sb);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            await next();

            stopwatch.Stop();

            Console.WriteLine($"Request #{_counter} completed in {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
