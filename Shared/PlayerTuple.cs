using System;
using System.IO;
using System.Xml.Serialization;
using Ozwego.Storage;
using Shared.Serialization;

namespace Shared
{
    public class PlayerTuple : IBinarySerializable
    {
        public string Name { get; set; }

        public PlayerGameStats Stats { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Stats);
        }

        public void Read(BinaryReader reader)
        {
            Name = reader.ReadString();
            Stats.Read(reader);
        }
    }
}
