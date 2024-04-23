using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.Models;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.CommandRegistration
{
    public class CommandRegistry : ICommandRegistry
    {
        private readonly IServiceCollection _services;
        private readonly Dictionary<string, HttpCommandRegistration> _commandRegistry = new Dictionary<string, HttpCommandRegistration>();

        public CommandRegistry(IServiceCollection services)
        {
            this._services = services;
        }

        public HttpCommandRegistration RegisterCommand<T>(string path, HttpMethod httpMethod) where T : HttpCommand
        {
            string key = $"{path}_${httpMethod}";

            if (this._commandRegistry.ContainsKey(key))
            {
                throw new ArgumentException($"Command with path {path} already registered");
            }

            Type commandType = typeof(T);



            HttpCommandRegistration httpCommandRegistration = new HttpCommandRegistration
            {
                Path = path,
                HttpMethod = httpMethod,
                Command = commandType,
                PathRegex = new Regex(GetRegexPath(path)),
                UrlParameters = GetParameterRegistrations(path)
            };

            this._commandRegistry.Add(key, httpCommandRegistration);
            this._services.AddScoped(commandType);

            return httpCommandRegistration;
        }

        public HttpCommandRegistration GetCommand(string path, HttpMethod httpMethod)
        {
            IEnumerable<KeyValuePair<string, HttpCommandRegistration>> allEntriesForMethod = this._commandRegistry.Where(x => x.Value.HttpMethod == httpMethod);
            foreach (KeyValuePair<string, HttpCommandRegistration> entry in allEntriesForMethod)
            {
                if (entry.Value.PathRegex.IsMatch(path))
                {
                    return entry.Value;
                }
            }

            return null;
        }

        private string GetRegexPath(string path)
        {
            string pattern = "{\\w+:\\\\([^}]+)}";
            string replacement = "(\\$1)";
            string regexPath = Regex.Replace(path, pattern, replacement);

            return $"^{regexPath}$";
        }

        private List<KeyValuePair<string, string>> GetParameterRegistrations(string path)
        {
            string pattern = "{(\\w+):(\\\\[^}]+)}";

            MatchCollection matches = Regex.Matches(path, pattern);
            List<KeyValuePair<string, string>> parameterPatterns = new List<KeyValuePair<string, string>>();

            foreach (Match match in matches)
            {
                parameterPatterns.Add(
                    new KeyValuePair<string, string>(match.Groups[1].Value, match.Groups[2].Value)
                );
            }

            return parameterPatterns;
        }
    }
}
