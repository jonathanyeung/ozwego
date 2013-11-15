using System.IO;
using Shared.Serialization;

namespace Shared
{
    public class ChatMessage : IBinarySerializable
    {
        public string Sender;
        public string Message;

        public void Write(BinaryWriter writer)
        {
            writer.Write(Sender);
            writer.Write(Message);
        }

        public void Read(BinaryReader reader)
        {
            Sender = reader.ReadString();
            Message = reader.ReadString();
        }
    }
}
