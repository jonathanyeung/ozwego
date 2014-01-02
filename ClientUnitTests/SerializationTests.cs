using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Ozwego.Gameplay.Ranking;
using Ozwego.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Diagnostics;
using Shared;
using Shared.Serialization;

namespace ClientUnitTests
{
    [TestClass]
    public class SerializationTests
    {
        private GameMoveDataPoint _testGameMoveDataPoint;
        private PlayerGameStats _playerGameStats;
        private GameData _gameData;
        private GameDataList _gameDataList;
        private PacketV1 _stringPacketV1;
        private PacketV1 _IBinarySerializablePacketV1;
        private PacketBase _packetBase;
        private UserStateChange _userStateChange;

        [TestInitialize()]
        public void Initialize()
        {
            _testGameMoveDataPoint = new GameMoveDataPoint { MoveType = MoveType.Dump, Player = "NewPlayer", TimeOfMove = 5 };

            _playerGameStats = new PlayerGameStats
                {
                    AvgTimeBetweenDumps = 10,
                    AvgTimeBetweenPeels = 5,
                    IsWinner = true,
                    NumberOfDumps = 3,
                    NumberOfPeels = 0,
                    PerformedFirstPeel = false,
                    RawGameData = new List<GameMoveDataPoint>() {_testGameMoveDataPoint, _testGameMoveDataPoint, _testGameMoveDataPoint}
                };

            _gameData = new GameData {GameDuration = 10, GameHost = "GameHost", GameStartTime = DateTime.Now, Winner = "NewPlayer"};
            _gameData.GameMoves.Add(_testGameMoveDataPoint);
            _gameData.GameMoves.Add(_testGameMoveDataPoint);
            _gameData.PlayerDictionary.Add("NewPlayer", _playerGameStats);
            _gameData.PlayerDictionary.Add("NewPlayer2", _playerGameStats);

            _gameDataList = new GameDataList();
            _gameDataList._GameData.Add(_gameData);
            _gameDataList._GameData.Add(_gameData);

            _stringPacketV1 = new PacketV1
                {
                    PacketType = PacketType.c_QueryIfAliasAvailable,
                    Data = "Mobius",
                    Recipients = null,
                    Sender = "NewPlayer@outlook.com"
                };

            _IBinarySerializablePacketV1 = new PacketV1
            {
                PacketType = PacketType.c_UploadGameData,
                Data = _gameData,
                Recipients = null,
                Sender = "NewPlayer@outlook.com"
            };

            _packetBase = new PacketBase
                {
                    PacketVersion = PacketVersion.Version1,
                    Data = _stringPacketV1
                };
        }


        [TestMethod]
        public void GameMoveDataPoint()
        {
            var obj = _testGameMoveDataPoint;

            var returnObj = SerializeAndDeserializeObject(obj, typeof(GameMoveDataPoint)) as GameMoveDataPoint;

            Assert.AreEqual(returnObj.MoveType, obj.MoveType);

            Assert.AreEqual(returnObj.Player, obj.Player);

            Assert.AreEqual(returnObj.TimeOfMove, obj.TimeOfMove);
        }


        [TestMethod]
        public void PlayerGameStats()
        {
            var obj = _playerGameStats;

            var returnObj = SerializeAndDeserializeObject(obj, typeof(PlayerGameStats)) as PlayerGameStats;

            Assert.AreEqual(returnObj.AvgTimeBetweenDumps, obj.AvgTimeBetweenDumps);
            Assert.AreEqual(returnObj.AvgTimeBetweenPeels, obj.AvgTimeBetweenPeels);
            Assert.AreEqual(returnObj.IsWinner, obj.IsWinner);
            Assert.AreEqual(returnObj.NumberOfDumps, obj.NumberOfDumps);
            Assert.AreEqual(returnObj.NumberOfPeels, obj.NumberOfPeels);
            Assert.AreEqual(returnObj.RawGameData[0].MoveType, obj.RawGameData[0].MoveType);
            Assert.AreEqual(returnObj.RawGameData[1].MoveType, obj.RawGameData[1].MoveType);
            Assert.AreEqual(returnObj.RawGameData[2].MoveType, obj.RawGameData[2].MoveType);
        }

        [TestMethod]
        public void GameData()
        {
            var obj = _gameData;

            var returnObj = SerializeAndDeserializeObject(obj, typeof(GameData)) as GameData;

            //ToDo: Add Assert validation statements.
        }

        [TestMethod]
        public void GameDataList()
        {
            var obj = _gameDataList;

            var returnObj = SerializeAndDeserializeObject(obj, typeof(GameDataList)) as GameDataList;

            //ToDo: Add Assert validation statements.
        }

        [TestMethod]
        public void StringPacketV1()
        {
            var obj = _stringPacketV1;

            var returnObj = SerializeAndDeserializeObject(obj, typeof(PacketV1)) as PacketV1;

            //ToDo: Add Assert validation statements.
        }

        [TestMethod]
        public void IBinarySerializablePacketV1()
        {
            var obj = _IBinarySerializablePacketV1;

            var returnObj = SerializeAndDeserializeObject(obj, typeof(PacketV1)) as PacketV1;

            //ToDo: Add Assert validation statements.
        }

        [TestMethod]
        public void PacketBase()
        {
            var obj = _packetBase;

            var returnObj = SerializeAndDeserializeObject(obj, typeof(PacketBase)) as PacketBase;

            //ToDo: Add Assert validation statements.
        }

        [TestMethod]
        public void UserStateChange()
        {
            var obj = _packetBase;

            var returnObj = SerializeAndDeserializeObject(obj, typeof(PacketBase)) as PacketBase;

            //ToDo: Add Assert validation statements.
        }




        private IBinarySerializable SerializeAndDeserializeObject(IBinarySerializable obj, Type objType)
        {
            dynamic buffer;

            using (var stream = new MemoryStream())
            {
                var binaryWriter = new BinaryWriter(stream);

                obj.Write(binaryWriter);

                buffer = stream.ToArray();
            }

            var returnObj = Activator.CreateInstance(objType) as IBinarySerializable;

            using (var readStream = new MemoryStream(buffer))
            {
                var binaryReader = new BinaryReader(readStream);
                returnObj.Read(binaryReader);
            }

            return returnObj;
        }
    }
}
