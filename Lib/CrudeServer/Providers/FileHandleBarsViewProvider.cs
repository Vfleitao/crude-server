using System.IO;
using System.Threading.Tasks;

using CrudeServer.Consts;
using CrudeServer.Models.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Providers
{
    public class FileHandleBarsViewProvider : BaseHandleBarsViewProvider
    {
        private readonly string _viewRoot;

        public FileHandleBarsViewProvider(
            [FromKeyedServices(ServerConstants.VIEW_ROOT)] string fileRoot,
            IServerConfig serverConfig
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
