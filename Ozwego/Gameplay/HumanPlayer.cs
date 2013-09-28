using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Gameplay
{
    /// <summary>
    /// Represents the real end-user player.
    /// </summary>
    public class HumanPlayer : IPlayer
    {
        // The tile set of the actual player.
        private readonly HandTileSet _playerTileSet;

        public HumanPlayer()
        {
            _playerTileSet = new HandTileSet();
            _alias = App.ClientBuddyInstance.Alias;
        }

        public void InitializeForGame()
        {
            _playerTileSet.CreateNewStartingHand();
        }

        public void PerformDumpAction()
        {
            // ToDo: Try to use this method in gameboardprototype...
            throw new NotImplementedException();
            //var gameController = GameController.GetInstance();
            //gameController.PerformDumpAction(Alias, ??);
        }

        public void PerformPeelAction()
        {
            var gameController = GameController.GetInstance();
            gameController.PeelActionReceived(Alias);
        }

        public void SignalVictory()
        {
            throw new NotImplementedException();
        }

        private string _alias;
        public string Alias
        {
            get
            {
                //ToDo: This ain't right...Refactor buddy class with IPlayer class.
                return App.ClientBuddyInstance.Alias;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void PeelActionReceived(Tile incomingTile)
        {
            _playerTileSet.returnTileToHand(incomingTile);
        }

        #region Methods That Interact with Game Controller to UI.

        public List<Tile> GetTileRack()
        {
            return _playerTileSet.GetCurrentHand();
        }

        public void ReturnTileToRack(string tileContents)
        {
            var newTile = new Tile(tileContents);
            _playerTileSet.returnTileToHand(newTile);
        }


        public void RemoveTileFromRack(string tileContents)
        {
            var newTile = new Tile(tileContents);
            _playerTileSet.removeTileFromHand(newTile);
        }

        public int GetCurrentHandSize()
        {
            return _playerTileSet.GetCurrentHandSize();
        }


        #endregion


        /// <summary>
        /// This method is to be called after a tile has been played. It checks to see if all of 
        /// the tiles have been played, and if so, whether the game board is valid.  If all of
        /// these conditions are met, then a peel action occurs.
        /// </summary>
        public void OnTilePlayed()
        {
            if (0 == _playerTileSet.GetCurrentHandSize())
            {
                var gc = GameController.GetInstance();
                if (gc.AreWordsValid())
                {
                    PerformPeelAction();
                }
            }
        }
    }
}
