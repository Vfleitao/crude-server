using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrudeServer.HttpCommands.Contract
{
    public interface IHttpResponse : IHttpResponse<object> { }

    public interface IHttpResponse<T>
    {
        /// <summary>
        /// Data passed to the view. Contains generic data and is created from the middlewares.
        /// </summary>
        IDictionary<string, object> Items { get; set; }

        /// <summary>
        /// Response Status Code
        /// </summary>
        byte[] ResponseData { get; set; }

        /// <summary>
        /// Response Status Code
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Response Status Code
        /// </summary>
        int StatusCode { get; set; }

        /// <summary>
        /// Method which processes the internal response and transforms it into a byte array.
        /// This method can also set the ContentType and StatusCode if required.
        /// </summary>
        /// <returns></returns>
        Task ProcessResponse();
    }
}
