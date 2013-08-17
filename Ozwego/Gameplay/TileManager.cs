using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;

namespace Ozwego.Gameplay
{
    /// <summary>
    /// Represents a single game tile
    /// </summary>
    public class Tile
    {
        public string TileContents;

        public Tile()
        {
            TileContents = "";
        }

        public Tile(string tileContents)
        {
            TileContents = tileContents;
        }
    }

    /// <summary>
    /// Manager for all things related to the game playing tiles.
    /// </summary>
    public class TileManager
    {
        #region Singleton

        private static TileManager _instance;

        private TileManager()
        {
            _pile = new List<Tile>();
            _currentHand = new List<Tile>();
        }

        public static TileManager GetInstance()
        {
            return _instance ?? (_instance = new TileManager());
        }

        #endregion

        private List<Tile> _currentHand;

        private readonly List<Tile> _pile;

        private const int StartingHandSize = 5;

        private const string Filename = @"ms-appx:///Gameplay/ReducedTileSet.xml";


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

        public int GetStartingHandSize()
        {
            return StartingHandSize;
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


        /// <summary>
        /// Returns the number of tiles remaining in the pile
        /// </summary>
        /// <returns></returns>
        public int GetPileCount()
        {
            return _pile.Count;
        }


        public void InitializeForNewGame()
        {
            var result = CreateNewPileSet();

            result.Wait();

            CreateNewStartingHand();
        }

        /// <summary>
        /// User and server Initiated Peel Action (peel action has the same result for all players)
        /// </summary>
        /// <param name="numberofPlayers"></param>
        /// <returns></returns>
        public Tile PerformPeelAction(int numberofPlayers)
        {
            Debug.Assert(numberofPlayers > 0);

            var tiles = SelectRandomTileSetFromPile(numberofPlayers);
            return tiles[0];
        }


        /// <summary>
        /// User (not server) initiated dump action
        /// </summary>
        /// <param name="returnedTile"></param>
        /// <returns></returns>
        public List<Tile> PerformDumpAction(Tile returnedTile)
        {
            _pile.Add(returnedTile);
            return SelectRandomTileSetFromPile(3);
        }

        /// <summary>
        /// Server initiated dump action
        /// </summary>
        public void DumpReceived(Tile tileOne, Tile tileTwo)
        {
            _pile.Add(tileOne);
            _pile.Add(tileTwo);
        }



        private void CreateNewStartingHand()
        {
            if (_pile.Count < StartingHandSize)
            {
                throw new IndexOutOfRangeException
                    ("The pile count is smaller than the starting hand count");
            }

            _currentHand.Clear();

            _currentHand = SelectRandomTileSetFromPile(StartingHandSize);
        }


        /// <summary>
        /// Resets the pile
        /// </summary>
        private async Task<bool> CreateNewPileSet()
        {
            bool isReady = true;

            _pile.Clear();


            //
            // Read the tile information from the TileSet XML file to create the initial
            // pool of tiles.
            //

            StorageFile file = null;

            try
            {
                var uri = new Uri(Filename);
                file = await StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                //ToDo: do some shit
                isReady = false;
            }

            if (file != null)
            {
                using (var stream = await file.OpenReadAsync())
                {
                    using (var reader = XmlReader.Create(stream.AsStreamForRead()))
                    {
                        reader.ReadToFollowing("TileSet");

                        while (!reader.EOF)
                        {
                            reader.ReadToFollowing("Letter");

                            var tileContents = reader.GetAttribute("Value");

                            if (reader.MoveToAttribute("Count"))
                            {
                                var count = reader.ReadContentAsInt();

                                for (var i = 0; i < count; i++)
                                {
                                    var newTile = new Tile { TileContents = tileContents };
                                    _pile.Add(newTile);
                                }
                            }
                        }
                    }
                }
            }

            return isReady;
        }


        private Tile SelectRandomTileFromPile()
        {
            var r = new Random();
            var index = r.Next(0, _pile.Count - 1);

            var selectedTile = _pile[index];
            _pile.RemoveAt(index);

            return selectedTile;
        }


        private List<Tile> SelectRandomTileSetFromPile(int tileCount)
        {
            if (tileCount > _pile.Count)
            {
                throw new ArgumentException("Specified tile count exceeds the pile count.");
            }

            var tileList = new List<Tile>();
            var r = new Random();

            for (int i = 0; i < tileCount; i++)
            {
                var index = r.Next(0, _pile.Count - 1);
                var selectedTile = _pile[index];
                _pile.RemoveAt(index);
                tileList.Add(selectedTile);
            }

            return tileList;
        }

    }
}
