using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using CrudeServer.Attributes;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;

namespace CrudeServer.HttpCommands
{
    public class HttpFunctionCommand : HttpCommand
    {
        public Delegate DelegateFunction { get; set; }

        public HttpFunctionCommand(ICommandContext requestContext)
            : base(requestContext)
        {
        }

        protected override async Task<IHttpResponse> Process()
        {
            MethodInfo methodInfo = DelegateFunction.Method;

            ParameterInfo[] parameters = methodInfo.GetParameters();
            Console.WriteLine("Parameters:");

            List<object> parametersForFunction = new List<object>();

            foreach (ParameterInfo param in parameters)
            {
                Type type = param.ParameterType;

                FromRequestAttribute fromRequestAttr = param.GetCustomAttribute<FromRequestAttribute>();
                if (fromRequestAttr != null)
                {
                    object dataFromRequest = this.RequestContext.GetModelFromRequest(type);
                    parametersForFunction.Add(dataFromRequest);
                }
                else
                {
                    parametersForFunction.Add(this.RequestContext.Services.GetService(type));
                }
            }

            Task methodTask = (Task)methodInfo.Invoke(DelegateFunction.Target, parametersForFunction.ToArray());
            await methodTask.ConfigureAwait(false);

            PropertyInfo resultProperty = methodTask.GetType().GetProperty("Result");

            IHttpResponse response = resultProperty.GetValue(methodTask) as IHttpResponse;

            return response;
        }
    }
}
