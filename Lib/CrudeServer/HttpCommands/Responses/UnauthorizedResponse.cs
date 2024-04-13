namespace CrudeServer.HttpCommands.Responses
{
    public class UnauthorizedResponse : StatusCodeResponse
    {
        public override int StatusCode { get; set; } = 401;
    }
}
