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
    public class DictionaryTests
    {
        private Dictionary _dictionary = Dictionary.GetInstance();

        [TestMethod]
        public void PopulateDictionary()
        {
            Task<bool> isReady = _dictionary.PopulateDictionary();

            isReady.Wait();

            var result = _dictionary.IsAValidWord("aardvark");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("zoo");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("notarealword");
            Assert.AreEqual(result, false);
        }
    }
}
