using System.IO;
using Shared.Serialization;

namespace Ozwego.Storage
{
    /// <summary>
    /// Represents the types of moves that a player can perform during the game.
    /// </summary>
    public enum MoveType : byte
    {
        Dump,
        Peel,
        Victory
    }


    /// <summary>
    /// Represents a data point of a move performed by a player during a game.
    /// </summary>
    public class GameMoveDataPoint : IBinarySerializable
    {
        // The Name of the player making the move.
        public string Player { get; set; }


        // Time of Move in Seconds.
        public int TimeOfMove { get; set; }


        // The Type of Move
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

        public void Write(BinaryWriter writer)
        {
            writer.Write(Player);
            writer.Write(TimeOfMove);
            writer.Write((byte)MoveType);
        }

        public void Read(BinaryReader reader)
        {
            Player = reader.ReadString();
            TimeOfMove = reader.ReadInt32();
            MoveType = (MoveType) reader.ReadByte();
        }
    }
}
