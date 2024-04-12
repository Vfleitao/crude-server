using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using CrudeServer.Consts;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.HttpCommands
{
    public class FileHttpCommand : HttpCommand
    {
        private readonly Assembly _fileAssembly;
        private readonly string _fileRoot;
        private readonly FileExtensionContentTypeProvider fileExtentionProvider;

        public FileHttpCommand(
            [FromKeyedServices(ServerConstants.FILE_ASSEMBLY)] Assembly fileAssembly,
            [FromKeyedServices(ServerConstants.FILE_ROOT)] string fileRoot
        )
        {
            this._fileAssembly = fileAssembly;
            this._fileRoot = fileRoot;

            this.fileExtentionProvider = new FileExtensionContentTypeProvider();
        }

        public async override Task<IHttpResponse> Process()
        {
            try
            {
                string resourceName = $"{this._fileRoot}.{this.Request.Url.LocalPath.Substring(1).Replace("\\", ".")}";
                string wantedResource = this._fileAssembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(resourceName));

                if (string.IsNullOrEmpty(wantedResource))
                {
                    return new NotFoundResponse();
                }

                using (Stream resourceStream = this._fileAssembly.GetManifestResourceStream(wantedResource))
                {
                    if (resourceStream == null)
                    {
                        Console.WriteLine("Resource not found. Make sure the resource name is spelled correctly.");
                        return new NotFoundResponse();
                    }

                    // Create a MemoryStream to read the resource into a byte array
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await resourceStream.CopyToAsync(ms);
                        byte[] data = ms.ToArray();

                        return new OkResponse()
                        {
                            StatusCode = 200,
                            ResponseData = data,
                            ContentType = fileExtentionProvider.TryGetContentType(resourceName, out string contentType) ?
                                                contentType :
                                                "application/octet-stream"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while reading the embedded resource: " + ex.Message);
                return new StatusCodeResponse()
                {
                    StatusCode = 500
                };
            }
        }
    }
}
