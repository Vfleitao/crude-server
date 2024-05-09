using System;
using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.Providers.Contracts
{
    public interface IStandardResponseRegistry
    {
        Type GetResponseType(DefaultStatusCodes statusCode);
        void ReplaceResponseInstance<T>(DefaultStatusCodes statusCode) where T : IHttpResponse;
    }
}