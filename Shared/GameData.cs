using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;


namespace Ozwego.Storage
{
    public class GameData
    {
        private Dictionary<string, PlayerGameStats> PlayerDictionary { get; set; }

        [XmlAttribute]
        public string GameHost { get; set; }

        [XmlElement("Players")]
        public List<PlayerTuple> Players { get; set; }

        [XmlElement("GameMoves")]
        public List<GameMoveDataPoint> GameMoves { get; private set; }

        [XmlAttribute]
        public string Winner { get; set; }

        [XmlAttribute]
        public int GameDuration { get; set; }

        [XmlAttribute]
        public DateTime GameStartTime { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameData()
        {
            PlayerDictionary = new Dictionary<string, PlayerGameStats>();
            GameMoves = new List<GameMoveDataPoint>();
            GameStartTime = DateTime.UtcNow;
        }


        /// <summary>
        /// Save a game move data point to the game data.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="timeOfMove"></param>
        /// <param name="moveType"></param>
        public void AddGameMoveDataPoint(string player, int timeOfMove, MoveType moveType)
        {
            var dataPoint = new GameMoveDataPoint(player, timeOfMove, moveType);

            GameMoves.Add(dataPoint);
        }


        /// <summary>
        /// Processes the data of this particular game by created player game stats for each player
        /// present in the game.
        /// </summary>
        public void ProcessGameData()
        {
            var activePlayers = new List<string>();
            var isFirstPeel = true;


            //
            // Populate the dictionary.
            //

            foreach (var playerTuple in Players)
            {
                PlayerDictionary.Add(playerTuple.Name, playerTuple.Stats);
            }

            GameDuration = GameMoves.Last().TimeOfMove;

            if (GameDuration <= 0)
            {
                throw new ArgumentOutOfRangeException(
                        string.Format("Game Duration must be positive. GameDuration = {0}",
                        GameDuration));
            }

            foreach (var dataPoint in GameMoves)
            {
                var curPlayer = dataPoint.Player;

                if (PlayerDictionary[curPlayer] == null)
                {
                    PlayerDictionary[curPlayer] = new PlayerGameStats();
                    activePlayers.Add(curPlayer);
                }

                PlayerDictionary[curPlayer].RawGameData.Add(dataPoint);

                if ((dataPoint.MoveType == MoveType.Peel) && isFirstPeel)
                {
                    isFirstPeel = false;
                    PlayerDictionary[curPlayer].PerformedFirstPeel = true;
                }
            }

            foreach (var activePlayer in activePlayers)
            {
                // Iterate through the player's game moves.
                foreach (var dataPoint in PlayerDictionary[activePlayer].RawGameData)
                {
                    switch (dataPoint.MoveType)
                    {
                        case MoveType.Dump:
                            PlayerDictionary[activePlayer].NumberOfDumps++;
                            break;

                        case MoveType.Peel:
                            PlayerDictionary[activePlayer].NumberOfPeels++;
                            break;

                        case MoveType.Victory:
                            PlayerDictionary[activePlayer].IsWinner = true;
                            break;
                    }
                }

                PlayerDictionary[activePlayer].AvgTimeBetweenDumps = 
                        PlayerDictionary[activePlayer].NumberOfDumps / GameDuration;

                PlayerDictionary[activePlayer].AvgTimeBetweenPeels = 
                        PlayerDictionary[activePlayer].NumberOfPeels / GameDuration;
            }


            //
            //Store back the dictionary data to the Player object.
            //ToDo: Figure out what to do with the data types here, and maybe remove the dictionary.
            //

            foreach (var playerTuple in Players)
            {
                playerTuple.Stats = PlayerDictionary[playerTuple.Name];

                //The code below solves the problem of when a player doesn't make any moves during the game (no peel or dumps).
                //ToDo: Remove this workaround code.  This is hacky.
                if (playerTuple.Stats == null)
                {
                    playerTuple.Stats = new PlayerGameStats();
                }
                
            }

        }
    }
}
