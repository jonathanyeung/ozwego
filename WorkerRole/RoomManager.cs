using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationServer.Caching;

namespace WorkerRole
{
    /// <summary>
    /// Contains all of the rooms that have been created, and associated logic.
    /// </summary>
    public class RoomManager
    {
        private static RoomManager _instance;

        public static RoomManager GetRoomManager()
        {
            return _instance ?? (_instance = new RoomManager());
        }


        public void CreateNewRoom(ExternalClient host)
        {
            CacheManager.DataCache.Put(host.Information.UserName, new List<string>());
        }


        public List<string> GetRoomMembers(string host)
        {
            return CacheManager.DataCache.Get(host) as List<string>;
        }


        /// <summary>
        /// Gets the room host that the specified player is a member of.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string GetRoomHost(string username)
        {
            var userInfo = WorkerRole.ClientManager.GetClientInformation(username);
            return userInfo != null ? userInfo.RoomHost : "";
        }


        public void AddMemberToRoom(string roomHost, ref ExternalClient memberToAdd)
        {
            //
            // First, remove the member from his previous room.
            //

            RemoveMemberfromRoom(roomHost, memberToAdd);


            //
            // Now add the member to the new room.
            //

            var roomMembers = new List<string>();

            while (true)
            {
                try
                {
                    DataCacheLockHandle lockHandle;

                    //ToDo: Check this 1 second timeout and pick one that makes sense.
                    roomMembers = CacheManager.DataCache.GetAndLock(
                            roomHost, TimeSpan.FromSeconds(1), out lockHandle) as List<string>;

                    if (roomMembers != null)
                    {
                        roomMembers.Add(memberToAdd.Information.UserName);
                    }

                    CacheManager.DataCache.PutAndUnlock(roomHost, roomMembers, lockHandle);
                    break;
                }
                catch (DataCacheException e)
                {
                    //
                    // Only continue the loop if the exception is that the object is locked.
                    //

                    if (e.ErrorCode != DataCacheErrorCode.ObjectLocked)
                    {
                        Trace.WriteLine(string.Format("Error during data cache retrieval! '{0}'\n{1}", e.Message,
                                                      e.StackTrace));
                        break;
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(string.Format("Error during data cache retrieval! '{0}'\n{1}", e.Message,
                              e.StackTrace));
                    break;
                }
            }

            if (roomMembers.Count == 0)
            {
                return;
            }


            //
            // Now send a message to the person who has joined containing the list of room
            // members.
            //

            string recipients = WorkerRole.MessageSender.GetRecipientListFormattedString(
                roomMembers);

            WorkerRole.MessageSender.SendMessage(
                memberToAdd.Information.UserName,
                PacketType.ServerRoomList,
                recipients);


            //
            // Now tell the joining person who the room host is.
            //

            WorkerRole.MessageSender.SendMessage(
                memberToAdd.Information.UserName,
                PacketType.HostTransfer,
                roomHost);


            //
            // Now send a message to all of the existing room members that someone is joining.
            //

            WorkerRole.MessageSender.BroadcastMessage(
                    roomMembers,
                    PacketType.JoinRoom,
                    memberToAdd.Information.UserName,
                    memberToAdd.Information.UserName);

            }


        public void RemoveMemberfromRoom(string roomHost, ExternalClient memberToRemove)
        {

            while (true)
            {
                try
                {
                    DataCacheLockHandle lockHandle;

                    //ToDo: Check this 1 second timeout and pick one that makes sense.
                    var roomMembers = CacheManager.DataCache.GetAndLock(
                            roomHost, TimeSpan.FromSeconds(1), out lockHandle) as List<string>;

                    if (roomMembers != null)
                    {
                        //
                        // If there are no more people in the room:
                        // Else if the host is quitting:
                        // Else it's just a non-host member is quitting.
                        //

                        if (roomMembers.Count == 0)
                        {
                            //ToDo: Potential bug: can I remove before unlocking?
                            CacheManager.DataCache.Remove(roomHost);
                        }
                        else if (roomHost == memberToRemove.Information.UserName)
                        {
                            var newHost = ChangeToRandomNewHost(roomMembers);

                            //ToDo: Potential bug: can I remove before unlocking?
                            CacheManager.DataCache.Remove(roomHost);
                            CacheManager.DataCache.Add(newHost, roomMembers);

                            WorkerRole.MessageSender.BroadcastMessage(
                                    roomMembers,
                                    PacketType.HostTransfer,
                                    newHost);
                        }
                        else
                        {
                            WorkerRole.MessageSender.BroadcastMessage(
                                    roomMembers,
                                    PacketType.UserLeftRoom,
                                    memberToRemove.Information.UserName,
                                    memberToRemove.Information.UserName);

                            CacheManager.DataCache.PutAndUnlock(roomHost, roomMembers, lockHandle);
                        }
                    }

                    break;
                }
                catch (DataCacheException e)
                {
                    //
                    // Only continue the loop if the exception is that the object is locked.
                    //

                    if (e.ErrorCode != DataCacheErrorCode.ObjectLocked)
                    {
                        Trace.WriteLine(string.Format("Error during data cache retrieval! '{0}'\n{1}", e.Message,
                                                      e.StackTrace));
                        break;
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(string.Format("Error during data cache retrieval! '{0}'\n{1}", e.Message,
                              e.StackTrace));
                    break;
                }
            }
        }


        private string ChangeToRandomNewHost(List<string> members)
        {
            string host = "";
            var random = new Random();

            if (members.Count > 0)
            {
                int index = random.Next(0, members.Count - 1);
                host = members[index];
            }

            return host;
        }
    }
}
