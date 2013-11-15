using System;
using System.Diagnostics;
using Shared.Serialization;
using System.Collections.Generic;
using System.IO;

namespace Shared
{
    public class PacketV1 : IBinarySerializable
    {
        public PacketType PacketType { get; set; }

        public string Sender { get; set; }

        public List<string> Recipients;

        public object Data { get; set; }

        public PacketV1()
        {
            Recipients = new List<string>();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)PacketType);
            writer.Write(Sender);
            writer.WriteList(Recipients);

            var type = DataPacket.PacketTypeMap[PacketType];

            if (type == typeof (string))
            {
                var stringData = Data as string;
                if (null != stringData)
                {
                    writer.Write(stringData);
                }
            }
            else
            {
                var binaryData = Data as IBinarySerializable;
                if (null != binaryData)
                {
                    writer.Write(binaryData);
                }
            }
        }


        public void Read(BinaryReader reader)
        {
            PacketType = (PacketType)reader.ReadByte();
            Sender = reader.ReadString();
            Recipients = reader.ReadList();

            var type = DataPacket.PacketTypeMap[PacketType];

            if (type == null)
            {
                Data = null;
            }
            else if (type == typeof(string))
            {
                Data = reader.ReadString();
            }
            else
            {
                var binaryData = Activator.CreateInstance(type);

                var hasInstance = reader.ReadBoolean();

                if (!hasInstance)
                {
                    return;
                }

                try
                {
                    ((IBinarySerializable) binaryData).Read(reader);

                    Data = binaryData;
                }
                catch
                {
#if DEBUG
                    throw;
#endif
                    Debug.WriteLine(string.Format("Deserialization failed on PacketType {0}", PacketType.ToString()));
                }


            }
        }
    }
}
