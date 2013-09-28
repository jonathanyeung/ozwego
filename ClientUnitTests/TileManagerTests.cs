using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ozwego.Gameplay;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace ClientUnitTests
{
    [TestClass]
    public class TileManagerTests
    {
        private TileManager _tileManager = TileManager.GetInstance();

        [TestMethod]
        public void CreateNewGame()
        {
            //_tileManager.InitializeForNewGame();

            //Assert.AreEqual(_tileManager.GetCurrentHand().Count, _tileManager.GetStartingHandSize());
        }
    }
}
