namespace CrudeServer.HttpCommands.Responses
{
    public class BadRequestResponse : StatusCodeResponse
    {
        public override int StatusCode { get; set; } = 400;
    }
}