using Ozwego.BuddyManagement;
using Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class UserStateChange : IBinarySerializable
    {
        /// <summary>
        /// User friend object that is changing state.
        /// </summary>
        public Friend user;

        /// <summary>
        /// The updated user state of the user Client.
        /// </summary>
        public ClientUserState newUserState;

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(user);
            writer.Write((byte)newUserState);
        }

        public void Read(System.IO.BinaryReader reader)
        {
            user.Read(reader);
            newUserState = (ClientUserState)reader.ReadByte();
        }
    }
}
