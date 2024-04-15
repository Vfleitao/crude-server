using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Middleware
{
    public class LoggerMiddleware : IMiddleware
    {
        private static int _counter = 0;

        public async Task Process(IRequestContext context, Func<Task> next)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Request #{++_counter}");
            sb.AppendLine(context.Url.ToString());
            sb.AppendLine(context.HttpMethod.ToString());
            sb.AppendLine(context.Host);
            sb.AppendLine(context.UserAgent);
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
