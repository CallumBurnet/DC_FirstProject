using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
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
        private Lobby lobby;
        private readonly string name;
        private readonly string owner;
        private Dictionary<string, ServerStruct> userConnections;
        private Dictionary<string, RoomFile> files;
        // TODO: Either mutex all dictionary operations, or use concurrent dictionary

        public Room(Lobby lobby, string name, string owner)
        {
            this.lobby = lobby;
            this.name = name;
            this.owner = owner;
            userConnections = new Dictionary<string, ServerStruct>();
            files = new Dictionary<string, RoomFile>();
        }

        public List<string> Users()
        {
            return userConnections.Keys.ToList();
        }

        public void Join(string username, MessageServer messageServer)
        {
            // Guard against unauthorised user at lobby-level
            if (!lobby.ValidateUser(username))
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.problemType = "User not in lobby.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in lobby."));
            }

            // Attempt MessageServer join
            if (userConnections.ContainsKey(username))
            {
                if (userConnections[username].MessageServer == null)
                {
                    DuplicateConnectionFault fault = new DuplicateConnectionFault();
                    fault.problemType = "User already connected to the message server.";
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

        public void Join(string username, FileServer fileServer)
        {
            // Guard against unauthorised user at lobby-level
            if (!lobby.ValidateUser(username))
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.problemType = "User not in lobby.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in lobby."));
            }

            // Attempt MessageServer join
            if (userConnections.ContainsKey(username))
            {
                if (userConnections[username].MessageServer == null)
                {
                    DuplicateConnectionFault fault = new DuplicateConnectionFault();
                    fault.problemType = "User already connected to the file server.";
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

        public void Leave(string username)
        {
            // Won't do any extra validation. Just remove them
            userConnections.Remove(username);
        }

        public void SendPrivateMessage(string message, string from, string to)
        {
            // Ensure the target is in the room
            if (!Users().Contains(from))
            {
                UserNotFoundFault fault = new UserNotFoundFault();
                fault.problemType = "Target user not in lobby.";
                throw new FaultException<UserNotFoundFault>(fault, new FaultReason("Target user not in lobby."));
            }

            // Send the edited message to the known client
            // TODO: Need to decide if we add timestamp here or purely client-side?
            RelayMessage($"{from}: {message}", new List<MessageServer>() { userConnections[to].MessageServer });
        }

        public void SendPublicMessage(string message, string from)
        {
            // Send the edited message to all but the origin
            List<MessageServer> filteredTargets = userConnections.Where(i => !i.Key.Equals(from)).Select(d => d.Value.MessageServer).ToList();
            RelayMessage($"{from}: {message}", filteredTargets);
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
                Console.WriteLine("ADDED");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Failed");

                InvalidFileFault fault = new InvalidFileFault();
                fault.problemType = "File already exists.";
                throw new FaultException<InvalidFileFault>(fault, new FaultReason("File already exists."));
            }
        }

        private void RelayFileChange()
        {
            // Broadcast to let clients know when to call FetchFilenames()
            foreach (FileServer target in userConnections.Select(d => d.Value.FileServer))
            {
                target.RelayFileChange();
            }
        }

        public List<string> FetchFilenames()
        {
            return files.Keys.ToList();
        }

        public RoomFile FetchFile(string filename)
        {
            try
            {
                return files[filename];
            }
            catch (KeyNotFoundException)
            {
                InvalidFileFault fault = new InvalidFileFault();
                fault.problemType = "File does not exist.";
                throw new FaultException<InvalidFileFault>(fault, new FaultReason("File does not exist."));
            }
        }
    }
}
