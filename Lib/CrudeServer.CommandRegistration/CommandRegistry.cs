using System;
using System.Collections.Generic;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.Models;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.CommandRegistration
{
    public class CommandRegistry : ICommandRegistry
    {
        private readonly IServiceCollection services;
        private Dictionary<string, HttpCommandRegistration> _commandRegistry = new Dictionary<string, HttpCommandRegistration>();

        public CommandRegistry(IServiceCollection services)
        {
            this.services = services;
        }

        public void RegisterCommand<T>(string path, HttpMethod httpMethod) where T : HttpCommand, new()
        {
            string key = $"{path}_${httpMethod}";

            if (this._commandRegistry.ContainsKey(path))
            {
                throw new ArgumentException($"Command with path {path} already registered");
            }

            HttpCommandRegistration httpCommandRegistration = new HttpCommandRegistration
            {
                Path = path,
                HttpMethod = httpMethod,
                Command = typeof(T)
            };

            this._commandRegistry.Add(path, typeof(T));
            this.services.AddSingleton(typeof(T));
        }
    }
}
