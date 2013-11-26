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
    /// Manager for the central pool/pile of game tiles.
    /// </summary>
    public class TileManager
    {
        #region Singleton Pattern

        private static TileManager _instance;

        private TileManager()
        {
            _pile = new List<Tile>();
        }

        public static TileManager GetInstance()
        {
            return _instance ?? (_instance = new TileManager());
        }

        #endregion


        #region Privates

        private readonly List<Tile> _pile;

#if DEBUG
        private const string Filename = @"ms-appx:///Gameplay/DebuggingTileSet.xml";
#else
        private const string Filename = @"ms-appx:///Gameplay/TileSet.xml";
#endif

        // private const string Filename = @"ms-appx:///Gameplay/ReducedTileSet.xml";

        #endregion

        public async Task InitializeForNewGame()
        {
            await CreateNewPileSet();
        }


        /// <summary>
        /// Returns the number of tiles remaining in the pile
        /// </summary>
        /// <returns></returns>
        public int GetPileCount()
        {
            return _pile.Count;
        }


        /// <summary>
        /// User and server Initiated Peel Action (peel action has the same result for all players)
        /// </summary>
        /// <param name="numberofPlayers"></param>
        /// <returns>All of the tiles drawn from the pool resulting from the peel.</returns>
        public List<Tile> PerformPeelAction(int numberofPlayers)
        {
            if (numberofPlayers <= 0)
            {
                throw new ArgumentException("Invalid number of players.");
            }
            //
            // In single player mode, because the room is empty, the number of players may be 0.
            // In this case, recorrect the number to 1 (to account for the single person playing).
            //

            if (numberofPlayers <= 0)
            {
                numberofPlayers = 1;
            }

            return SelectRandomTileSetFromPile(numberofPlayers);
        }


        /// <summary>
        /// User (not server) initiated dump action
        /// </summary>
        /// <param name="returnedTile"></param>
        /// <returns>null if dump failed, otherwise returns 3 tiles.</returns>
        public List<Tile> PerformDumpAction(Tile returnedTile)
        {
            var tiles = SelectRandomTileSetFromPile(3);


            //
            // If the dump action failed (b/c there aren't enough tiles left to do a dump, then
            // don't return the tile to the pile.  Instead, let the GameController return it 
            // back to the tile rack.
            //

            if (tiles != null)
            {
                _pile.Add(returnedTile);
            }

            return tiles;
        }


        /// <summary>
        /// Server initiated dump action
        /// </summary>
        public void DumpReceived(Tile tileOne, Tile tileTwo)
        {
            //ToDo: Use the arguments here and remove those tiles instead of 2 random ones.
            SelectRandomTileFromPile();
            SelectRandomTileFromPile();
        }


        /// <summary>
        /// Method to initialize the pile set for a player at the start of the game.
        /// </summary>
        /// <param name="tileCount"></param>
        /// <returns></returns>
        public List<Tile> SelectRandomTileSetFromPile(int tileCount)
        {
            if (tileCount > _pile.Count)
            {
                return null;
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


        #region Private Methods

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


        /// <summary>
        /// Helper method to select a single random tile from the pile
        /// </summary>
        /// <returns></returns>
        private Tile SelectRandomTileFromPile()
        {
            var r = new Random();
            var index = r.Next(0, _pile.Count - 1);

            var selectedTile = _pile[index];
            _pile.RemoveAt(index);

            return selectedTile;
        }

        #endregion // Private Methods
    }
}
