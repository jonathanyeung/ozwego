using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Ozwego.Server;
using Shared;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Ozwego.Storage
{
    public class GameDataHistory
    {
        private const string BaseFileName = "GameDataHistory.gdh";
        private List<GameData> _gameDataList;
        private static GameDataHistory _instance;


        private GameDataHistory()
        {
            _gameDataList = new List<GameData>();
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
            var localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                var file = await localFolder.GetFileAsync(BaseFileName);
                await file.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
            }
        }


        /// <summary>
        /// Adds the pending game data to local storage.
        /// </summary>
        /// <returns></returns>
        public async Task StoreGameData(GameData dataToSave)
        {
            var localFolder = ApplicationData.Current.LocalFolder;

            var file = await localFolder.CreateFileAsync(
                BaseFileName,
                CreationCollisionOption.OpenIfExists);


            //
            // Retrieve existing data in storage so that it isn't overwritten.
            //

            _gameDataList.Clear();
            _gameDataList = await RetrieveGameData();
            _gameDataList.Add(dataToSave);

            using (var streamFile = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var outputNewFile = streamFile.GetOutputStreamAt(0))
                {
                    var ser = new XmlSerializer(typeof(List<GameData>));

                    ser.Serialize(outputNewFile.AsStreamForWrite(), _gameDataList);
                }
            }
        }


        /// <summary>
        /// Retrieves a list of all the game data history stored in local storage.
        /// </summary>
        /// <returns></returns>
        public async Task<List<GameData>> RetrieveGameData()
        {
            var data = new List<GameData>();

            var dataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                BaseFileName,
                CreationCollisionOption.OpenIfExists);

            if (dataFile == null)
            {
                return data;
            }

            using (IInputStream dataInputStream = await dataFile.OpenReadAsync())
            {
                try
                {
                    var ser = new XmlSerializer(typeof (List<GameData>));

                    data = (List<GameData>)ser.Deserialize(dataInputStream.AsStreamForRead());
                }
                catch (InvalidOperationException)
                {
                    // Swallow this exception which is thrown when you try to deserialize an empty obj.
                }
            }

            return data;
        }


        public async Task UploadPendingGameData()
        {
            var dataList = await RetrieveGameData();

            foreach (var gameData in dataList)
            {
                //ToDo: Does 3000 need to be adjusted?  Max seen value is 750.  Should handle an exception for buffer over run.
                var buffer = new byte[1000];

                using (var stream = new MemoryStream(buffer))
                {
                    var ser = new XmlSerializer(typeof(GameData));

                    ser.Serialize(stream, gameData);

                    var messageSender = MessageSender.GetInstance();

                    await messageSender.SendMessage(PacketType.ClientUploadGameData, buffer);
                }
            }
        }
    }
}
