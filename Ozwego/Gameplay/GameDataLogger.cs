using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ozwego.Storage;
using Ozwego.BuddyManagement;

namespace Ozwego.Gameplay
{
    public class GameDataLogger
    {
        private GameData _gameData;


        /// <summary>
        /// Method called to initialize the Game Data Logger for a new game
        /// </summary>
        public void BeginLoggingSession(string gameHost, IEnumerable<IPlayer> players)
        {
            _gameData = new GameData {GameHost = gameHost};

            foreach (var p in players)
            {
                _gameData.PlayerDictionary.Add(p.Alias, null);
            }
        }

        //ToDo: Refactor IPlayer and get rid of this overload.
        /// <summary>
        /// Method called to initialize the Game Data Logger for a new game
        /// </summary>
        public void BeginLoggingSession(string gameHost, IEnumerable<Friend> players)
        {
            _gameData = new GameData { GameHost = gameHost };

            foreach (var p in players)
            {
                _gameData.PlayerDictionary.Add(p.Alias, null);
            }
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
        public async Task<GameData> EndLoggingSession()
        {
            _gameData.ProcessGameData();

            var gdh = GameDataHistory.GetInstance();

            await gdh.StoreGameData(_gameData);

            var tempData = _gameData;

            _gameData = null;

            return tempData;
        }
    }
}
