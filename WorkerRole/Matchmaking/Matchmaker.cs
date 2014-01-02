using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace WorkerRole.Matchmaking
{
    internal struct ClientPair
    {
        public readonly Client client;
        public readonly int IntervalsInQueue;

        public ClientPair(Client client, int currentInterval)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            this.client = client;
            IntervalsInQueue = currentInterval;
        }
    }

    /// <summary>
    /// Client-facing object that allows clients to enter the matchmaking queue. 
    /// </summary>
    public class Matchmaker
    {
        private static Matchmaker _instance;
        private Queue<ClientPair> _queue;
        private readonly object _queueLock;
        private readonly Timer _timer;
        private int _currentInterval = 0;
        private const int QueueExaminationInterval = 5000;

        #region Public Methods

        public static Matchmaker GetInstance()
        {
            return _instance ?? (_instance = new Matchmaker());
        }


        /// <summary>
        /// 
        /// </summary>
        public void JoinMatchmakingQueue(Client client)
        {
            var cp = new ClientPair(client, _currentInterval);

            lock (_queueLock)
            {
                _queue.Enqueue(cp);
            }
        }


        public void RemoveFromMatchmakingQueue(Client client)
        {
            lock (_queueLock)
            {
                var cp = _queue.First(c => c.client.UserInfo.EmailAddress == client.UserInfo.EmailAddress);
            }

            throw new NotImplementedException();
        }


        public int MaxClientWaitTime
        {
            get { return QueueExaminationInterval * ClientDequeuer.MaximumWaitInterval; }
        }

        #endregion


        #region Privates

        private Matchmaker()
        {
            _queue = new Queue<ClientPair>();
            _queueLock = new object();

            _timer = new Timer(QueueExaminationInterval);
            _timer.Elapsed += ExamineQueue;
            _timer.Start();
        }


        ~Matchmaker()
        {
            _timer.Stop();
            _timer.Elapsed -= ExamineQueue;
            _timer.Dispose();
        }


        private void ExamineQueue(object sender, ElapsedEventArgs e)
        {
            //
            // For debugging, stops timer.
            //

            if (Debugger.IsAttached)
            {
                _timer.Stop();
            }


            //
            // First update the interval variable.
            //
            _currentInterval++;
            _currentInterval = _currentInterval % ClientDequeuer.MaximumWaitInterval;

            List<ClientGroup> clientGroups;

            lock (_queueLock)
            {
                clientGroups = ClientDequeuer.GenerateGroups(ref _queue, _currentInterval);
            }

            if (null == clientGroups)
            {
#if DEBUG
                throw new NullReferenceException();
#endif
                return;
            }

            foreach (var group in clientGroups)
            {
                //
                // If client2 (or 3 & 4) are null, this means that a game could not be found for
                // the client.  Send them a message to tell the client to start a game with bots.
                //

                if (group.client2 == null)
                {

                    MessageSender.SendMessage(
                            group.client1, 
                            PacketType.s_MatchmakingGameNotFound, 
                            null);
                }
                else
                {
                    //
                    // Currently, the room members must be set up before the s_MatchmakingGameFound
                    // packet gets sent due to a timing issue.
                    // ToDo: Fix the timing issue in the client code.
                    //

                    var roomManager = RoomManager.GetInstance();
                    var room = roomManager.CreateNewRoom(group.client1);

                    var tempClient = group.client2;
                    roomManager.AddMemberToRoom(group.client1, ref tempClient);

                    tempClient = group.client1;
                    MessageSender.SendMessage(
                            tempClient,
                            PacketType.s_MatchmakingGameFound,
                            null);

                    tempClient = group.client2;
                    MessageSender.SendMessage(
                            tempClient,
                            PacketType.s_MatchmakingGameFound,
                            null);

                    /* ToDo: Re-enable this code if the group size goes back up to 4.  Better yet,
                     * refactor this so that it scales with group # const change.
                    tempClient = group.client3;
                    roomManager.AddMemberToRoom(group.client1, ref tempClient);
                    MessageSender.SendMessage(
                            tempClient,
                            Ozwego.Shared.PacketType.s_MatchmakingGameFound,
                            null);

                    tempClient = group.client4;
                    roomManager.AddMemberToRoom(group.client1, ref tempClient);
                    MessageSender.SendMessage(
                            tempClient,
                            Ozwego.Shared.PacketType.s_MatchmakingGameFound,
                            null);
                     * */
                }
            }

            if (Debugger.IsAttached)
            {
                _timer.Start();
            }
        }


        /// <summary>
        /// Generates a game room from a given list of clients
        /// </summary>
        /// <param name="clients"></param>
        private void GenerateGameRoom(List<Client> clients)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
