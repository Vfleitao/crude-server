using System.IO;
using System.Threading.Tasks;

using CrudeServer.Consts;
using CrudeServer.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CrudeServer.Providers
{
    public class FileHandleBarsViewProvider : BaseHandleBarsViewProvider
    {
        private readonly string _viewRoot;

        public FileHandleBarsViewProvider(
            [FromKeyedServices(ServerConstants.VIEW_ROOT)] string fileRoot,
            IOptions<ServerConfiguration> serverConfig
        ) : base(serverConfig)
        {
            this._viewRoot = fileRoot;
        }

        protected override async Task<string> GetTemplateFile(string templatePath)
        {
            string requestedFile = templatePath;
            string resourceName = $"{this._viewRoot}/{requestedFile}";

            if (!File.Exists(resourceName))
            {
                return null;
            }

            return await File.ReadAllTextAsync(resourceName);
        }
    }
}
