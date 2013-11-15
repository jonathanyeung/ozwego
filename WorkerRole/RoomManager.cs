using System.Collections.Generic;
using System.Linq;
using Shared;
using WorkerRole.DataTypes;

namespace WorkerRole
{
    /// <summary>
    /// Contains all of the rooms that have been created, and associated logic.
    /// </summary>
    public class RoomManager
    {
        private readonly List<Room> _roomList;
        private readonly object _roomListLock;
        private static RoomManager _instance;


        private RoomManager()
        {
            _roomList = new List<Room>();
            _roomListLock = new object();
        }


        public static RoomManager GetInstance()
        {
            return _instance ?? (_instance = new RoomManager());
        }


        public Room CreateNewRoom(Client host)
        {
            var newRoom = new Room(host);

            lock (_roomListLock)
            {
                _roomList.Add(newRoom);
            }

            return newRoom;
        }


        public List<Client> GetRoomMembers(Client host)
        {
            var roomMembers = new List<Client>();

            lock (_roomListLock)
            {
                foreach (var room in _roomList.Where(room => room.Host == host))
                {
                    roomMembers = room.Members;
                    break;
                }
            }

            return roomMembers;
        }


        public void AddMemberToRoom(Client roomHost, ref Client memberToAdd)
        {
            Room room;

            lock (_roomListLock)
            {
                room = _roomList.FirstOrDefault(myRoom => myRoom.Host == roomHost);
            }

            if (room == null) return;


            //
            // First, remove the member from his previous room.
            //

            RemoveMemberfromRoom(memberToAdd.Room.Host, memberToAdd);


            //
            // Now add the member to the new room.
            //

            room.Members.Add(memberToAdd);
            memberToAdd.Room = room;


            //
            // Send a message to all the existing members in the room indicating who has joined.
            //

            MessageSender.BroadcastMessage(
                room.Members, 
                PacketType.UserJoinedRoom,
                memberToAdd.UserInfo,
                memberToAdd);


            //
            // Now send a message to the person who has joined containing the list of room
            // members.
            //

            var friendList = new FriendList();

            foreach (var c in room.Members)
            {
                friendList.Friends.Add(c.UserInfo);
            }

            MessageSender.SendMessage(
                memberToAdd,
                PacketType.ServerRoomList,
                friendList);


            //
            // Now tell the joining person who the room host is.
            //

            MessageSender.SendMessage(
                memberToAdd,
                PacketType.HostTransfer,
                room.Host.UserInfo);
        }


        public void RemoveMemberfromRoom(Client roomHost, Client memberToRemove)
        {
            Room room;

            lock (_roomListLock)
            {
                room = _roomList.FirstOrDefault(myRoom => myRoom.Host == roomHost);
            }

            if (room == null) return;

            room.Members.Remove(memberToRemove);

            if (roomHost == memberToRemove)
            {
                room.ChangeToRandomNewHost();
            }

            if (0 == room.Members.Count)
            {
                lock (_roomListLock)
                {
                    _roomList.Remove(room);
                }
            }
            else
            {
                MessageSender.BroadcastMessage(
                    room.Members,
                    PacketType.UserLeftRoom,
                    memberToRemove.UserInfo,
                    memberToRemove);
            }
        }
    }
}
