using CrudeServer.Enums;
using CrudeServer.HttpCommands;

namespace CrudeServer.CommandRegistration.Contracts
{
    public interface ICommandRegistry
    {
        /// <summary>
        /// Register a command with against a path
        /// It will also automatically register the command withing the Service Provider as a Singleton for automatic IOC resolution
        /// </summary>
        void RegisterCommand<T>(string path, HttpMethod httpMethod) where T : HttpCommand, new();
    }
}