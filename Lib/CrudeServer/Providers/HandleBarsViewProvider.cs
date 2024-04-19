using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CrudeServer.Consts;
using CrudeServer.Providers.Contracts;

using HandlebarsDotNet;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Providers
{
    public class HandleBarsViewProvider : ITemplatedViewProvider
    {
        private const string LAYOUT_REGEX = @"@@layout\s+(\S+)(?=\s|$)";
        private const string PARTIAL_REGEX = @"@@partial\s+(\S+)(?=\s|$)";

        private readonly Regex layoutRegex = new Regex(LAYOUT_REGEX, RegexOptions.Compiled);
        private readonly Regex partialRegex = new Regex(PARTIAL_REGEX, RegexOptions.Compiled);

        private readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> _compiledViewCache;
        private readonly Assembly _viewAssembly;
        private readonly string _viewRoot;

        public HandleBarsViewProvider(
           [FromKeyedServices(ServerConstants.VIEW_ASSEMBLY)] Assembly fileAssembly,
           [FromKeyedServices(ServerConstants.VIEW_ROOT)] string fileRoot
       )
        {
            this._viewAssembly = fileAssembly;
            this._viewRoot = fileRoot;

            this._compiledViewCache = new ConcurrentDictionary<string, HandlebarsTemplate<object, object>>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<string> GetTemplate(string templatePath, object data)
        {
            HandlebarsTemplate<object, object> compiledTemplate = null;

            if (this._compiledViewCache.ContainsKey(templatePath))
            {
                compiledTemplate = this._compiledViewCache[templatePath] as HandlebarsTemplate<object, object>;
            }
            else
            {
                string template = await GetTemplateStringFromFiles(templatePath);
                compiledTemplate = Handlebars.Compile(template);

                this._compiledViewCache.TryAdd(templatePath, compiledTemplate);
            }

            return compiledTemplate(data);
        }

        private async Task<string> GetTemplateStringFromFiles(string templatePath)
        {
            string resourceName = $"{this._viewRoot}.{templatePath.Replace("\\", ".").Replace("/", ".")}";
            string wantedResource = this._viewAssembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(resourceName));
            string templateString = null;

            if (string.IsNullOrEmpty(wantedResource))
            {
                return templateString;
            }

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

            if (templateString.Contains("@@layout") && layoutRegex.IsMatch(templateString))
            {
                Match match = layoutRegex.Match(templateString);
                string layoutPath = match.Groups[1].Value;

                string layoutTemplate = await GetTemplateStringFromFiles(layoutPath);

                templateString = Regex.Replace(
                    templateString,
                    LAYOUT_REGEX,
                    string.Empty,
                    RegexOptions.Multiline
                ).Trim();

                layoutTemplate = layoutTemplate.Replace("@@body", templateString);

                templateString = layoutTemplate;
            }

            if (templateString.Contains("@@partial"))
            {
                while (partialRegex.IsMatch(templateString))
                {
                    Match match = partialRegex.Match(templateString);
                    string layoutPath = match.Groups[1].Value;

                    string partial = await GetTemplateStringFromFiles(layoutPath);

                    templateString = templateString.Replace($"@@partial {layoutPath}", partial);
                }
            }

            return templateString;
        }
    }
}
