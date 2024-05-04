using CrudeServer.Models.Contracts;

namespace CrudeServer.HttpCommands.Contract
{
    public interface IHttpViewResponse : IHttpResponse
    {
        object ViewModel { get; set; }

        ICommandContext CommandContext { get; set; }

        void SetTemplatePath(string templatePath);
    }
}
