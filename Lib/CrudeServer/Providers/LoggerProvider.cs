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

        public void Error(Exception ex)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }

        public void Error(string message, Exception ex)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }

        public void Error(string format, Exception ex, params object[] args)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(format, args);
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
    }
}
