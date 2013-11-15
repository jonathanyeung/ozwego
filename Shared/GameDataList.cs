using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Serialization;

namespace Ozwego.Storage
{
    public class GameDataList : IBinarySerializable
    {
        public List<GameData> _GameData;

        public GameDataList()
        {
            _GameData = new List<GameData>();
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteList(_GameData);
        }

        public void Read(BinaryReader reader)
        {
            _GameData = reader.ReadList<GameData>();
        }
    }
}
