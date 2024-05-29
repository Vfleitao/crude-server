using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;

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
            StringBuilder builder = new StringBuilder();
            builder.Append("^");

            string[] pathSplits = path.StartsWith("/") ? path.Substring(1).Split('/') : path.Split('/');
            for (int i = 0; i < pathSplits.Length; i++)
            {
                if (pathSplits[i].StartsWith("{") && pathSplits[i].EndsWith("}"))
                {
                    string pathWithoutBrackets = pathSplits[i].Substring(1, pathSplits[i].Length - 2);
                    string regexChunk = pathWithoutBrackets.Substring(pathWithoutBrackets.IndexOf(":") + 1);
                    builder.Append("/(" + regexChunk + ")");
                }
                else
                {
                    builder.Append("/" + pathSplits[i]);
                }
            }

            builder.Append("$");

            return builder.ToString();
        }

        private List<KeyValuePair<string, string>> GetParameterRegistrations(string path)
        {
            List<KeyValuePair<string, string>> parameterPatterns = new List<KeyValuePair<string, string>>();

            string[] pathSplits = path.StartsWith("/") ? path.Substring(1).Split('/') : path.Split('/');
            for (int i = 0; i < pathSplits.Length; i++)
            {
                if (pathSplits[i].StartsWith("{") && pathSplits[i].EndsWith("}"))
                {
                    string pathWithoutBrackets = pathSplits[i].Substring(1, pathSplits[i].Length - 2);
                    string nameChunk = pathWithoutBrackets.Substring(0, pathWithoutBrackets.IndexOf(":"));
                    string regexChunk = pathWithoutBrackets.Substring(pathWithoutBrackets.IndexOf(":") + 1);

                    parameterPatterns.Add(new KeyValuePair<string, string>(nameChunk, regexChunk));
                }
            }


            return parameterPatterns;
        }
    }
}
