using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Server
{
    /// <summary>
    /// Represents a message to be sent to the server's message table
    /// </summary>

    public abstract class Message
    {
        public PacketType PacketType { get; set; }

        public string MessageString { get; set; }

        public string SenderEmailAddress { get; set; }
    }
}
