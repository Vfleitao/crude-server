using System;
using System.Collections.Generic;
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
                PathRegex = new Regex($"^{path}$")
            };

            this._commandRegistry.Add(key, httpCommandRegistration);
            this._services.AddSingleton(commandType);

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
    }
}
