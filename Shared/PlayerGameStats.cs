using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ozwego.Storage
{
    /// <summary>
    /// Represents the stats for a single game by a single player
    /// </summary>
    public class PlayerGameStats
    {
        [XmlAttribute]
        public int NumberOfPeels;

        [XmlAttribute]
        public bool PerformedFirstPeel;

        [XmlAttribute]
        public int AvgTimeBetweenPeels;

        [XmlAttribute]
        public int NumberOfDumps;

        [XmlAttribute]
        public int AvgTimeBetweenDumps;

        [XmlAttribute]
        public bool IsWinner;

        [XmlElement("RawGameData")]
        public List<GameMoveDataPoint> RawGameData;

        public PlayerGameStats()
        {
            NumberOfPeels = 0;
            PerformedFirstPeel = false;
            AvgTimeBetweenPeels = 0;
            NumberOfDumps = 0;
            AvgTimeBetweenDumps = 0;
            IsWinner = false;
            RawGameData = new List<GameMoveDataPoint>();
        }
    }
}
