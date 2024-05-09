using CrudeServer.HttpCommands.Responses;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Demo.Responses
{
    public class NotFoundResponse : ViewResponse
    {
        public NotFoundResponse(ITemplatedViewProvider templatedViewProvider) : base(templatedViewProvider)
        {
            this.StatusCode = 404;
            this.SetTemplatePath("not-found.html");
        }
    }
}
