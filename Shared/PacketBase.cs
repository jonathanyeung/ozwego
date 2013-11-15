using System;
using Shared.Serialization;
using System.IO;

namespace Shared
{
    public enum PacketVersion : byte
    {
        Version1 = 1,
        MaxValue = 2
    }

    public class PacketBase : IBinarySerializable
    {
        public PacketVersion PacketVersion { get; set; }

        public IBinarySerializable Data { get; set; }


        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)PacketVersion);
            writer.Write(Data);
        }


        public void Read(BinaryReader reader)
        {
            var hasInstance = reader.ReadBoolean();

            if (!hasInstance)
            {
                return;
            }

            PacketVersion = (PacketVersion)reader.ReadByte();

            switch (PacketVersion)
            {
                case PacketVersion.Version1:
                    Data = new PacketV1();
                    break;

                default:
#if DEBUG
                    throw new ArgumentException(string.Format("Invalid Packet Version : {0}", PacketVersion.ToString()));
#else
                    return;
#endif
            }

            Data.Read(reader);
        }
    }
}
