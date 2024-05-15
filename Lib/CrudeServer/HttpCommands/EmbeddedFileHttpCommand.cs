using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using CrudeServer.Consts;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CrudeServer.HttpCommands
{
    public class EmbeddedFileHttpCommand : HttpCommand
    {
        private readonly IDictionary<string, byte[]> _cache;

        private readonly Assembly _fileAssembly;
        private readonly string _fileRoot;
        private readonly IOptions<ServerConfiguration> serverConfig;
        private readonly ILogger loggerProvider;
        private readonly FileExtensionContentTypeProvider _fileExtentionProvider;

        public EmbeddedFileHttpCommand(
            [FromKeyedServices(ServerConstants.FILE_ASSEMBLY)] Assembly fileAssembly,
            [FromKeyedServices(ServerConstants.FILE_ROOT)] string fileRoot,
            IOptions<ServerConfiguration> serverConfig,
            ILogger loggerProvider,
            ICommandContext requestContext
        ) : base(requestContext)
        {
            this._fileAssembly = fileAssembly;
            this._fileRoot = fileRoot;
            this.serverConfig = serverConfig;
            this.loggerProvider = loggerProvider;
            this._fileExtentionProvider = new FileExtensionContentTypeProvider();
            this._cache = new Dictionary<string, byte[]>();
        }

        protected async override Task<IHttpResponse> Process()
        {
            try
            {
                string resourceName = $"{this._fileRoot}.{this.RequestContext.RequestUrl.LocalPath.Substring(1).Replace("\\", ".").Replace("/", ".")}";
                string wantedResource = this._fileAssembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(resourceName));

                if (string.IsNullOrEmpty(wantedResource))
                {
                    loggerProvider.Log($"[EmbeddedFileHttpCommand] Resource {resourceName} not found");
                    return new NotFoundResponse();
                }

                byte[] fileData;

                if (this.serverConfig.Value.EnableServerFileCache && this._cache.ContainsKey(wantedResource))
                {
                    fileData = this._cache[wantedResource];
                }
                else
                {
                    fileData = await GetData(wantedResource);

                    if (fileData == null)
                    {
                        return new NotFoundResponse();
                    }

                    if (this.serverConfig.Value.EnableServerFileCache)
                    {
                        this._cache.TryAdd(wantedResource, fileData);
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
                        { "Cache-Control", $"max-age={this.serverConfig.Value.CachedDurationMinutes}" }
                    }
                };
            }
            catch (Exception ex)
            {
                this.loggerProvider.Error($"[EmbeddedFileHttpCommand] An error occurred while reading the embedded resource", ex);
                return new StatusCodeResponse()
                {
                    StatusCode = 500
                };
            }
        }

        private async Task<byte[]> GetData(string wantedResource)
        {
            using (Stream resourceStream = this._fileAssembly.GetManifestResourceStream(wantedResource))
            {
                if (resourceStream == null)
                {
                    return null;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    await resourceStream.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
