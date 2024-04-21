using System;

namespace CrudeServer.Providers.Contracts
{
    public interface ILogger
    {
        void Log(string message);
        void Log(string format, params object[] args);
        void Error(Exception ex);
        void Error(string message, Exception ex);
        void Error(string format, Exception ex, params object[] args);
    }
}
