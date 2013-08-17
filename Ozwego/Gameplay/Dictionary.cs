using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Ozwego.Gameplay
{
    public class Dictionary
    {
        #region Singleton

        private static Dictionary _instance;
        private bool IsPopulated = false;

        private Dictionary()
        {
            _englishDictionary = new SortedDictionary<string, string>();
        }

        public static Dictionary GetInstance()
        {
            return _instance ?? (_instance = new Dictionary());
        }

        #endregion

        private SortedDictionary<string, string> _englishDictionary;

        private const string filename = @"ms-appx:///Gameplay/2of12inf.data";


        public async Task<bool> PopulateDictionary()
        {

            StorageFile file = null;
            bool result = true;

            if (IsPopulated)
            {
                return true;
            }

            IsPopulated = true;

            try
            {
                var uri = new System.Uri(filename);
                file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            }
            catch (FileNotFoundException)
            {
                //ToDo: do some shit
                result = false;
            }

            if (file != null)
            {
                var fileContent = await FileIO.ReadLinesAsync(file);

                foreach (var word in fileContent)
                {
                    //ToDo: Replace 1 with something perhaps more meaningful.
                    _englishDictionary.Add(word, "1");
                }
            }

            return result;
        }


        public bool IsAValidWord(string word)
        {
            return _englishDictionary.ContainsKey(word);
        }
    }
}
