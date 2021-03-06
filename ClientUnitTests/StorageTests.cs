﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ozwego.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Diagnostics;

namespace ClientUnitTests
{
    [TestClass]
    public class StorageTests
    {
        private GameDataHistory _gameDataHistory;

        [TestInitialize()]
        public void Initialize()
        {
            Debug.WriteLine("StorageTests.Initialize()");

            _gameDataHistory = GameDataHistory.GetInstance();
        }

        [TestMethod]
        public void TestBasicStorage()
        {
            //
            // Test methods must be synchronous, so create a Task type to block on all async calls.
            //

            Task t;


            //
            // Clear game history data from local storage.
            //

            t = _gameDataHistory.ClearAllStoredData();
            t.Wait();


            //
            // Create 2 game data sets.
            //

            var dataSet1 = CreateTestDataSet();

            var dataSet2 = CreateTestDataSet();


            //
            // Add one set and commit it to storage.
            //

            t = _gameDataHistory.StoreGameData(dataSet1);
            t.Wait();


            //
            // Add a second set and commit it to storage.
            //

            t = _gameDataHistory.StoreGameData(dataSet2);
            t.Wait();


            //
            // Retrieve the complete set of game history data from storage.
            //

            var gamesListTask = _gameDataHistory.RetrieveGameData();
            gamesListTask.Wait();
            var gamesList = gamesListTask.Result;


            //
            // Validate that there are two sets of game data in the history and that the second
            // storage commit did not overwrite the first one.
            //

            Assert.IsTrue(gamesList._GameData.Count == 2);

            t = _gameDataHistory.UploadPendingGameData();
            t.Wait();
        }


        [TestMethod]
        public void RetrieveData()
        {
            var gamesListTask = _gameDataHistory.RetrieveGameData();
            gamesListTask.Wait();
            var gamesList = gamesListTask.Result;
        }


        [TestMethod]
        public void 
            SettingsTest()
        {
            var alias = Settings.Alias;
            var addr = Settings.EmailAddress;

            var exp = Settings.Experience;

            var testTime = new DateTime(2013, 10, 30, 10, 50, 0, 0);

            Settings.CreationTime = testTime;

            var retrievedTime = Settings.CreationTime;

            Assert.AreEqual(retrievedTime, testTime);


        }


        private static GameData CreateTestDataSet()
        {
            var dataSet = new GameData { GameDuration = 50, GameHost = "AGameHost", Winner = "NewPlayer" };

            dataSet.GameMoves.Add(new GameMoveDataPoint("Player", 5, MoveType.Peel));

            var stats = new PlayerGameStats()
            {
                AvgTimeBetweenDumps = 5,
                AvgTimeBetweenPeels = 5,
                NumberOfDumps = 3,
                NumberOfPeels = 3,
                PerformedFirstPeel = true,
                IsWinner = false,
                RawGameData = new List<GameMoveDataPoint>() { new GameMoveDataPoint("Me", 1, MoveType.Peel) }
            };

            dataSet.PlayerDictionary.Add("NewPlayer", stats);
            return dataSet;
        }
    }
}
