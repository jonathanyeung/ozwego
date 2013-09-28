using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            var dataSet1 = new GameData();
            var dataSet2 = new GameData();


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

            Assert.IsTrue(gamesList.Count == 2);
        }


        [TestMethod]
        public void RetrieveData()
        {
            var gamesListTask = _gameDataHistory.RetrieveGameData();
            gamesListTask.Wait();
            var gamesList = gamesListTask.Result;
        }
    }
}
