namespace CrudeServer.Providers.Contracts
{
    public interface ILoggerProvider
    {
        void Log(string message);
        void Log(string format, params object[] args);
    }
}
