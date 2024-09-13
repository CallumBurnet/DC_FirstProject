using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

using LobbyDLL;

namespace LobbyServer
{
    internal class ServerStruct
    {
        // Bundles one or more subscription channels by a user
        // Should only be used within this file
        public MessageServer MessageServer = null;
        public FileServer FileServer = null;

    }

    internal class Room  // Won't move to DLL until we need it to be public
    {
        private readonly Lobby lobby;
        public readonly string name;
        private readonly string owner;
        private readonly Dictionary<string, ServerStruct> userConnections = new Dictionary<string, ServerStruct>();
        private readonly Dictionary<string, RoomFile> files = new Dictionary<string, RoomFile>();
        private readonly Object userConnectionsLock = new Object();
        private readonly Object filesLock = new Object();
        // TODO: Either mutex all dictionary operations, or use concurrent dictionary

        public Room(Lobby lobby, string name, string owner)
        {
            this.lobby = lobby;
            this.name = name;
            this.owner = owner;
        }

        public List<string> Users()
        {
            lock (userConnectionsLock)
            {
                return userConnections.Keys.ToList();
            }
        }

        public void Join(string username, MessageServer messageServer)
        {
            // Guard against unauthorised user at lobby-level
            if (!lobby.ValidateUser(username))
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.problemType = "User not in lobby.";
                log(username + "'s attempt to join the lobby failed as user is not in the lobby.");
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in lobby."));
            }

            // Attempt MessageServer join
            lock (userConnectionsLock)
            {
                if (userConnections.ContainsKey(username))
                {
                    if (userConnections[username].MessageServer == null)
                    {
                        DuplicateConnectionFault fault = new DuplicateConnectionFault();
                        fault.problemType = "User already connected to the message server.";
                        log(username + "'s attemp to connect to message server failed due to already being connected to room '" + name + "'.");
                        throw new FaultException<DuplicateConnectionFault>(fault, new FaultReason("User already connected to the message server."));
                    }
                    else
                    {
                        // User exists, but not subscribed for messages yet
                        userConnections[username].MessageServer = messageServer;
                    }
                }
                else
                {
                    userConnections.Add(username, new ServerStruct() { MessageServer = messageServer });
                }
            }
            log(username + " has been subscribed to the message server for room '" + name + "'.");
        }

        public void Join(string username, FileServer fileServer)
        {
            // Guard against unauthorised user at lobby-level
            if (!lobby.ValidateUser(username))
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.problemType = "User not in lobby.";
                log(username + "'s attempt to join the lobby failed as user is not in the lobby.");
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in lobby."));
            }

            // Attempt MessageServer join
            lock (userConnectionsLock)
            {
                if (userConnections.ContainsKey(username))
                {
                    if (userConnections[username].MessageServer == null)
                    {
                        DuplicateConnectionFault fault = new DuplicateConnectionFault();
                        fault.problemType = "User already connected to the file server";
                        log(username + "'s attemp to connect to message server failed due to already being connected to room '" + name + "'.");
                        throw new FaultException<DuplicateConnectionFault>(fault, new FaultReason("User already connected to the file server."));
                    }
                    else
                    {
                        // User exists, but not subscribed for messages yet
                        userConnections[username].FileServer = fileServer;
                    }
                }
                else
                {
                    userConnections.Add(username, new ServerStruct() { FileServer = fileServer });
                }
            }
            log(username + " has been subscribed to the file server for room '" + name + "'.");
        }

        public void Leave(string username)
        {
            // Won't do any extra validation. Just remove them
            lock (userConnectionsLock)
            {
                userConnections.Remove(username);
            }
        }

        public void SendPrivateMessage(string message, string from, string to)
        {
            // Ensure the target is in the room
            if (!userConnections.ContainsKey(to))
            {
                UserNotFoundFault fault = new UserNotFoundFault();
                fault.problemType = "Target user not in lobby.";
                log("Message '" + message + "' from '" + from + "' to '" + to + "' failed. " + fault.problemType);
                throw new FaultException<UserNotFoundFault>(fault, new FaultReason("Target user not in lobby."));
            }

            // Send the edited message to the known client
            // TODO: Need to decide if we add timestamp here or purely client-side?
            List<MessageServer> filteredTargets;
            lock (userConnectionsLock)
            {
                filteredTargets = new List<MessageServer>() { userConnections[to].MessageServer };
            }
            RelayMessage($"{from}: {message}", filteredTargets);
            log("Message '" + message + "' from '" + from + "' to '" + to + "' sent.");
        }

        public void SendPublicMessage(string message, string from)
        {
            // Send the edited message to all but the origin
            List<MessageServer> filteredTargets;
            lock (userConnectionsLock)
            {
                filteredTargets = userConnections.Where(i => !i.Key.Equals(from)).Select(d => d.Value.MessageServer).ToList();
            }
            RelayMessage($"{from}: {message}", filteredTargets);
            log("Message '" + message + "' from '" + from + "' sent.");

        }

        private void RelayMessage(string message, List<MessageServer> targets)
        {
            // Handles the broadcasting
            foreach (MessageServer target in targets)
            {
                target.RelayMessage(message);
            }
        }

        public void AddFile(RoomFile file)
        {
            // Don't bother with appending a enumerator tag for dupes for now. Just throw
            try
            {
                files.Add(file.name, file);
                RelayFileChange();
            }
            catch (ArgumentException)
            {
                InvalidFileFault fault = new InvalidFileFault();
                fault.problemType = "File already exists.";
                log("File '" + file + "' has failed to upload. " + fault.problemType);
                throw new FaultException<InvalidFileFault>(fault, new FaultReason("File already exists."));
            }
            log("File '" + file.name + "' has been uploaded.");

        }

        private void RelayFileChange()
        {
            // Broadcast to let clients know when to call FetchFilenames()
            lock (userConnectionsLock)  // The loop shouldn't block for too long, so it's fine
            {
                foreach (FileServer target in userConnections.Select(d => d.Value.FileServer))
                {
                    target.RelayFileChange();
                }
            }
        }

        public List<string> FetchFilenames()
        {
            lock (filesLock)
            {
                return files.Keys.ToList();
            }
        }

        public RoomFile FetchFile(string filename)
        {
            try
            {
                lock (filesLock)
                {
                    return files[filename];
                }
            }
            catch (KeyNotFoundException)
            {
                InvalidFileFault fault = new InvalidFileFault();
                fault.problemType = "File does not exist.";
                throw new FaultException<InvalidFileFault>(fault, new FaultReason("File does not exist."));
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
