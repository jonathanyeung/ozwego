using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole.Matchmaking
{
    public struct ClientGroup
    {
        public Client client1;
        public Client client2;
        public Client client3;
        public Client client4;
    }


    /// <summary>
    /// This class is internal to matchmaking.  It dequeues clients from the matchmaking queue
    /// in an algorithm that takes into account time spent in the queue and player skill level.
    /// </summary>
    internal static class ClientDequeuer
    {
        private const int GroupSize = 2;
        private const int MaximumWaitIntervals = 4;

        public static int MaximumWaitInterval
        {
            get {return MaximumWaitIntervals;}
        }
    
        public static List<ClientGroup> GenerateGroups(ref Queue<ClientPair> queue, int currentInterval)
        {
            var clientGroups = new List<ClientGroup>();
            var clientGroup = new ClientGroup();

            while (queue.Count >= GroupSize)
            {
                //
                // For now, do a simple FIFO algorithm to generate the groups.
                //

                var clientPair = queue.Dequeue();
                clientGroup.client1 = clientPair.client;

                clientPair = queue.Dequeue();
                clientGroup.client2 = clientPair.client;

                /* ToDo: Re-enable this if Group Size goes back up to 4.
                clientPair = queue.Dequeue();
                clientGroup.client3 = clientPair.client;

                clientPair = queue.Dequeue();
                clientGroup.client4 = clientPair.client;
                */

                clientGroups.Add(clientGroup);
            }


            //
            // If there are any remaining clients, check to see if they have gone through a full
            // cycle of intervals, which means that they should be dequeued.
            //

            if (queue.Count == 0)
            {
                return clientGroups;
            }


            //
            // If a client has been in the queue for the maximum time, dequeue them into a 
            // new clientGroup which only contains them.
            //

            while (queue.ElementAt(0).IntervalsInQueue == currentInterval)
            {
                var cp = queue.Dequeue();

                clientGroup = new ClientGroup();
                clientGroup.client1 = cp.client;

                clientGroups.Add(clientGroup);

                if (queue.Count == 0)
                {
                    break;
                }
            }

            return clientGroups;
        }
    }
}
