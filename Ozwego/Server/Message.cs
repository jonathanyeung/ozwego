
using Shared;

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
