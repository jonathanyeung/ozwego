using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ozwego.BuddyManagement;
using Windows.UI.Xaml;

namespace Ozwego.Gameplay.Bots
{
    public class TimerBot : IRobot, IPlayer
    {
        private HandTileSet _tileSet;


        public TimerBot(int botLevel)
        {
            _level = botLevel;
            Alias = "Level " + _level.ToString() + " Robot";

            _botTimer = new DispatcherTimer();

            _tileSet = new HandTileSet();
        }


        private string _alias;
        public string Alias
        {
            get
            {
                return _alias;
            }
            set
            {
                _alias = value;
            }
        }


        private int _level;
        public int Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
            }
        }


        /// <summary>
        /// Method that tells the bot to begin firing peel and dump actions.
        /// </summary>
        public void StartBot()
        {
            _elapsedTicks = 0;
            _botTimer.Tick += OnTimerTick;
            _botTimer.Interval = new TimeSpan(0, 0, 3);
            _botTimer.Start();
        }


        /// <summary>
        /// Method that tells the bot to stop all of its actions.
        /// </summary>
        public void StopBot()
        {
            _botTimer.Stop();
            _botTimer.Tick -= OnTimerTick;
        }


        public void InitializeForGame()
        {
            _tileSet.CreateNewStartingHand();
        }


        public async void PerformDumpAction()
        {
            if ( 0 < _tileSet.GetCurrentHandSize())
            {
                var tile = _tileSet.removeRandomTileFromHand();

                if (null != tile)
                {
                    var gameController = GameController.GetInstance();
                    await gameController.PerformDumpAction(_alias, tile);
                }
            }
        }


        public void PerformPeelAction()
        {
            //
            // Tell the game controller that a peel has occurred.  The returned tile will come back
            // via the PeelActionReceived callback, just like everyone else.
            //

            var gameController = GameController.GetInstance();
            gameController.PeelActionReceived(_alias);
        }

        public void PeelActionReceived(Tile returnedTile)
        {
            _tileSet.returnTileToHand(returnedTile);
        }

        //ToDo: Probably can remove this method, I think the game logic handles this automatically on peel with no tiles left.
        public void SignalVictory()
        {
            var gameController = GameController.GetInstance();
            gameController.EndGame(Alias);
        }

        // Represents the amount of time it takes for the bot to get to the first peel point
        private int TicksToFirstPeel = 60;

        private int _elapsedTicks = 0;

        private DispatcherTimer _botTimer;


        void OnTimerTick(object sender, object e)
        {
            _elapsedTicks++;

            if (_elapsedTicks > 1)
            {
                if (_elapsedTicks % 3 == 0)
                {
                    OnTilePlayed();
                }

                if (_elapsedTicks % 5 == 0)
                {
                    PerformDumpAction();
                }
            }
        }


        public void OnTilePlayed()
        {
            _tileSet.removeRandomTileFromHand();
            if (0 == _tileSet.GetCurrentHandSize())
            {
                PerformPeelAction();
            }
        }


        public int GetCurrentHandSize()
        {
            return _tileSet.GetCurrentHandSize();
        }
    }
}
