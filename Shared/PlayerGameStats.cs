using System.Collections.Generic;
using System.IO;
using Shared.Serialization;

namespace Ozwego.Storage
{
    /// <summary>
    /// Represents the stats for a single game by a single player
    /// </summary>
    public class PlayerGameStats : IBinarySerializable
    {
        public int NumberOfPeels;

        public bool PerformedFirstPeel;

        public int AvgTimeBetweenPeels;

        public int NumberOfDumps;

        public int AvgTimeBetweenDumps;

        public bool IsWinner;

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

        public void Write(BinaryWriter writer)
        {
            writer.Write(NumberOfPeels);
            writer.Write(PerformedFirstPeel);
            writer.Write(AvgTimeBetweenPeels);
            writer.Write(NumberOfDumps);
            writer.Write(AvgTimeBetweenDumps);
            writer.Write(IsWinner);
            writer.WriteList(RawGameData);
        }

        public void Read(BinaryReader reader)
        {
            NumberOfPeels = reader.ReadInt32();
            PerformedFirstPeel = reader.ReadBoolean();
            AvgTimeBetweenPeels = reader.ReadInt32();
            NumberOfDumps = reader.ReadInt32();
            AvgTimeBetweenDumps = reader.ReadInt32();
            IsWinner = reader.ReadBoolean();
            RawGameData = reader.ReadList<GameMoveDataPoint>();
        }
    }
}
