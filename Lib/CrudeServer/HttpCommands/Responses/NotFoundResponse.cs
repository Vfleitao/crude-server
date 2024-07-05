namespace CrudeServer.HttpCommands.Responses
{
    public class InternalErrorResponse : StatusCodeResponse
    {
        public override int StatusCode { get; set; } = 500;
    }
}
