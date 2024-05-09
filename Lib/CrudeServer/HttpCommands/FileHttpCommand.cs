using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using CrudeServer.Consts;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.HttpCommands
{
    public class FileHttpCommand : HttpCommand
    {
        private readonly IDictionary<string, byte[]> _cache;

        private readonly string _fileRoot;
        private readonly IServerConfig serverConfig;
        private readonly ILogger loggerProvider;
        private readonly IStandardResponseRegistry standardResponseProvider;
        private readonly FileExtensionContentTypeProvider _fileExtentionProvider;

        public FileHttpCommand(
            [FromKeyedServices(ServerConstants.FILE_ROOT)] string fileRoot,
            IServerConfig serverConfig,
            ILogger loggerProvider,
            IStandardResponseRegistry standardResponseProvider
        )
        {
            this._fileRoot = fileRoot;
            this.serverConfig = serverConfig;
            this.loggerProvider = loggerProvider;
            this.standardResponseProvider = standardResponseProvider;
            this._fileExtentionProvider = new FileExtensionContentTypeProvider();
            this._cache = new Dictionary<string, byte[]>();
        }

        protected async override Task<IHttpResponse> Process()
        {
            try
            {
                string requestedFile = this.RequestContext.RequestUrl.LocalPath.Substring(1);
                string resourceName = $"{this._fileRoot}/{requestedFile}";

                if (!File.Exists(resourceName))
                {
                    loggerProvider.Log($"[FileHttpCommand] Resource {resourceName} not found");
                    return new NotFoundResponse();
                }

                byte[] fileData;

                if (this.serverConfig.EnableServerFileCache && this._cache.ContainsKey(resourceName))
                {
                    fileData = this._cache[resourceName];
                }
                else
                {
                    fileData = await File.ReadAllBytesAsync(resourceName);

                    if (fileData == null)
                    {
                        return new NotFoundResponse();
                    }

                    if (this.serverConfig.EnableServerFileCache)
                    {
                        this._cache.TryAdd(resourceName, fileData);
                    }
                }

                return new OkResponse()
                {
                    StatusCode = 200,
                    ResponseData = fileData,
                    ContentType = _fileExtentionProvider.TryGetContentType(resourceName, out string contentType) ?
                                            contentType :
                                            "application/octet-stream",
                    Headers = new Dictionary<string, string>()
                    {
                        { "Cache-Control", $"max-age={this.serverConfig.CachedDurationMinutes}" }
                    }
                };
            }
            catch (Exception ex)
            {
                this.loggerProvider.Error($"[FileHttpCommand] An error occurred while reading the embedded resource", ex);
                return new StatusCodeResponse()
                {
                    StatusCode = 500
                };
            }
        }
    }
}
