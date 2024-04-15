using System.Collections.Generic;

namespace CrudeServer.HttpCommands.Contract
{
    public interface IHttpViewResponse : IHttpResponse
    {
        object ViewModel { get; set; }

        void SetTemplatePath(string templatePath);
    }
}
