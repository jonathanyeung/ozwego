using System.Xml.Serialization;

namespace Ozwego.Storage
{
    public class PlayerTuple
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("Stats")]
        public PlayerGameStats Stats { get; set; }
    }
}
