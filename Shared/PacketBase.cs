using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Shared
{
    public enum PacketVersion : byte
    {
        Version1 = 1,
        MaxValue = 2
    }

    public class PacketBase
    {
        [XmlAttribute]
        public PacketVersion PacketVersion { get; set; }

        [XmlElement]
        public object Data { get; set; }
    }
}
