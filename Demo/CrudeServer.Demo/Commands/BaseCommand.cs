using System;

using CrudeServer.HttpCommands;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Demo.Commands
{
    public abstract class BaseCommand : HttpCommand
    {
        protected BaseCommand(ICommandContext requestContext) : base(requestContext)
        {
        }

        protected void AddGenericItemData()
        {
            if (this.RequestContext.Items == null)
            {
                return;
            }

            this.RequestContext.Items.Add("Year", DateTime.UtcNow.Year);
        }
    }
}
