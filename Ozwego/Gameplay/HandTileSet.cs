using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Gameplay
{
    /// <summary>
    /// Represents the player's set of tiles in his hand.  This class should be instantiated for 
    /// both the real player and any bots in the game.
    /// </summary>
    public class HandTileSet
    {
        private List<Tile> _currentHand;
        private const int _startingHandSize = 21;
        //private const int _startingHandSize = 5;

        public HandTileSet()
        {
            _currentHand = new List<Tile>();
        }


        /// <summary>
        /// Gets the tiles that are in the player's rack
        /// </summary>
        /// <returns></returns>
        public List<Tile> GetCurrentHand()
        {
            return _currentHand;
        }


        public int GetCurrentHandSize()
        {
            return _currentHand.Count;
        }


        public void returnTileToHand(Tile returningTile)
        {
            _currentHand.Add(returningTile);
        }


        public void removeTileFromHand(Tile tile)
        {
            var tileToRemove = _currentHand.FirstOrDefault(t => t.TileContents == tile.TileContents);
            _currentHand.Remove(tileToRemove);
        }


        public void CreateNewStartingHand()
        {
            var tileManager = TileManager.GetInstance();

            var pileCount = tileManager.GetPileCount();
            if (pileCount < _startingHandSize)
            {
                throw new IndexOutOfRangeException
                    ("The pile count is smaller than the starting hand count");
            }

            _currentHand.Clear();

            _currentHand = tileManager.SelectRandomTileSetFromPile(_startingHandSize);
        }


        /// <summary>
        /// This method is intended for bots only.  It's meant to remove a single tile.
        /// </summary>
        public Tile removeRandomTileFromHand()
        {
            Tile returnTile = null;

            if (_currentHand.Count > 0)
            {
                returnTile = _currentHand[0];
                _currentHand.RemoveAt(0);
            }

            return returnTile;
        }

    }
}
