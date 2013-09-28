using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ozwego.Storage;

namespace Ozwego.Gameplay
{
    public class GameDataLogger
    {
        private GameData _gameData;


        /// <summary>
        /// Method called to initialize the Game Data Logger for a new game
        /// </summary>
        public void BeginLoggingSession(string GameHost, List<IPlayer> Players)
        {
            _gameData = new GameData();
            _gameData.GameHost = GameHost;

            var nameList = new List<string>();

            foreach (IPlayer player in Players)
            {
                nameList.Add(player.Alias);
            }

            _gameData.Players = nameList;
        }


        /// <summary>
        /// Log a game move to the logging session
        /// </summary>
        /// <param name="player"></param>
        /// <param name="timeOfMove"></param>
        /// <param name="moveType"></param>
        public void LogMove(string player, int timeOfMove, MoveType moveType)
        {
            if (_gameData == null)
            {
                throw new InvalidOperationException("The logging session has not begun. Unable to log move.");
            }

            _gameData.AddGameMoveDataPoint(player, timeOfMove, moveType);
        }


        /// <summary>
        /// Method called to complete the logging session.  It saves the current game data to disk.
        /// </summary>
        public async void EndLoggingSession()
        {
            var gdh = GameDataHistory.GetInstance();
            await gdh.StoreGameData(_gameData);
            _gameData = null;
        }
    }
}
