using Shared;

namespace Ozwego.Server.MessageProcessors
{
    /// <summary>
    /// Strategy class for processing an incoming message.  The incoming message class delegates
    /// responsibility of the message processing to these classes.
    /// </summary>
    public abstract class MessageProcessor
    {
        public abstract void DoActions(PacketType packetType, object data, string senderEmailAddress);
    }
}
