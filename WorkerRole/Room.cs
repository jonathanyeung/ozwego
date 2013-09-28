using Ozwego.Shared;
using System;
using System.Collections.Generic;

namespace WorkerRole
{
    /// <summary>
    /// Represents a game room.
    /// </summary>
    public class Room
    {
        public Client Host;
        public List<Client> Members;

        internal Room(Client host)
        {
            Host = host;
            Members = new List<Client> {Host};
        }

        public void ChangeHost(Client host)
        {
            Host = host;
        }

        public void ChangeToRandomNewHost()
        {
            var random = new Random();

            if (Members.Count > 0)
            {
                int index = random.Next(0, Members.Count - 1);
                Host = Members[index];
            }

            var messageSender = MessageSender.GetMessageSender();
            messageSender.BroadcastMessage(
                Members,
                PacketType.HostTransfer, 
                Host.UserName);
        }
    }
}
