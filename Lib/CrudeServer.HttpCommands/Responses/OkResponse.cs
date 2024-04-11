namespace CrudeServer.HttpCommands.Responses
{
    public class OkResponse : StatusCodeResponse
    {
        public override int StatusCode { get; set; } = 200;
    }
}
