using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

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
    [DataContractAttribute]
    public class GameMoveDataPoint
    {
        // The Name of the player making the move.
        [DataMember]
        public string player { get; private set; }


        // Time of Move in Seconds.
        [DataMember]
        public int timeOfMove { get; private set; }


        // The Type of Move
        [DataMember]
        public MoveType moveType { get; private set; }


        public GameMoveDataPoint(string Player, int TimeOfMove, MoveType MoveType)
        {
            player = Player;
            timeOfMove = TimeOfMove;
            moveType = MoveType;
        }
    }
}
