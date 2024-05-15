using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;

using Microsoft.Extensions.Options;

namespace CrudeServer.Middleware
{
    public class RequestSizeLimiterMiddleware : IMiddleware
    {
        private readonly IOptions<ServerConfiguration> serverConfig;

        public RequestSizeLimiterMiddleware(IOptions<ServerConfiguration> serverConfig)
        {
            this.serverConfig = serverConfig;
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            long maxRequestSize = this.serverConfig.Value.MaxRequestSizeMB * 1024 * 1024;

            if (IsRequestOverMaxSize(context.HttpListenerRequest, maxRequestSize))
            {
                context.Response = new BadRequestResponse();
                return;
            }

            await next();
        }


        private bool IsRequestOverMaxSize(HttpListenerRequest request, long maxSize)
        {
            if(request.ContentLength64 > 0 && request.ContentLength64 > maxSize)
            {
                return true;
            }

            Stream inputStream = request.InputStream;
            byte[] buffer = new byte[1024];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                totalBytesRead += bytesRead;
                if(totalBytesRead > maxSize)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
