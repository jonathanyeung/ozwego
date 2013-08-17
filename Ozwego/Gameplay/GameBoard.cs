using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;


namespace Ozwego.Gameplay
{
    public class GameBoard
    {
        struct BoardSpace
        {
            public Tile _tile;

            // Bool if the space on the board occupied by a tile.
            public bool _isOccupied;

        }

        /// <summary>
        /// Used to check for board validity in IsGameBoardValid();
        /// </summary>
        private enum TileValidity
        {
            Unset,
            Valid,
            Unoccupied
        }


        // The number of spaces on each axis of the game board.
        // ToDo: Should this be equal to the number of tiles?
        public const int BoardSpaceDimension = 31; // This value should be odd! Remember to update corresponding value in GameBoardPrototype.cs

        private readonly BoardSpace[,] _boardMatrix;

        // Singleton
        private static GameBoard _instance;

        public int GetBoardDimension()
        {
            return BoardSpaceDimension;
        }


        /// <summary>
        /// Private Constructor
        /// </summary>
        private GameBoard()
        {
            _boardMatrix = new BoardSpace[BoardSpaceDimension, BoardSpaceDimension];
            ClearBoard();
        }


        /// <summary>
        /// Public method to instantiate ServerGameBoard singleton.
        /// </summary>
        /// <returns></returns>
        public static GameBoard GetInstance()
        {
            return _instance ?? (_instance = new GameBoard());
        }


        public void ClearBoard()
        {
            for (int i = 0; i < BoardSpaceDimension; i++)
            {
                for (int j = 0; j < BoardSpaceDimension; j++)
                {
                    _boardMatrix[i, j]._isOccupied = false;
                    _boardMatrix[i,j]._tile = new Tile();
                }
            }
        }




        public void SetBoardSpace(Tile tile, int i, int j)
        {
            Debug.Assert(i >= 0);
            Debug.Assert(i < BoardSpaceDimension);
            Debug.Assert(j >= 0);
            Debug.Assert(j < BoardSpaceDimension);

            if (tile.TileContents.Length > 0)
            {
                _boardMatrix[i, j]._isOccupied = true;
                _boardMatrix[i, j]._tile = tile;
            }
        }

        public bool IsBoardSpaceOccupied(int i, int j)
        {
            //ToDo: Do proper argument checking on all of these methods.

            return _boardMatrix[i, j]._isOccupied;
        }

        public void ClearBoardSpace(int i, int j)
        {
            _boardMatrix[i, j]._isOccupied = false;
            _boardMatrix[i, j]._tile = new Tile();
        }


        public struct WordCoordinate
        {
            public String _string;
            public Point _begin;
            public Point _end;
        }


        /// <summary>
        /// Gets the list of words formed by the game board.  This method scans column by column,
        /// row by row, and returns all of the formed words.
        /// </summary>
        /// <returns>List of all of the words formed. If the gameboard is invalid, an empty list
        /// is returned.</returns>
        public List<WordCoordinate> GetWordList()
        {
            var wordsFormedOnGameBoard = new List<WordCoordinate>();


            //
            // If the game board is invalid, return an empty list.
            //

            if (!IsGameBoardValid())
            {
                return wordsFormedOnGameBoard;
            }


            //
            // Iterate through all of the columns.
            //

            for (var i = 0; i < BoardSpaceDimension; i++)
            {
                int j = 0;

                while (j < BoardSpaceDimension)
                {
                    if (_boardMatrix[i, j]._isOccupied)
                    {
                        int jStart = j;

                        var wc = new WordCoordinate
                        {
                            _string = "",
                            _begin = new Point(i, j)
                        };

                        do
                        {
                           wc._string += _boardMatrix[i, j]._tile.TileContents;
                            j++;
                        } 
                        while ((j < BoardSpaceDimension) && (_boardMatrix[i, j]._isOccupied));

                        wc._end = new Point(i, j - 1);

                        //
                        // Only add a word if it's greater than one tile in length.
                        //

                        if (j - jStart > 1)
                        {
                            wordsFormedOnGameBoard.Add(wc);
                        }
                    }

                    j++;
                }
            }


            //
            // Iterate through all of the rows
            //

            for (var j = 0; j < BoardSpaceDimension; j++)
            {
                int i = 0;

                while (i < BoardSpaceDimension)
                {
                    if (_boardMatrix[i, j]._isOccupied)
                    {
                        int iStart = i;

                        var wc = new WordCoordinate
                        {
                            _string = "",
                            _begin = new Point(i, j)
                        };

                        do
                        {
                            wc._string += _boardMatrix[i, j]._tile.TileContents;
                            i++;
                        }
                        while ((i < BoardSpaceDimension) && (_boardMatrix[i, j]._isOccupied));

                        wc._end = new Point(i - 1, j);

                        //
                        // Only add a word if it's greater than one tile in length.
                        //

                        if (i - iStart > 1)
                        {
                            wordsFormedOnGameBoard.Add(wc);
                        }
                    }

                    i++;
                }
            }

            return wordsFormedOnGameBoard;
        }


        /// <summary>
        /// Determines whether the game board is valid or not. Valid is defined as having all of
        /// the game tiles connected together. NOTE - this method does not check to see whether
        /// all of the game tiles are on the game board and none are remaining in the tile rack.
        /// </summary>
        /// <returns></returns>
        public bool IsGameBoardValid()
        {
            bool isValid = true;
            int unoccupiedTileCount = 0;
            int markedTileCount = 0;

            //ToDo: Does this array need to be initialized to some default value?
            var connections = new TileValidity[BoardSpaceDimension,BoardSpaceDimension];

            //
            // Start with marking all of the unoccupied spots.
            //

            for (int i = 0; i < BoardSpaceDimension; i++)
            {
                for (int j = 0; j < BoardSpaceDimension; j++)
                {
                    if (!_boardMatrix[i, j]._isOccupied)
                    {
                        connections[i, j] = TileValidity.Unoccupied;
                        unoccupiedTileCount++;
                    }
                    else if (connections[i,j] != TileValidity.Valid)
                    {
                        markedTileCount = MarkConnectedTiles(ref connections, i, j);
                    }

                }
            }

            //ToDo: Find some way to mark the smallest groups of unoccupied tiles as Invalid.
            if ((markedTileCount + unoccupiedTileCount) != BoardSpaceDimension*BoardSpaceDimension)
            {
                isValid = false;
            }

            return isValid;
        }


        /// <summary>
        /// Helper method for IsGameBoardValid().  This method is recursive and finds the number
        /// of connected tiles.
        /// </summary>
        /// <param name="connections"></param>
        /// <param name="i">X index into the connections array to examine.</param>
        /// <param name="j">Y index into the connections array to examine.</param>
        /// <returns>The count of connected tiles.</returns>
        private int MarkConnectedTiles(ref TileValidity[,] connections, int i, int j)
        {
            Debug.Assert(0 < i && i < BoardSpaceDimension);
            Debug.Assert(0 < j && j < BoardSpaceDimension);

            int tileCountMarked = 1;


            //
            // Mark current tile as valid.
            //

            connections[i,j] = TileValidity.Valid;


            //
            // Check the tile to the West.
            //

            if (i > 0)
            {
                if ((_boardMatrix[i - 1, j]._isOccupied) && (connections[i - 1, j] != TileValidity.Valid))
                {
                    tileCountMarked += MarkConnectedTiles(ref connections, i - 1, j);
                }
            }


            //
            // Check the tile to the East.
            //

            if (i < BoardSpaceDimension - 1)
            {
                if ((_boardMatrix[i + 1, j]._isOccupied) && (connections[i + 1, j] != TileValidity.Valid))
                {
                    tileCountMarked += MarkConnectedTiles(ref connections, i + 1, j);
                }
            }


            //
            // Check the tile to the North
            //

            if (j > 0)
            {
                if ((_boardMatrix[i, j - 1]._isOccupied) && (connections[i, j - 1] != TileValidity.Valid))
                {
                    tileCountMarked += MarkConnectedTiles(ref connections, i, j - 1);
                }
            }


            //
            // Check the tile to the South
            //

            if (j < BoardSpaceDimension - 1)
            {
                if ((_boardMatrix[i, j + 1]._isOccupied) && (connections[i, j + 1] != TileValidity.Valid))
                {
                    tileCountMarked += MarkConnectedTiles(ref connections, i, j + 1);
                }
            }

            return tileCountMarked;
        }
    }
}
