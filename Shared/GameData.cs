using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shared.Serialization;


namespace Ozwego.Storage
{
    public class GameData : IBinarySerializable
    {
        public string GameHost { get; set; }

        public List<GameMoveDataPoint> GameMoves { get; private set; }

        public string Winner { get; set; }

        public int GameDuration { get; set; }

        public DateTime GameStartTime { get; set; }

        // <Player's Alias, PlayerGameStats>.  It has to be alias because bots have no email address.
        public Dictionary<string, PlayerGameStats> PlayerDictionary { get; set; }


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
                            Winner = activePlayer;
                            break;
                    }
                }

                PlayerDictionary[activePlayer].AvgTimeBetweenDumps = 
                        PlayerDictionary[activePlayer].NumberOfDumps / GameDuration;

                PlayerDictionary[activePlayer].AvgTimeBetweenPeels = 
                        PlayerDictionary[activePlayer].NumberOfPeels / GameDuration;
            }


            //
            // Create a blank PlayerGameStats object for all inactive players
            //

            var inactivePlayerKeys = PlayerDictionary
                    .Where(kvp => kvp.Value == null)
                    .Select(kvp => kvp.Key).ToList();

            foreach (var key in inactivePlayerKeys)
            {
                PlayerDictionary[key] = new PlayerGameStats();
            }

        }


        public void Write(BinaryWriter writer)
        {
            writer.Write(GameHost);
            writer.WriteList(GameMoves);
            writer.Write(Winner);
            writer.Write(GameDuration);
            writer.Write(GameStartTime);
            writer.WriteDictionary(PlayerDictionary);
        }


        public void Read(BinaryReader reader)
        {
            GameHost = reader.ReadString();
            GameMoves = reader.ReadList<GameMoveDataPoint>();
            Winner = reader.ReadString();
            GameDuration = reader.ReadInt32();
            GameStartTime = reader.ReadDateTime();
            PlayerDictionary = reader.ReadPlayerDictionary<PlayerGameStats>();
        }
    }
}
