using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole
{
    /// <summary>
    /// Represents another instance of the Ozwego Worker Role, aka another server instance
    /// connecting through the internal endpoint.
    /// </summary>
    public class InternalClient : Client
    {
        public Guid ServerInstanceID;

        public InternalClient(Socket client)
            : base(client)
        {
            ServerInstanceID = WorkerRole.instanceID;
        }

        protected override void HandleMessage(ref Client client, byte[] msgBytes)
        {
            WorkerRole.IncomingMessageHandler.HandleMessage(ref client, EndpointType.Internal, msgBytes);
        }

        public override void Disconnect()
        {
            base.Disconnect();
            WorkerRole.InternalClients.Remove(this);
        }
    }
}
