using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Middleware
{
    public class LoggerMiddleware : IMiddleware
    {
        private readonly ILoggerProvider _loggerProvider;

        public LoggerMiddleware(ILoggerProvider loggerProvider)
        {
            this._loggerProvider = loggerProvider;
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(context.RequestUrl.ToString());
            sb.AppendLine(context.RequestHttpMethod.ToString());
            sb.AppendLine(context.RequestHost);
            sb.AppendLine(context.UserAgent);
            sb.AppendLine();

            this._loggerProvider.Log(sb.ToString());

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            await next();

            stopwatch.Stop();

            this._loggerProvider.Log($"Request completed in {0}ms", stopwatch.ElapsedMilliseconds);
        }
    }
}
