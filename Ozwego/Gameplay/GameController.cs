using Ozwego.BuddyManagement;
using Ozwego.Gameplay.Bots;
using Ozwego.Server;

using Ozwego.UI;
using Ozwego.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Ozwego.Gameplay
{
    public class GameController
    {
        private Dictionary _dictionary;
        private GameBoard _gameBoard;
        private TileManager _tileManager;
        private BotManager _botManager;
        private DispatcherTimer _gameClock;
        private GameDataLogger _gameDataLogger;

        private GameConnectionType _gameConnectionType;
        private GameMode _gameMode;

        private HumanPlayer _humanPlayer;
        public HumanPlayer HumanPlayer
        {
            get
            {
                return _humanPlayer;
            }
        }

        private readonly List<IPlayer> _localPlayers;
        public List<IPlayer> LocalPlayers
        {
            get
            {
                return _localPlayers;
            }
        }

        public bool GameStarted;


        private static GameController _instance;

        private GameController()
        {
            _localPlayers = new List<IPlayer>();
            _gameDataLogger = new GameDataLogger();
        }


        /// <summary>
        /// Public method to instantiate GameController singleton.
        /// </summary>
        /// <returns></returns>
        public static GameController GetInstance()
        {
            return _instance ?? (_instance = new GameController());
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

        public delegate void PeelEventHandler(object sender, string tileContents, string senderName);
        public event PeelEventHandler PeelEvent;
        
        private void OnPeelOccurred(string tileContents, string senderName)
        {
            PeelEventHandler handler = PeelEvent;

            if (handler != null)
            {
                handler(this, tileContents, senderName);
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


        public delegate void InitializeCompletedHandler(object sender);
        public event InitializeCompletedHandler InitializeCompleteEvent;

        private void OnInitializeComplete()
        {
            InitializeCompletedHandler handler = InitializeCompleteEvent;

            if (handler != null)
            {
                handler(this);
            }
        }

        #endregion


        #region GameClock

        private void InitializeGameClock()
        {
            _gameClock = new DispatcherTimer();
            _gameClock.Tick += GameClockOnTick;
            _gameClock.Interval = new TimeSpan(0, 0, 1);

            var viewModel = GameBoardViewModel.GetInstance();
            viewModel.GameTime = 0;
        }

        private void GameClockOnTick(object sender, object o)
        {
            var viewModel = GameBoardViewModel.GetInstance();
            viewModel.GameTime++;
        }

        #endregion


        public async Task Initialize(GameBoardNavigationArgs args)
        {
            _gameConnectionType = args.GameConnectionType;
            _gameMode = args.GameMode;

            _tileManager = TileManager.GetInstance();
            await _tileManager.InitializeForNewGame();

            _botManager = BotManager.GetInstance();

            _gameBoard = GameBoard.GetInstance();
            _gameBoard.ClearBoard();

            _dictionary = Dictionary.GetInstance();
            await _dictionary.PopulateDictionary();

            InitializeGameClock();

            _humanPlayer = new HumanPlayer();

            _localPlayers.Clear();
            _localPlayers.Add(_humanPlayer);

            _botManager.ClearBotList();

            for (int i = 0; i < args.BotCount; i++)
            {
                _botManager.CreateBot(5);
            }

            foreach (IRobot bot in _botManager.BotList)
            {
                _localPlayers.Add(bot);
            }

            foreach (IPlayer player in _localPlayers)
            {
                player.InitializeForGame();
            }

            OnInitializeComplete();
        }


        /// <summary>
        /// Method starts the game.  Updates UI with game start animations and kicks off the bots.
        /// </summary>
        public async void StartGame()
        {
            if (GameStarted) return;
            GameStarted = true;

            var serverProxy = ServerProxy.GetInstance();

            if (serverProxy.messageSender != null)
            {
                await serverProxy.messageSender.SendMessage(PacketType.ClientStartGame);
            }

            _gameDataLogger.BeginLoggingSession(_humanPlayer.Alias, _localPlayers);

            OnGameStarted();

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var viewModel = GameBoardViewModel.GetInstance();

                viewModel.GameTime = 0;
                viewModel.TilePileCount = _tileManager.GetPileCount();
                _gameClock.Start();
            });

            // ToDo: Add a blocking method here that waits for the startgameanimation to complete.

            _botManager.StartBots();
        }


        /// <summary>
        /// These interface primarily between the Game Board UI and the tile manager component of 
        /// the GameController.
        /// </summary>
        #region Tile Operations

        public List<Tile> GetTileRack()
        {
            return _humanPlayer.GetTileRack();
        }


        public void ReturnTileToRack(string tileContents)
        {
            _humanPlayer.ReturnTileToRack(tileContents);
        }


        public void RemoveTileFromRack(string tileContents)
        {
            _humanPlayer.RemoveTileFromRack(tileContents);
        }


        /// <summary>
        /// This method is to be called after a tile has been played. It checks to see if all of 
        /// the tiles have been played, and if so, whether the game board is valid.  If all of
        /// these conditions are met, then a peel action occurs.
        /// </summary>
        public void OnTilePlayedbyHumanPlayer()
        {
            _humanPlayer.OnTilePlayed();
        }

        #endregion


        /// <summary>
        /// Methods that interface between the GameBoard UI and the game board data class of the
        /// GameController.
        /// </summary>
        #region Game Board UI Interface

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


        public bool AreWordsValid()
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


        /// <summary>
        /// Contains methods that are called when game action messages are received.
        /// </summary>
        #region Game Action Message Handlers


        /// <summary>
        /// This method is invoked whenever someone (either local or from the server) performs a
        /// peel action.
        /// </summary>
        /// <param name="actionSender">The person who performed the dump action</param>
        public async void PeelActionReceived(string actionSender)
        {
            var serverProxy = ServerProxy.GetInstance();
            var roomManager = RoomManager.GetInstance();
            var viewModel = GameBoardViewModel.GetInstance();


            //
            // If there are enough tiles left, then do a peel.  Otherwise, if there are not enough
            // tiles, then actionSender has won the game.
            //

            if (_tileManager.GetPileCount() >= (roomManager.RoomMembers.Count + _localPlayers.Count - 1))
            {

                var tiles = _tileManager.PerformPeelAction(roomManager.RoomMembers.Count + _localPlayers.Count - 1);

                if (null == tiles)
                {
                    return;
                }


                //
                // Update the hands of all of the players with one of the returned tiles from the peel.
                // If it's the human player's hand, update the UI too with the returned tile.
                //

                foreach (IPlayer player in _localPlayers)
                {
                    if (player.Alias == _humanPlayer.Alias)
                    {
                        OnPeelOccurred(tiles[0].TileContents, actionSender);
                    }

                    // ToDo: This line can possibly throw an out of bounds exception.
                    player.PeelActionReceived(tiles[0]);
                    tiles.RemoveAt(0);
                }


                //
                // Update UI.
                //

                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //
                    // Subtract total online players + bots.  Subtract one b/c the human player is 
                    // double counted.
                    //

                    viewModel.TilePileCount -=
                            (roomManager.RoomMembers.Count + _localPlayers.Count - 1);
                    
                });

                if (serverProxy.messageSender != null)
                {
                    // ToDo: This will send the incorrect message to the server if a bot has peeled and not the client.
                    await serverProxy.messageSender.SendMessage(PacketType.ClientPeel);
                }


                //
                // Log the Peel event
                //

                _gameDataLogger.LogMove(actionSender, viewModel.GameTime, Storage.MoveType.Peel);
            }
            else
            {
                if (serverProxy.messageSender != null)
                {
                    await serverProxy.messageSender.SendMessage(PacketType.ClientVictory);
                }
                else
                {
                    EndGame(actionSender);
                }
            }
        }


        /// <summary>
        /// This method is invoked whenever someone from the server performs a 
        /// dump action.
        /// </summary>
        /// <param name="actionSender">The person who performed the dump action</param>
        public async void DumpActionReceivedFromServer(string actionSender)
        {
            var viewModel = GameBoardViewModel.GetInstance();

            _gameDataLogger.LogMove(actionSender, viewModel.GameTime, Storage.MoveType.Dump);

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                viewModel.TilePileCount -= 2;

                // ToDo: Get these two Tiles from the server.
                _tileManager.DumpReceived(new Tile("a"), new Tile("b"));
            });
        }


        /// <summary>
        /// This method is invoked whenever someone from the server performs a 
        /// dump action.
        /// </summary>
        /// <param name="actionSender">The person who performed the dump action</param>
        /// <param name="returnedTile"></param>
        public async Task<List<Tile>> PerformDumpAction(string actionSender, Tile returnedTile)
        {
            var tiles = _tileManager.PerformDumpAction(returnedTile);


            //
            // If the dump failed, return an empty tile list
            //

            if (tiles == null)
            {
                return new List<Tile>();
            }

            var viewModel = GameBoardViewModel.GetInstance();

            _gameDataLogger.LogMove(actionSender, viewModel.GameTime, Storage.MoveType.Dump);

            viewModel.TilePileCount -= 2;

            var serverProxy = ServerProxy.GetInstance();

            if (serverProxy.messageSender != null)
            {
                // ToDo: Send the actionsender name here instead of the client name in case it's a bot that's dumping.
                await serverProxy.messageSender.SendMessage(PacketType.ClientDump);
            }

            return tiles;
        }


        /// <summary>
        /// This method is invoked whenever someone (either local or from the server) signals that
        /// they have won the game.
        /// </summary>
        /// <param name="winnerName"></param>
        public async void EndGame(string winnerName)
        {
            //
            // Log the victory event.
            //

            var viewModel = GameBoardViewModel.GetInstance();
            _gameDataLogger.LogMove(winnerName, viewModel.GameTime, Storage.MoveType.Victory);

            GameStarted = false;

            _botManager.StopBots();

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                _gameClock.Stop();

                string title = string.Format("We Have a Winner!");
                var dialog = new MessageDialog(winnerName, title);
                dialog.Commands.Add(new UICommand("OK", CommandInvokedHandler));
                await dialog.ShowAsync();
            });

            _gameDataLogger.EndLoggingSession();
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            var args = new PostGamePageNavigationArgs()
            {
                GameConnectionType = _gameConnectionType,
                GameMode = _gameMode
            };

            var currentFrame = Window.Current.Content as Frame;
            if (currentFrame != null)
            {
                currentFrame.Navigate(typeof(PostGamePage), args);
            }
        }

        #endregion // MessageReceivedHandlers

    }
}
