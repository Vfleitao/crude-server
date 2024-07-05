using System;
using System.Collections.Generic;

using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Providers.Responses
{
    public class StandardResponseRegistry : IStandardResponseRegistry
    {
        private readonly Dictionary<DefaultStatusCodes, Type> responseCodeToTypeMapping = new Dictionary<DefaultStatusCodes, Type>() {
            { DefaultStatusCodes.BadRequest, typeof(BadRequestResponse) },
            { DefaultStatusCodes.Unauthorized, typeof(UnauthorizedResponse) },
            { DefaultStatusCodes.Forbidden, typeof(ForbiddenResponse) },
            { DefaultStatusCodes.NotFound, typeof(NotFoundResponse) },
            { DefaultStatusCodes.InternalError, typeof(InternalErrorResponse) },
        };

        public StandardResponseRegistry()
        {
        }

        public Type GetResponseType(DefaultStatusCodes statusCode)
        {
            if (responseCodeToTypeMapping.ContainsKey(statusCode))
            {
                return responseCodeToTypeMapping[statusCode];
            }

            return null;
        }

        public void ReplaceResponseInstance<T>(DefaultStatusCodes statusCode) where T : IHttpResponse
        {
            if (!responseCodeToTypeMapping.ContainsKey(statusCode))
            {
                return;
            }

            responseCodeToTypeMapping[statusCode] = typeof(T);
        }
    }
}
