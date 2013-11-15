using Ozwego.Storage;
using System;
using Shared;

namespace Ozwego.Gameplay.Ranking
{
    public static class ExperienceCalculator
    {
        public static int GetGameExperienceEarned(PlayerGameStats stats)
        {
            // Add 1000 experience for a playing the game.
            int exp = 1000;

            if (stats.IsWinner)
            {
                exp += 500;
            }

            if (stats.PerformedFirstPeel)
            {
                exp += 50;
            }

            exp += stats.NumberOfPeels * 20;

            return exp;
        }


        /// <summary>
        /// It will take approximate 1000 games to get to level 30.
        /// </summary>
        /// <param name="experience"></param>
        /// <returns></returns>
        public static PlayerLevel GetPlayerLevelFromExperienceTotal(int experience)
        {
            const double numberOfGamesToReachMaxLevel = 1000;
            const double averageExperienceEarnedPerGame = 1200;
            const int numberOfLevels = (int)PlayerLevel.MaxValue - 1;

            // ToDo: Switch back to real.
            //var experiencePerUnitIncrease = Math.Floor( (numberOfGamesToReachMaxLevel * 2 * averageExperienceEarnedPerGame) / (numberOfLevels * (numberOfLevels + 1)) );
            double experiencePerUnitIncrease = 1000;

            var numberOfUnits = (int) Math.Floor(experience/experiencePerUnitIncrease);

            var levelAttained = -1;

            
            for (int level = 0; level <= numberOfLevels + 1; level++ )
            {
                if (Math.Pow(level, 2) + level > 2 * numberOfUnits)
                {
                    levelAttained = level - 1;
                    break;
                }
            }

            // If the experience points exceeds that of the max value, set the ranking to max level.
            if (levelAttained == -1)
            {
                levelAttained = numberOfLevels;
            }

            return (PlayerLevel) levelAttained;
        }


        public static PlayerRank GetCurrentPlayerRank()
        {
            throw new NotImplementedException();
        }


    }
}
