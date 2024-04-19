using System;
using System.IO;
using System.Threading;

using CrudeServer.Providers.Contracts;

using Newtonsoft.Json;

namespace CrudeServer.Providers
{
    public class LoggerProvider : ILoggerProvider
    {
        private static SemaphoreSlim SEMAPHORE = new SemaphoreSlim(1, 1);

        private const string LOG_FILE = "log.txt";

        public LoggerProvider()
        {
        }

        public void Log(string message)
        {
            SEMAPHORE.Wait();
            try
            {
                Console.WriteLine(message);
                File.AppendAllText(LOG_FILE, message);
            }
            finally
            {
                SEMAPHORE.Release();
            }
        }

        public void Log(string format, params object[] args)
        {
            SEMAPHORE.Wait();
            try
            {
                Console.WriteLine(format, args);
                File.AppendAllText(LOG_FILE, string.Format(format, args));
            }
            finally
            {
                SEMAPHORE.Release();
            }
        }

        public void Error(Exception ex)
        {
            SEMAPHORE.Wait();
            try
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();

                File.AppendAllText(LOG_FILE, JsonConvert.SerializeObject(ex));
            }
            finally
            {
                SEMAPHORE.Release();
            }
        }

        public void Error(string message, Exception ex)
        {
            SEMAPHORE.Wait();
            try
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();

                File.AppendAllText(LOG_FILE, message);
            }
            finally
            {
                SEMAPHORE.Release();
            }
        }

        public void Error(string format, Exception ex, params object[] args)
        {
            SEMAPHORE.Wait();
            try
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(format, args);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();

                File.AppendAllText(LOG_FILE, string.Format(format, args));
            }
            finally
            {
                SEMAPHORE.Release();
            }
        }
    }
}
