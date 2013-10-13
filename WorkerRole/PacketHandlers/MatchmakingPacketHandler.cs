
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Shared;
using WorkerRole.Matchmaking;

namespace WorkerRole.PacketHandlers
{
    public class MatchmakingPacketHandler : PacketHandler
    {
        public MatchmakingPacketHandler(PacketType packetType, string sender, List<string> recipients, object data)
            : base(packetType, sender, recipients, data)
        {
        }

        public override void DoActions(ref Client client)
        {
            switch (PacketType)
            {
                case PacketType.ClientStartingMatchmaking:
                    var matchmaker = Matchmaker.GetInstance();
                    matchmaker.JoinMatchmakingQueue(client);
                    break;

                case PacketType.ClientStoppingMatchmaking:
                    throw new NotImplementedException();
                    break;

                default:
                    Trace.WriteLine(string.Format("[MatchmakingPacketHandler.DoActions] - " +
                        "Invalid packet type for this PacketHandler = {0}", PacketType.ToString()));
                    break;
            }
        }
    }
}
