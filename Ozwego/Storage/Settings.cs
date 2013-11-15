using System;
using Ozwego.BuddyManagement;
using Windows.Storage;

namespace Ozwego.Storage
{
    public static class Settings
    {
        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        // ToDo: Remove or change these default values.
        private const string DefaultAlias = "Jonathan Yeung";
        private const string DefaultEmailAddress = "jonathanyeung@outlook.com";
        private const int DefaultLevel = 0;
        private const long DefaultExpCount = 0;
        private const int DefaultRanking = 1;
        private static readonly DateTime DefaultCreationTime = new DateTime(9999, 9, 9);


        public static Friend userInstance
        {
            get
            {
                var user = new Friend
                    {
                        Alias = Alias,
                        EmailAddress = EmailAddress,
                        Experience = Experience,
                        Level = Level,
                        Ranking = Ranking,
                        CreationTime = CreationTime
                    };

                return user;
            }
        }


        public static string Alias
        {
            get 
            {
                var alias = localSettings.Values["alias"];

                if (null == alias)
                {
                    return DefaultAlias;
                }

                return (string)alias;
            }

            set 
            {
                localSettings.Values["alias"] = value;
            }
        }


        public static string EmailAddress
        {
            get
            {
                var addr = localSettings.Values["emailAddress"];

                if (null == addr)
                {
                    return DefaultEmailAddress;
                }

                return (string)addr;
            }

            set
            {
                localSettings.Values["emailAddress"] = value;
            }
        }


        /// <summary>
        /// Level is solely determined by amount of experience. This maps to the PlayerLevel enum.
        /// </summary>
        public static int Level
        {
            get
            {
                var rank = localSettings.Values["level"];

                if (null == rank)
                {
                    return DefaultLevel;
                }

                return (int)rank;
            }

            set
            {
                localSettings.Values["level"] = value;
            }
        }


        /// <summary>
        /// Experience is gained per game and is monotonically increasing. A player can never lost
        /// experience but can gain it more quickly with good performances.
        /// </summary>
        public static long Experience
        {
            get
            {
                var exp = localSettings.Values["experience"];

                if (null == exp)
                {
                    return DefaultExpCount;
                }

                return (long)exp;
            }

            set
            {
                localSettings.Values["experience"] = value;
            }
        }


        /// <summary>
        /// Ranking is an indicator of how good a player is.  It is not monotonically increasing, 
        /// and it is a value between 1 - 100, 100 being the best.
        /// </summary>
        public static int Ranking
        {
            get
            {
                var rank = localSettings.Values["ranking"];

                if (null == rank)
                {
                    return DefaultRanking;
                }

                return (int)rank;
            }

            set
            {
                localSettings.Values["ranking"] = value;
            }
        }


        public static DateTime CreationTime
        {
            get
            {
                var ticks = localSettings.Values["creationTime"];

                if (null == ticks)
                {
                    return DefaultCreationTime;
                }

                return new DateTime((long)ticks);
            }

            set
            {
                localSettings.Values["creationTime"] = value.Ticks;
            }
        }
    }
}
