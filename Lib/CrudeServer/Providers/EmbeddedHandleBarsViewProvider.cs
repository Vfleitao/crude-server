using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using CrudeServer.Consts;
using CrudeServer.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CrudeServer.Providers
{
    public class EmbeddedHandleBarsViewProvider : BaseHandleBarsViewProvider
    {
        private readonly Assembly _viewAssembly;
        private readonly string _viewRoot;

        public EmbeddedHandleBarsViewProvider(
           [FromKeyedServices(ServerConstants.VIEW_ASSEMBLY)] Assembly fileAssembly,
           [FromKeyedServices(ServerConstants.VIEW_ROOT)] string fileRoot,
           IOptions<ServerConfiguration> serverConfig
       ) : base(serverConfig)
        {
            this._viewAssembly = fileAssembly;
            this._viewRoot = fileRoot;
        }

        protected override async Task<string> GetTemplateFile(string templatePath)
        {
            string resourceName = $"{this._viewRoot}.{templatePath.Replace("\\", ".").Replace("/", ".")}";
            string[] allEmbededResources = this._viewAssembly.GetManifestResourceNames();
            string wantedResource = allEmbededResources.FirstOrDefault(x => x.EndsWith(resourceName));

            if (wantedResource == null) {
                resourceName = resourceName.Replace("-", "_");
                wantedResource = allEmbededResources.FirstOrDefault(x => x.EndsWith(resourceName));
            }

            if (string.IsNullOrEmpty(wantedResource))
            {
                return null;
            }

            string templateString = null;
            using (Stream resourceStream = this._viewAssembly.GetManifestResourceStream(wantedResource))
            {
                if (resourceStream == null)
                {
                    return null;
                }

                using (StreamReader ms = new StreamReader(resourceStream))
                {
                    templateString = await ms.ReadToEndAsync();
                }
            }

            return templateString;
        }
    }
}
