using System.Collections.Generic;
using System.Threading.Tasks;
using Ozwego.Gameplay.Ranking;
using Ozwego.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Diagnostics;

namespace ClientUnitTests
{
    [TestClass]
    public class RankingCalculatorTests
    {

        [TestInitialize()]
        public void Initialize()
        {
        }

        [TestMethod]
        public void TestRankCalculator()
        {
            const int experienceForOneLevel = 2758;

            var returnedRank = ExperienceCalculator.GetPlayerLevelFromExperienceTotal(0);
            Assert.AreEqual(returnedRank, PlayerLevel.TypoL1);

            returnedRank = ExperienceCalculator.GetPlayerLevelFromExperienceTotal(experienceForOneLevel - 1);
            Assert.AreEqual(returnedRank, PlayerLevel.TypoL1);

            returnedRank = ExperienceCalculator.GetPlayerLevelFromExperienceTotal(experienceForOneLevel);
            Assert.AreEqual(returnedRank, PlayerLevel.TypoL2);

            returnedRank = ExperienceCalculator.GetPlayerLevelFromExperienceTotal(experienceForOneLevel + 1);
            Assert.AreEqual(returnedRank, PlayerLevel.TypoL2);

            returnedRank = ExperienceCalculator.GetPlayerLevelFromExperienceTotal(3*experienceForOneLevel - 1);
            Assert.AreEqual(returnedRank, PlayerLevel.TypoL2);

            returnedRank = ExperienceCalculator.GetPlayerLevelFromExperienceTotal(3*experienceForOneLevel);
            Assert.AreEqual(returnedRank, PlayerLevel.TypoL3);

            returnedRank = ExperienceCalculator.GetPlayerLevelFromExperienceTotal(10000000);
            Assert.AreEqual(returnedRank, PlayerLevel.TheTruthL3);
        }
    }
}
