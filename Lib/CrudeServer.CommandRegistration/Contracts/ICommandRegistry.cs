using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.Models;

namespace CrudeServer.CommandRegistration.Contracts
{
    public interface ICommandRegistry
    {
        /// <summary>
        /// Get a command from the registry for a given path and http method
        /// </summary>
        /// <param name="path"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        HttpCommandRegistration GetCommand(string path, HttpMethod httpMethod);

        /// <summary>
        /// Register a command with against a path
        /// It will also automatically register the command withing the Service Provider as a Singleton for automatic IOC resolution
        /// </summary>
        HttpCommandRegistration RegisterCommand<T>(string path, HttpMethod httpMethod) where T : HttpCommand, new();
    }
}