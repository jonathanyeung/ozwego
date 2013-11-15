using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ozwego.BuddyManagement;
using Shared.Serialization;

namespace WorkerRole.DataTypes
{
    public class FriendList : IBinarySerializable
    {
        public List<Friend> Friends;

        public FriendList()
        {
            Friends = new List<Friend>();
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteList(Friends);
        }

        public void Read(BinaryReader reader)
        {
            Friends = reader.ReadList<Friend>();
        }
    }
}
