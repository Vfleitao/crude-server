namespace CrudeServer.HttpCommands.Responses
{
    public class NotFoundResponse : StatusCodeResponse
    {
        public override int StatusCode { get; set; } = 404;
    }
}
