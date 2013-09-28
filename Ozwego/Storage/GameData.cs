using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Storage
{
    [DataContractAttribute]
    public class GameData
    {
        [DataMember]
        public string GameHost { get; set; }

        [DataMember]
        public List<string> Players { get; set; }

        [DataMember]
        public List<GameMoveDataPoint> GameMoves { get; private set; }

        [DataMember]
        public string Winner { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameData()
        {
            Players = new List<string>();
            GameMoves = new List<GameMoveDataPoint>();
        }

        /// <summary>
        /// Save a game move data point to the game data.
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="TimeOfMove"></param>
        /// <param name="MoveType"></param>
        public void AddGameMoveDataPoint(string Player, int TimeOfMove, MoveType MoveType)
        {
            var dataPoint = new GameMoveDataPoint(Player, TimeOfMove, MoveType);

            GameMoves.Add(dataPoint);
        }
    }
}
