using System.Xml.Serialization;

namespace Ozwego.Storage
{
    /// <summary>
    /// Represents the types of moves that a player can perform during the game.
    /// </summary>
    public enum MoveType
    {
        Dump,
        Peel,
        Victory
    }


    /// <summary>
    /// Represents a data point of a move performed by a player during a game.
    /// </summary>
    public class GameMoveDataPoint
    {
        // The Name of the player making the move.
        [XmlAttribute]
        public string Player { get; set; }


        // Time of Move in Seconds.
        [XmlAttribute]
        public int TimeOfMove { get; set; }


        // The Type of Move
        [XmlAttribute]
        public MoveType MoveType { get; set; }


        public GameMoveDataPoint(string player, int timeOfMove, MoveType moveType)
        {
            Player = player;
            TimeOfMove = timeOfMove;
            MoveType = moveType;
        }

        public GameMoveDataPoint()
        {
        }
    }
}
