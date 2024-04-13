namespace CrudeServer.Enums
{
    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        DELETE,
        OPTIONS,
        HEAD
    }

    public static class HttpMethodExtensions
    {
        public static string ToHttpString(this HttpMethod method)
        {
            switch (method)
            {
                case HttpMethod.GET:
                    return "GET";
                case HttpMethod.POST:
                    return "POST";
                case HttpMethod.PUT:
                    return "PUT";
                case HttpMethod.DELETE:
                    return "DELETE";
                case HttpMethod.OPTIONS:
                    return "OPTIONS";
                case HttpMethod.HEAD:
                    return "HEAD";
                default:
                    return "GET";
            }
        }

        public static HttpMethod FromHttpString(string method)
        {
            switch (method)
            {
                case "GET":
                    return HttpMethod.GET;
                case "POST":
                    return HttpMethod.POST;
                case "PUT":
                    return HttpMethod.PUT;
                case "DELETE":
                    return HttpMethod.DELETE;
                case "OPTIONS":
                    return HttpMethod.OPTIONS;
                case "HEAD":
                    return HttpMethod.HEAD;
                default:
                    return HttpMethod.GET;
            }
        }
    }
}
