using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using HandlebarsDotNet;

namespace CrudeServer.Providers
{
    public abstract class BaseHandleBarsViewProvider : ITemplatedViewProvider
    {
        private const string LAYOUT_REGEX = @"@@layout\s+(\S+)(?=\s|$)";
        private const string PARTIAL_REGEX = @"@@partial\s+(\S+)(?=\s|$)";

        private readonly Regex layoutRegex = new Regex(LAYOUT_REGEX, RegexOptions.Compiled);
        private readonly Regex partialRegex = new Regex(PARTIAL_REGEX, RegexOptions.Compiled);

        private readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> _compiledViewCache;
        private readonly IServerConfig serverConfig;

        public BaseHandleBarsViewProvider(IServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
            this._compiledViewCache = new ConcurrentDictionary<string, HandlebarsTemplate<object, object>>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<string> GetTemplate(string templatePath, object data)
        {
            HandlebarsTemplate<object, object> compiledTemplate = null;

            if (this.serverConfig.EnableServerFileCache && this._compiledViewCache.ContainsKey(templatePath))
            {
                compiledTemplate = this._compiledViewCache[templatePath] as HandlebarsTemplate<object, object>;
            }
            else
            {
                string template = await GetTemplateStringFromFiles(templatePath);
                compiledTemplate = Handlebars.Compile(template);
                
                if (this.serverConfig.EnableServerFileCache)
                {
                    this._compiledViewCache.TryAdd(templatePath, compiledTemplate);
                }
            }

            return compiledTemplate(data);
        }

        protected abstract Task<string> GetTemplateFile(string templatePath);

        protected async Task<string> GetTemplateStringFromFiles(string templatePath)
        {
            string templateString = await GetTemplateFile(templatePath);

            if (string.IsNullOrEmpty(templateString))
            {
                return null;
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
