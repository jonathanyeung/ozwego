using System.Collections.Generic;
using System.Linq;
using Shared;

namespace WorkerRole
{
    /// <summary>
    /// Contains all of the rooms that have been created, and associated logic.
    /// </summary>
    public class RoomManager
    {
        private readonly List<Room> _roomList;

        private static RoomManager _instance;


        private RoomManager()
        {
            _roomList = new List<Room>();
        }


        public static RoomManager GetInstance()
        {
            return _instance ?? (_instance = new RoomManager());
        }


        public Room CreateNewRoom(Client host)
        {
            var newRoom = new Room(host);
            _roomList.Add(newRoom);
            return newRoom;
        }


        public List<Client> GetRoomMembers(Client host)
        {
            var roomMembers = new List<Client>();

            foreach (var room in _roomList.Where(room => room.Host == host))
            {
                roomMembers = room.Members;
                break;
            }
            return roomMembers;
        }


        public void AddMemberToRoom(Client roomHost, ref Client memberToAdd)
        {
            var room = _roomList.FirstOrDefault(myRoom => myRoom.Host == roomHost);

            if (room != null)
            {
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

                var messageSender = MessageSender.GetInstance();
                messageSender.BroadcastMessage(
                    room.Members, 
                    PacketType.UserJoinedRoom, 
                    memberToAdd.UserName,
                    memberToAdd);


                //
                // Now send a message to the person who has joined containing the list of room
                // members.
                //

                string recipients = messageSender.GetRecipientListFormattedString(
                    room.Members);

                messageSender.SendMessage(
                    memberToAdd,
                    PacketType.ServerRoomList,
                    recipients);


                //
                // Now tell the joining person who the room host is.
                //

                messageSender.SendMessage(
                    memberToAdd,
                    PacketType.HostTransfer,
                    room.Host.UserName);
            }
        }


        public void RemoveMemberfromRoom(Client roomHost, Client memberToRemove)
        {
            var room = _roomList.FirstOrDefault(myRoom => myRoom.Host == roomHost);

            if (room != null)
            {
                room.Members.Remove(memberToRemove);

                if (roomHost == memberToRemove)
                {
                    room.ChangeToRandomNewHost();
                }

                if (0 == room.Members.Count)
                {
                    _roomList.Remove(room);
                }

                else
                {
                    var messageSender = MessageSender.GetInstance();
                    messageSender.BroadcastMessage(
                        room.Members,
                        PacketType.UserLeftRoom,
                        memberToRemove.UserName,
                        memberToRemove);
                }
            }
        }
    }
}
