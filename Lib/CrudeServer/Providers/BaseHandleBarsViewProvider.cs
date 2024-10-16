﻿using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;
using CrudeServer.Providers.Utilities;

using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;

using Microsoft.Extensions.Options;

namespace CrudeServer.Providers
{
    public abstract class BaseHandleBarsViewProvider : ITemplatedViewProvider
    {
        private static IHandlebars handlebarsContext;

        private static IHandlebars HandlebarsContext
        {
            get
            {
                if (handlebarsContext == null)
                {
                    handlebarsContext = Handlebars.CreateSharedEnvironment(new HandlebarsConfiguration()
                    {
                        ThrowOnUnresolvedBindingExpression = false
                    });

                    handlebarsContext.AddToArrayHelper();

                    HandlebarsHelpers.Register(handlebarsContext);
                }

                return handlebarsContext;
            }
        }

        private const string LAYOUT_REGEX = @"@@layout\s+(\S+)(?=\s|$)";
        private const string PARTIAL_REGEX = @"@@partial\s+(\S+)(?=\s|$)";

        private readonly Regex layoutRegex = new Regex(LAYOUT_REGEX, RegexOptions.Compiled);
        private readonly Regex partialRegex = new Regex(PARTIAL_REGEX, RegexOptions.Compiled);

        private readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> _compiledViewCache;
        private readonly IOptions<ServerConfiguration> serverConfig;

        protected BaseHandleBarsViewProvider(IOptions<ServerConfiguration> serverConfig)
        {
            this.serverConfig = serverConfig;
            this._compiledViewCache = new ConcurrentDictionary<string, HandlebarsTemplate<object, object>>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<string> GetTemplate(string templatePath, object data, ICommandContext commandContext)
        {
            HandlebarsTemplate<object, object> compiledTemplate = null;

            if (this.serverConfig.Value.EnableServerFileCache && this._compiledViewCache.ContainsKey(templatePath))
            {
                compiledTemplate = this._compiledViewCache[templatePath] as HandlebarsTemplate<object, object>;
            }
            else
            {
                (string template, bool eligibleForCache) templateResult = await GetTemplateStringFromFiles(templatePath, commandContext);

                if (string.IsNullOrEmpty(templateResult.template))
                {
                    return null;
                }

                compiledTemplate = HandlebarsContext.Compile(templateResult.template);

                if (templateResult.eligibleForCache && this.serverConfig.Value.EnableServerFileCache)
                {
                    this._compiledViewCache.TryAdd(templatePath, compiledTemplate);
                }
            }

            return compiledTemplate(data);
        }

        protected abstract Task<string> GetTemplateFile(string templatePath);

        protected async Task<(string template, bool eligibleForCache)> GetTemplateStringFromFiles(string templatePath, ICommandContext commandContext)
        {
            bool canCache = true;

            string templateString = await GetTemplateFile(templatePath);

            if (string.IsNullOrEmpty(templateString))
            {
                return (null, false);
            }

            if (templateString.Contains("@@layout") && layoutRegex.IsMatch(templateString))
            {
                (string template, bool eligibleForCache) templateResult = await HandleLayout(templateString, commandContext);

                templateString = templateResult.template;
                canCache = canCache && templateResult.eligibleForCache;
            }

            if (templateString.Contains("@@partial"))
            {
                (string template, bool eligibleForCache) partialProcess = await HandlePartials(templateString, commandContext);
                templateString = partialProcess.template;
                canCache = canCache && partialProcess.eligibleForCache;
            }

            if (templateString.Contains("@@antiforgerytoken"))
            {
                canCache = false;
                templateString = HandleAntiforgeryToken(templateString, commandContext);
            }

            return (templateString, canCache);
        }

        private string HandleAntiforgeryToken(string templateString, ICommandContext commandContext)
        {
            string inputName = this.serverConfig.Value.AntiforgeryTokenInputName ?? "___csrt";
            string cookieName = this.serverConfig.Value.AntiforgeryTokenCookieName;
            string token = commandContext.GetCookie(cookieName);

            string inputHtml = $"<input type=\"hidden\" name=\"{inputName}\" value=\"{token}\" />";

            while (templateString.Contains("@@antiforgerytoken"))
            {
                templateString = templateString.Replace("@@antiforgerytoken", inputHtml);
            }

            return templateString;
        }

        private async Task<(string template, bool eligibleForCache)> HandlePartials(string templateString, ICommandContext commandContext)
        {
            bool canCache = true;

            while (partialRegex.IsMatch(templateString))
            {
                Match match = partialRegex.Match(templateString);
                string layoutPath = match.Groups[1].Value;

                (string template, bool eligibleForCache) partial = await GetTemplateStringFromFiles(layoutPath, commandContext);

                templateString = templateString.Replace($"@@partial {layoutPath}", partial.template);
                canCache = canCache && partial.eligibleForCache;
            }

            return (templateString, canCache);
        }

        private async Task<(string template, bool eligibleForCache)> HandleLayout(string templateString, ICommandContext commandContext)
        {
            Match match = layoutRegex.Match(templateString);
            string layoutPath = match.Groups[1].Value;

            (string template, bool eligibleForCache) layoutTemplateResult = await GetTemplateStringFromFiles(layoutPath, commandContext);

            templateString = Regex.Replace(
                templateString,
                LAYOUT_REGEX,
                string.Empty,
                RegexOptions.Multiline
            ).Trim();

            layoutTemplateResult.template = layoutTemplateResult.template.Replace("@@body", templateString);

            templateString = layoutTemplateResult.template;

            return (templateString, layoutTemplateResult.eligibleForCache);
        }
    }
}
