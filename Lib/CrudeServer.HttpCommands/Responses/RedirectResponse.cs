using System.Text;

namespace CrudeServer.HttpCommands.Responses
{
    public class RedirectResponse : StatusCodeResponse
    {
        public override int StatusCode { get; set; } = 302;

        public RedirectResponse(string location, int statusCode = 302)
        {
            this.StatusCode = statusCode;
            this.ResponseData = Encoding.UTF8.GetBytes(location);
        }
    }
}
