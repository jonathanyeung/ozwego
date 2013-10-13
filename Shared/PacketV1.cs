using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Shared
{
    public class PacketV1
    {
        [XmlAttribute]
        public PacketType PacketType { get; set; }

        [XmlAttribute]
        public string Sender { get; set; }

        [XmlElement("Recipients")] public List<string> Recipients;

        [XmlElement]
        public object Data { get; set; }
    }
}
