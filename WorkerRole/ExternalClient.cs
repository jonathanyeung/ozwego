using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole
{
    /// <summary>
    /// Represents an end-user client, aka someone who is playing the game and connecting to the
    /// Ozwego server through an external endpoint.
    /// </summary>
    public class ExternalClient : Client
    {
        public ClientInformation Information;

        public ExternalClient(Socket client) 
            : base(client)
        { }

        protected override void HandleMessage(ref Client client, byte[] msgBytes)
        {
            WorkerRole.IncomingMessageHandler.HandleMessage(ref client, EndpointType.External, msgBytes);
        }

        public override void Disconnect()
        {
            base.Disconnect();
            WorkerRole.ClientManager.RemoveClient(this);
        }
    }
}
