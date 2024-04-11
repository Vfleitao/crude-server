using System.Threading.Tasks;

namespace CrudeServer.HttpCommands.Contract
{
    public interface IHttpResponse
    {
        /// <summary>
        /// Response Status Code
        /// </summary>
        public byte[] ResponseData { get; set; }

        /// <summary>
        /// Response Status Code
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Response Status Code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Method which processes the internal response and transforms it into a byte array.
        /// This method can also set the ContentType and StatusCode if required.
        /// </summary>
        /// <returns></returns>
        Task ProcessResponse();
    }
}
