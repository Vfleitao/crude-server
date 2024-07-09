using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
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

        public HttpCommandRegistration RegisterCommand(Type commandType, string path, HttpMethod httpMethod)
        {
            return RegisterCommandOrFunction(path, httpMethod, commandType, null);
        }

        public HttpCommandRegistration RegisterCommand<T>(string path, HttpMethod httpMethod) where T : HttpCommand
        {
            return this.RegisterCommand(typeof(T), path, httpMethod);
        }

        public HttpCommandRegistration RegisterCommandFunction(string path, HttpMethod httpMethod, Delegate delegateFunction)
        {
            MethodInfo methodInfo = delegateFunction.Method;
            Type returnType = methodInfo.ReturnType;

            if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Task<>))
            {
                throw new ArgumentException("Delegate must return a Task<IHttpResponse> or a type that inherits from IHttpResponse.");
            }

            Type genericArgument = returnType.GetGenericArguments()[0];
            if (!typeof(IHttpResponse).IsAssignableFrom(genericArgument))
            {
                throw new ArgumentException("Delegate must return a Task<IHttpResponse> or a type that inherits from IHttpResponse.");
            }

            return RegisterCommandOrFunction(path, httpMethod, null, delegateFunction);
        }

        private HttpCommandRegistration RegisterCommandOrFunction(string path, HttpMethod httpMethod, Type? command, Delegate delegateFunction)
        {
            string key = $"{path}_${httpMethod}";

            if (this._commandRegistry.ContainsKey(key))
            {
                throw new ArgumentException($"Command with path {path} already registered");
            }

            HttpCommandRegistration httpCommandRegistration = new HttpCommandRegistration
            {
                Path = path,
                HttpMethod = httpMethod,
                PathRegex = new Regex(GetRegexPath(path)),
                UrlParameters = GetParameterRegistrations(path),
                CommandFunction = delegateFunction,
                Command = command
            };

            this._commandRegistry.Add(key, httpCommandRegistration);
            if (command != null)
            {
                this._services.AddScoped(command);
            }

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
