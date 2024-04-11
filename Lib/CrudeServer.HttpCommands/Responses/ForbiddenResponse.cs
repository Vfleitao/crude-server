namespace CrudeServer.HttpCommands.Responses
{
    public class ForbiddenResponse : StatusCodeResponse
    {
        public override int StatusCode { get; set; } = 403;
    }
}
