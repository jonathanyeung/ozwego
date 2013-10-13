using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;
using Shared;

namespace WorkerRole.Matchmaking
{
    internal struct ClientPair
    {
        public Client client;
        public int IntervalsInQueue;

        public ClientPair(Client _client, int currentInterval)
        {
            client = _client;
            IntervalsInQueue = currentInterval;
        }
    }

    /// <summary>
    /// Client-facing object that allows clients to enter the matchmaking queue. 
    /// </summary>
    public class Matchmaker
    {
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
            _queue.Enqueue(cp);
        }


        public void RemoveFromMatchmakingQueue(Client client)
        {
            var cp = _queue.First(c => c.client.UserName == client.UserName);
            throw new NotImplementedException();
        }

        #endregion


        #region Privates

        private static Matchmaker _instance;
        private Queue<ClientPair> _queue;
        private Timer _timer;
        private int _currentInterval = 0;

        private const int queueExaminationInterval = 5000;


        public int MaxClientWaitTime
        {
            get { return queueExaminationInterval * ClientDequeuer.MaximumWaitInterval; }
        }


        private Matchmaker()
        {
            _queue = new Queue<ClientPair>();

            _timer = new Timer(queueExaminationInterval);
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

            var clientGroups = ClientDequeuer.GenerateGroups(ref _queue, _currentInterval);

            foreach (ClientGroup group in clientGroups)
            {
                var messageSender = MessageSender.GetInstance();


                //
                // If client2 (or 3 & 4) are null, this means that a game could not be found for
                // the client.  Send them a message to tell the client to start a game with bots.
                //

                if (group.client2 == null)
                {

                    messageSender.SendMessage(
                            group.client1, 
                            PacketType.ServerMatchmakingGameNotFound, 
                            "");
                }
                else
                {
                    //
                    // Currently, the room members must be set up before the ServerMatchmakingGameFound
                    // packet gets sent due to a timing issue.
                    // ToDo: Fix the timing issue in the client code.
                    //

                    var roomManager = RoomManager.GetInstance();
                    var room = roomManager.CreateNewRoom(group.client1);

                    var tempClient = group.client2;
                    roomManager.AddMemberToRoom(group.client1, ref tempClient);

                    tempClient = group.client1;
                    messageSender.SendMessage(
                            tempClient,
                            PacketType.ServerMatchmakingGameFound,
                            "");

                    tempClient = group.client2;
                    messageSender.SendMessage(
                            tempClient,
                            PacketType.ServerMatchmakingGameFound,
                            "");

                    /* ToDo: Re-enable this code if the group size goes back up to 4.  Better yet,
                     * refactor this so that it scales with group # const change.
                    tempClient = group.client3;
                    roomManager.AddMemberToRoom(group.client1, ref tempClient);
                    messageSender.SendMessage(
                            tempClient,
                            Ozwego.Shared.PacketType.ServerMatchmakingGameFound,
                            "");

                    tempClient = group.client4;
                    roomManager.AddMemberToRoom(group.client1, ref tempClient);
                    messageSender.SendMessage(
                            tempClient,
                            Ozwego.Shared.PacketType.ServerMatchmakingGameFound,
                            "");
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

        // Idea: Have a dispatch timer here.  Every 10 seconds, call the clientdequeuer.  If unable to dequeue after 3 cycles, then do a brute force pairing or return with bots.

        


        #endregion


    }
}
