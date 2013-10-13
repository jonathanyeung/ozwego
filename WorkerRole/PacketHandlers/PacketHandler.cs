using System.Collections.Generic;

using Shared;

namespace WorkerRole.PacketHandlers
{
    public abstract class PacketHandler
    {
        protected readonly PacketType PacketType;
        protected readonly string Sender;
        protected List<string> Recipients;
        protected readonly object Data;

        public abstract void DoActions(
            ref Client client);

        protected PacketHandler(PacketType packetType, string sender, List<string> recipients, object data)
        {
            PacketType = packetType;
            Sender = sender;
            Recipients = recipients;
            Data = data;
        }
    }
}
