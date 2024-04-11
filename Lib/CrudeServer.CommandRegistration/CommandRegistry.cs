using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        public void RegisterCommand<T>(string path, HttpMethod httpMethod) where T : HttpCommand, new()
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
                Command = commandType
            };

            this._commandRegistry.Add(key, httpCommandRegistration);
            this._services.AddSingleton(commandType);
        }

        public HttpCommandRegistration GetCommand(string path, HttpMethod httpMethod)
        {
            string key = $"{path}_${httpMethod}";

            if (!this._commandRegistry.ContainsKey(key))
            {
                return null;
            }

            return this._commandRegistry[key];
        }
    }
}
