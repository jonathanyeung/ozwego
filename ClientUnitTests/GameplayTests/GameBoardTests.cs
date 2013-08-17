using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ozwego.Gameplay;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace ClientUnitTests
{
    [TestClass]
    public class GameBoardTests
    {
        private GameBoard _gameBoard = GameBoard.GetInstance();

        #region ClearBoard() Tests

        [TestMethod]
        public void ClearEmptyBoard()
        {
            _gameBoard.ClearBoard();
            var wordList = _gameBoard.GetWordList();

            Assert.IsTrue(wordList.Count == 0);
        }

        [TestMethod]
        public void ClearNonEmptyBoard()
        {
            var newTile = new Tile("A");

            _gameBoard.SetBoardSpace(newTile, 0, 0);
            _gameBoard.SetBoardSpace(newTile, GameBoard.BoardSpaceDimension - 1, 0);
            _gameBoard.SetBoardSpace(newTile, 0, GameBoard.BoardSpaceDimension - 1);
            _gameBoard.SetBoardSpace(newTile, GameBoard.BoardSpaceDimension - 1, GameBoard.BoardSpaceDimension - 1);

            _gameBoard.ClearBoard();
            var wordList = _gameBoard.GetWordList();

            Assert.IsTrue(wordList.Count == 0);
        }

        #endregion

        #region GetWordList() Tests

        [TestMethod]
        public void GetWordListOnInvalidBoard()
        {
            const string testWord = "Hello";

            var newTile = new Tile(testWord);

            _gameBoard.ClearBoard();

            _gameBoard.SetBoardSpace(newTile, 0, 0);
            _gameBoard.SetBoardSpace(newTile, GameBoard.BoardSpaceDimension - 1, 0);
            _gameBoard.SetBoardSpace(newTile, 0, GameBoard.BoardSpaceDimension - 1);
            _gameBoard.SetBoardSpace(newTile, GameBoard.BoardSpaceDimension - 1, GameBoard.BoardSpaceDimension - 1);

            var wordList = _gameBoard.GetWordList();


            //
            // This is an invalid game board, so expected word count length is 0
            //

            Assert.IsTrue(wordList.Count == 0);

            _gameBoard.ClearBoard();
        }

        [TestMethod]
        public void GetWordListYDimension()
        {
            var aTile = new Tile("a");
            var bTile = new Tile("b");
            var cTile = new Tile("c");
            var dTile = new Tile("d");
            var eTile = new Tile("e");
            var fTile = new Tile("f");

            _gameBoard.ClearBoard();

            _gameBoard.SetBoardSpace(aTile, 0, 0);
            _gameBoard.SetBoardSpace(bTile, 0, 1);
            _gameBoard.SetBoardSpace(cTile, 0, 2);
            _gameBoard.SetBoardSpace(dTile, 0, 3);
            _gameBoard.SetBoardSpace(eTile, 0, 4);
            _gameBoard.SetBoardSpace(fTile, 0, 5);

            var wordList = _gameBoard.GetWordList();

            Assert.IsTrue(wordList.Count == 1);
            Assert.AreEqual("abcdef", wordList[0]._string);
            _gameBoard.ClearBoard();
        }

        [TestMethod]
        public void GetWordListXDimension()
        {
            var aTile = new Tile("a");
            var bTile = new Tile("b");
            var cTile = new Tile("c");
            var dTile = new Tile("d");
            var eTile = new Tile("e");
            var fTile = new Tile("f");

            _gameBoard.ClearBoard();

            _gameBoard.SetBoardSpace(aTile, 0, 0);
            _gameBoard.SetBoardSpace(bTile, 1, 0);
            _gameBoard.SetBoardSpace(cTile, 2, 0);
            _gameBoard.SetBoardSpace(dTile, 3, 0);
            _gameBoard.SetBoardSpace(eTile, 4, 0);
            _gameBoard.SetBoardSpace(fTile, 5, 0);

            var wordList = _gameBoard.GetWordList();

            Assert.IsTrue(wordList.Count == 1);
            Assert.AreEqual("abcdef", wordList[0]._string);

            _gameBoard.ClearBoard();
        }

        [TestMethod]
        public void GetWordListHollowBoxShape()
        {
            /*
             *  b o b
             *  o   o
             *  b o b
             */

            _gameBoard.ClearBoard();

            var bTile = new Tile("b");
            var oTile = new Tile("o");

            _gameBoard.SetBoardSpace(bTile, 5, 5);
            _gameBoard.SetBoardSpace(bTile, 5, 7);
            _gameBoard.SetBoardSpace(bTile, 7, 5);
            _gameBoard.SetBoardSpace(bTile, 7, 7);

            _gameBoard.SetBoardSpace(oTile, 5, 6);
            _gameBoard.SetBoardSpace(oTile, 6, 5);
            _gameBoard.SetBoardSpace(oTile, 6, 7);
            _gameBoard.SetBoardSpace(oTile, 7, 6);

            var wordList = _gameBoard.GetWordList();

            Assert.IsTrue(wordList.Count == 4);

            foreach (var word in wordList)
            {
                Assert.AreEqual("bob", word._string);
            }

            _gameBoard.ClearBoard();
        }

        [TestMethod]
        public void GetWordListFourByFourShape()
        {
            /*
             *  x o x o
             *  o x o x
             *  x o x o
             *  o x o x
             */

            _gameBoard.ClearBoard();

            var xTile = new Tile("x");
            var oTile = new Tile("o");

            _gameBoard.SetBoardSpace(xTile, 0, 0);
            _gameBoard.SetBoardSpace(xTile, 0, 2);
            _gameBoard.SetBoardSpace(xTile, 1, 1);
            _gameBoard.SetBoardSpace(xTile, 1, 3);
            _gameBoard.SetBoardSpace(xTile, 2, 0);
            _gameBoard.SetBoardSpace(xTile, 2, 2);
            _gameBoard.SetBoardSpace(xTile, 3, 1);
            _gameBoard.SetBoardSpace(xTile, 3, 3);

            _gameBoard.SetBoardSpace(oTile, 0, 1);
            _gameBoard.SetBoardSpace(oTile, 0, 3);
            _gameBoard.SetBoardSpace(oTile, 1, 0);
            _gameBoard.SetBoardSpace(oTile, 1, 2);
            _gameBoard.SetBoardSpace(oTile, 2, 1);
            _gameBoard.SetBoardSpace(oTile, 2, 3);
            _gameBoard.SetBoardSpace(oTile, 3, 0);
            _gameBoard.SetBoardSpace(oTile, 3, 2);

            var wordList = _gameBoard.GetWordList();

            Assert.IsTrue(wordList.Count == 8);

            int xoxoCount = 0;
            int oxoxCount = 0;
            foreach (var word in wordList)
            {
                if (word._string == "xoxo")
                {
                    xoxoCount++;
                }
                else if (word._string == "oxox")
                {
                    oxoxCount++;
                }
            }

            Assert.AreEqual(xoxoCount, 4);
            Assert.AreEqual(oxoxCount, 4);

            _gameBoard.ClearBoard();
        }

        #endregion

        #region IsGameBoardValid Tests

        #endregion
    }
}
