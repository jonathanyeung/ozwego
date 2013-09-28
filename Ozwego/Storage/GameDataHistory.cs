using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Search;
using System.Xml;

namespace Ozwego.Storage
{
    public class GameDataHistory
    {
        private const string baseFileName = "GameDataHistory.gdh";
        private List<GameData> gameDataList;
        private static GameDataHistory _instance;


        private GameDataHistory()
        {
            gameDataList = new List<GameData>();
        }


        public static GameDataHistory GetInstance()
        {
            return _instance ?? (_instance = new GameDataHistory());
        }


        /// <summary>
        /// Permanently deletes all game data from local storage.
        /// </summary>
        /// <returns></returns>
        public async Task ClearAllStoredData()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                StorageFile file = await localFolder.GetFileAsync(baseFileName);
                await file.DeleteAsync();
            }
            catch (FileNotFoundException) { }
        }


        /// <summary>
        /// Adds the pending game data to local storage.
        /// </summary>
        /// <returns></returns>
        public async Task StoreGameData(GameData dataToSave)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            StorageFile file = await localFolder.CreateFileAsync(
                    baseFileName,
                    CreationCollisionOption.OpenIfExists);


            //
            // Retrieve existing data in storage so that it isn't overwritten.
            //

            gameDataList.Clear();
            gameDataList = await RetrieveGameData();
            gameDataList.Add(dataToSave);

            using (var streamFile = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var outputNewFile = streamFile.GetOutputStreamAt(0))
                {
                    var ser = new DataContractSerializer(typeof(List<GameData>));

                    ser.WriteObject(outputNewFile.AsStreamForWrite(), gameDataList);
                }
            }
        }


        /// <summary>
        /// Retrieves a list of all the game data history stored in local storage.
        /// </summary>
        /// <returns></returns>
        public async Task<List<GameData>> RetrieveGameData()
        {
            List<GameData> data = new List<GameData>();

            StorageFile dataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    baseFileName,
                    CreationCollisionOption.OpenIfExists);

            if (dataFile == null)
            {
                return data;
            }

            using (IInputStream dataInputStream = await dataFile.OpenReadAsync())
            {
                var sessionSerializer = new DataContractSerializer(typeof(List<GameData>));

                try
                {
                    data = (List<GameData>)sessionSerializer.ReadObject(dataInputStream.AsStreamForRead());
                }
                catch (XmlException) { }
            }

            return data;
        }
    }
}
