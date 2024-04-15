using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.HttpCommands.Responses
{
    public class ViewResponse : IHttpViewResponse
    {
        private readonly ITemplatedViewProvider templatedViewProvider;
        private string TemplatePath { get; set; }
        
        public byte[] ResponseData { get; set; }
        public string ContentType { get; set; } = "text/html";
        public int StatusCode { get; set; } = 200;
        public IDictionary<string, object> Items { get; set; }
        public object ViewModel { get; set; }

        public ViewResponse(ITemplatedViewProvider templatedViewProvider)
        {
            this.templatedViewProvider = templatedViewProvider;
        }

        public void SetTemplatePath(string templatePath)
        {
            TemplatePath = templatePath;
        }

        public async Task ProcessResponse()
        {
            string template = await templatedViewProvider.GetTemplate(TemplatePath, new { 
                viewModel = this.ViewModel,
                viewData = this.Items
            });

            if (template == null)
            {
                throw new InvalidOperationException(string.Format("Could not parse template {0}", TemplatePath));
            }

            ResponseData = System.Text.Encoding.UTF8.GetBytes(template);
        }
    }
}
