using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Search;

namespace Ozwego.Storage
{
    public static class Settings
    {
        private static ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public static string Alias
        {
            get 
            {
                return (string)localSettings.Values["alias"];
            }
            set 
            {
                localSettings.Values["alias"] = value;
            }
        }

        public static int Ranking
        {
            get
            {
                return (int)localSettings.Values["ranking"];
            }
            set
            {
                localSettings.Values["ranking"] = value;
            }
        }
    }
}
