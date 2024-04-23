using System;

using CrudeServer.HttpCommands;

namespace CrudeServer.Demo.Commands
{
    public abstract class BaseCommand : HttpCommand
    {
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
