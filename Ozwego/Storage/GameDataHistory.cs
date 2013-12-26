using Ozwego.Server;
using Shared;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Ozwego.Storage
{
    public class GameDataHistory
    {
        private const string BaseFileName = "GameDataHistory.gd";
        private GameDataList _gameDataList;
        private static GameDataHistory _instance;


        private GameDataHistory()
        {
            _gameDataList = new GameDataList();
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
            if (dataToSave.GameDuration <= 0 ||
                dataToSave.GameHost == null ||
                dataToSave.GameMoves == null ||
                dataToSave.PlayerDictionary == null ||
                dataToSave.Winner == null)
            {
#if DEBUG
                throw new ArgumentException("Invalid game data.");
#else
                return;
#endif
            }


            //
            // Retrieve existing data in storage so that it isn't overwritten.
            //

            _gameDataList._GameData.Clear();
            _gameDataList = await RetrieveGameData();
            _gameDataList._GameData.Add(dataToSave);


            //
            // Now write the data to the file.
            //

            var localFolder = ApplicationData.Current.LocalFolder;

            var file = await localFolder.CreateFileAsync(
                BaseFileName,
                CreationCollisionOption.OpenIfExists);

            using (var streamFile = await file.OpenStreamForWriteAsync())
            {
                    var binaryWriter = new BinaryWriter(streamFile);

                    _gameDataList.Write(binaryWriter);
            }
        }


        /// <summary>
        /// Retrieves a list of all the game data history stored in local storage.
        /// </summary>
        /// <returns></returns>
        public async Task<GameDataList> RetrieveGameData()
        {
            var data = new GameDataList();

            var dataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                BaseFileName,
                CreationCollisionOption.OpenIfExists);


            if (dataFile == null)
            {
                return data;
            }


            using (var dataInputStream = await dataFile.OpenStreamForReadAsync())
            {
                try
                {
                    var binaryReader = new BinaryReader(dataInputStream);

                    data.Read(binaryReader);
                }
                catch (EndOfStreamException)
                {
                    // Swallow this exception which is thrown when you try to deserialize an empty obj.
                }
            }
            return data;
        }


        public async Task UploadPendingGameData()
        {
            var dataList = await RetrieveGameData();

            foreach (var gameData in dataList._GameData)
            {
                var messageSender = MessageSender.GetInstance();
                await messageSender.SendMessage(PacketType.ClientUploadGameData, gameData);
            }
        }
    }
}
