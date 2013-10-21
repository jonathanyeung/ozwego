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
            var isReady = _dictionary.PopulateDictionary();

            isReady.Wait();

            var result = _dictionary.IsAValidWord("aardvark");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("zoo");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("notarealword");
            Assert.AreEqual(result, false);

            result = _dictionary.IsAValidWord("Irena"); // Proper Noun.
            Assert.AreEqual(result, false);

            result = _dictionary.IsAValidWord("hm");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("qi");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("op");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("aal");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("efs");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("fet");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("qat");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("zoa");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("xis");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("gnu");
            Assert.AreEqual(result, true);

            result = _dictionary.IsAValidWord("jus");
            Assert.AreEqual(result, true);


        }
    }
}
