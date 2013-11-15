using System;
using System.Collections.Generic;
using Shared;
using System.Linq;

namespace WorkerRole
{
    public enum RoomStatus
    {
        InLobby,
        WaitingForGameStart,
        InGame,
    }

    /// <summary>
    /// Represents a game room.
    /// </summary>
    public class Room
    {
        private Client _host;
        private readonly List<Client> _members;
        public RoomStatus RoomStatus;

        private List<Client> _clientsReadyForGame;



        private readonly object roomLock;

        public Client Host
        {
            get
            {
                lock (roomLock)
                {
                    return _host;
                }
            }
        }

        // ToDo: This looks messed up.  Not sure locks are required, not sure a getter is needed here (just make it public?)
        public List<Client> Members
        {
            get
            {
                lock (roomLock)
                {
                    return _members;
                }
            }
        }

        internal Room(Client host)
        {
            _host = host;
            _members = new List<Client> {_host};
            _clientsReadyForGame = new List<Client>();

            roomLock = new object();
        }


        public void AddMember(Client memberToAdd)
        {
            lock (roomLock)
            {
                _members.Add(memberToAdd);
            }
        }


        public void RemoveMember(Client memberToRemove)
        {
            lock (roomLock)
            {
                var member =
                    _members.FirstOrDefault(m => m.UserInfo.EmailAddress == memberToRemove.UserInfo.EmailAddress);

                if (null != member)
                {
                    _members.Remove(member);
                }
            }
        }

        public void ChangeHost(Client host)
        {
            lock (roomLock)
            {
                _host = host;
            }
        }


        public void ChangeToRandomNewHost()
        {
            var random = new Random();

            lock (roomLock)
            {
                if (_members.Count > 0)
                {
                    var index = random.Next(0, _members.Count - 1);
                    _host = _members[index];
                }

                MessageSender.BroadcastMessage(
                    _members,
                    PacketType.HostTransfer,
                    _host.UserInfo);
            }
        }


        public string GetHostAddress()
        {
            lock (roomLock)
            {
                return _host.UserInfo.EmailAddress;
            }
        }

        public void SignalClientIsReadyForGame(Client client)
        {
            // ToDo: Add some sort of time out guard here and drop clients if they take too long to respond.
            _clientsReadyForGame.Add(client);

            //
            //  If this condition has been met, it means that all of the clients are ready.
            //

            if (_clientsReadyForGame.Count == _members.Count)
            {
                MessageSender.BroadcastMessage(_members, PacketType.ServerGameStart, null);

                _clientsReadyForGame.Clear();

                RoomStatus = RoomStatus.InGame;
            }
        }
    }
}
