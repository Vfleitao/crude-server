using System;

using CrudeServer.Providers.Contracts;

namespace CrudeServer.Providers
{
    public class LoggerProvider : ILoggerProvider
    {
        public LoggerProvider()
        {
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Log(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
