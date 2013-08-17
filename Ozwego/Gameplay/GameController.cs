using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ozwego.Server;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace Ozwego.Gameplay
{
    public class GameController
    {
        private readonly Dictionary _dictionary;
        private readonly GameBoard _gameBoard;
        private readonly TileManager _tileManager;

        public bool GameStarted;


        private DispatcherTimer _gameClock;


        /// <summary>
        /// Singleton, private constructor.
        /// </summary>
        private static GameController _instance;

        private GameController()
        {
            _dictionary = Dictionary.GetInstance();
            _gameBoard = GameBoard.GetInstance();
            _tileManager = TileManager.GetInstance();
        }


        /// <summary>
        /// Public method to instantiate GameController singleton.
        /// </summary>
        /// <returns></returns>
        public static GameController GetInstance()
        {
            return _instance ?? (_instance = new GameController());
        }


        public async Task<bool> Initialize()
        {
            InitializeGameClock();
            await _dictionary.PopulateDictionary();
            _gameBoard.ClearBoard();
            _tileManager.InitializeForNewGame();
            return true;
        }


        /// <summary>
        /// To be called when navigating away from the game page
        /// </summary>
        public void Cleanup()
        {
            GameStarted = false;
            _gameClock.Tick -= GameClockOnTick;
            _gameClock.Stop();
        }


        #region Events

        public delegate void PeelEventHandler(object sender, string args);
        public event PeelEventHandler PeelEvent;

        private void OnPeelOccurred(string args)
        {
            PeelEventHandler handler = PeelEvent;

            if (handler != null)
            {
                handler(this, args);
            }
        }


        public delegate void InvalidWordUiUpdateHandler(object sender, List<Point> invalidWordCoordinates);
        public event InvalidWordUiUpdateHandler InvalidWordUiUpdateEvent;

        private void OnInvalidWordUiUpdate(List<Point> args)
        {
            InvalidWordUiUpdateHandler handler = InvalidWordUiUpdateEvent;

            if (handler != null)
            {
                handler(this, args);
            }
        }


        public delegate void GameStartedHandler(object sender);
        public event GameStartedHandler GameStartedEvent;

        private void OnGameStarted()
        {
            GameStartedHandler handler = GameStartedEvent;

            if (handler != null)
            {
                handler(this);
            }
        }

        #endregion


        /// <summary>
        /// These interface primarily with the tile manager
        /// </summary>
        #region Tile Operations

        public List<Tile> GetTileRack()
        {
            return _tileManager.GetCurrentHand();
        }


        public void ReturnTileToRack(string tileContents)
        {
            var newTile = new Tile(tileContents);
            _tileManager.returnTileToHand(newTile);
        }


        public void RemoveTileFromRack(string tileContents)
        {
            var newTile = new Tile(tileContents);
            _tileManager.removeTileFromHand(newTile);
        }

        #endregion


        #region GameBoard Interface

        public void SetBoardSpace(string contents, int i, int j)
        {
            var newTile = new Tile(contents);
            _gameBoard.SetBoardSpace(newTile, i, j);
        }


        public bool IsBoardSpaceOccupied(int i, int j)
        {
            return _gameBoard.IsBoardSpaceOccupied(i, j);
        }


        public void ClearBoardSpace(int i, int j)
        {
            _gameBoard.ClearBoardSpace(i, j);
        }


        private bool AreWordsValid()
        {
            bool isValid = true;

            var wordList = _gameBoard.GetWordList();
            var invalidWords = new List<GameBoard.WordCoordinate>();


            //
            // 0 here means that the board was invalid (tiles not properly placed).
            //

            if (wordList.Count == 0)
            {
                return false;
            }


            //
            // Go through all the words and look them up in the dictionary.
            //

            foreach (var wc in wordList)
            {
                if (!_dictionary.IsAValidWord(wc._string))
                {
                    isValid = false;
                    invalidWords.Add(wc);
                }
            }


            //
            // If the board isn't valid, signal to the UI page to highlight the invalid tiles
            //

            if (!isValid)
            {
                var invalidPoints = new List<Point>();

                foreach (var wc in invalidWords)
                {
                    //
                    // Begin and End of WordCoordinates give us the two endpoints of an invalid 
                    // word.  All tiles in between those must be marked invalid as well.
                    //

                    if (wc._begin.X == wc._end.X)
                    {
                        var variable = wc._begin.Y;
                        while (variable <= wc._end.Y)
                        {
                            var newPoint = new Point(wc._begin.X, variable);
                            invalidPoints.Add(newPoint);
                            variable++;
                        }
                    }
                    else if (wc._begin.Y == wc._end.Y)
                    {
                        var variable = wc._begin.X;
                        while (variable <= wc._end.X)
                        {
                            var newPoint = new Point(variable, wc._begin.Y);
                            invalidPoints.Add(newPoint);
                            variable++;
                        }
                    }
                }
                OnInvalidWordUiUpdate(invalidPoints);
            }

            return isValid;
        }


        #endregion


        #region GameClock

        private void InitializeGameClock()
        {
            _gameClock = new DispatcherTimer();
            _gameClock.Tick += GameClockOnTick;
            _gameClock.Interval = new TimeSpan(0, 0, 1);

            App.GameBoardViewModel.GameTime = 0;
        }

        private void GameClockOnTick(object sender, object o)
        {
            App.GameBoardViewModel.GameTime++;
        }

        #endregion


        /// <summary>
        /// This method is to be called after a tile has been played. It checks to see if all of 
        /// the tiles have been played, and if so, whether the game board is valid.  If all of
        /// these conditions are met, then a peel action occurs.
        /// </summary>
        public void OnTilePlayed()
        {
            if (0 == _tileManager.GetCurrentHandSize())
            {
                if (AreWordsValid())
                {
                    InitiatePeelAction();
                }
            }
        }


        public async void StartGame()
        {
            if (GameStarted) return;

            GameStarted = true;

            if (App.ServerProxy.messageSender != null)
            {
                await App.ServerProxy.messageSender.SendMessage(PacketType.StartGame);
            }

            OnGameStarted();

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    App.GameBoardViewModel.GameTime = 0;
                    App.GameBoardViewModel.TilePileCount = _tileManager.GetPileCount();
                    _gameClock.Start();
                });
        }


        public async void EndGame(string winnerName)
        {
            GameStarted = false;

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                _gameClock.Stop();

                string title = string.Format("We Have a Winner!");
                var dialog = new MessageDialog(winnerName, title);
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();
            });
        }


        public async Task<List<Tile>> InitiateDumpAction(Tile returnedTile)
        {
            App.GameBoardViewModel.TilePileCount -= 2;

            if (App.ServerProxy.messageSender != null)
            {
                await App.ServerProxy.messageSender.SendMessage(PacketType.ClientDump);
            }

            return _tileManager.PerformDumpAction(returnedTile);
        }


        private async void InitiatePeelAction()
        {
            //
            // If there are enough tiles left, then do a peel.  Otherwise, if there are not enough
            // tiles, then this client has won the game.
            //

            if (_tileManager.GetPileCount() >= App.MainPageViewModel.RoomMembers.Count)
            {
                Tile tile = _tileManager.PerformPeelAction(App.MainPageViewModel.RoomMembers.Count);
                OnPeelOccurred(tile.TileContents);

                App.GameBoardViewModel.TilePileCount -= App.MainPageViewModel.RoomMembers.Count;

                if (App.ServerProxy.messageSender != null)
                {
                    await App.ServerProxy.messageSender.SendMessage(PacketType.ClientPeel);
                }
            }
            else
            {
                if (App.ServerProxy.messageSender != null)
                {
                    await App.ServerProxy.messageSender.SendMessage(PacketType.ClientVictory);
                }
                else
                {
                    EndGame(App.ClientBuddyInstance.MicrosoftAccountAddress);
                }
            }
        }

        /// <summary>
        /// Contains methods that are called when server messages are received.
        /// </summary>
        #region MessageReceivedHandlers

        public async void PeelActionReceived()
        {
            Tile tile = _tileManager.PerformPeelAction(App.MainPageViewModel.RoomMembers.Count);

            OnPeelOccurred(tile.TileContents);

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                App.GameBoardViewModel.TilePileCount -= App.MainPageViewModel.RoomMembers.Count;
            });
        }

        public async void DumpActionReceived()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                App.GameBoardViewModel.TilePileCount -= 2;

                // ToDo: Get these two Tiles from the server.
                _tileManager.DumpReceived(new Tile("a"), new Tile("b"));
            });
        }

        #endregion // MessageReceivedHandlers

    }
}
