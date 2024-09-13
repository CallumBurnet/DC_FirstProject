using LobbyDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace LobbyServer
{
    internal class Lobby  // Won't move to DLL until we need it to be public
    {
        // For lack of better options to enforce data model privacy, make this a singleton
        // Roughly equivalent to just making a data server with hardcoded IP
        private static readonly Lobby instance = new Lobby();
        private readonly Dictionary<string, LobbyServer> userConnections = new Dictionary<string, LobbyServer>();
        private readonly Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        private readonly Object userConnectionsLock = new Object();  // Lock class isn't available in this .NET ver
        private readonly Object roomsLock = new Object();

        private Lobby() { }
        public static Lobby GetInstance() { return instance; }

        public bool ValidateUser(string username)
        {
            // Check that the user exists at lobby-level
            lock (userConnectionsLock)
            {
                return userConnections.ContainsKey(username);
            }
        }
        
       

        public void Join(string username, LobbyServer lobbyServer)
        {
            UnauthorisedUserFault fault = new UnauthorisedUserFault();
            

            if (username == "")
            {
                // Guard against known "unassigned" state
                fault.problemType = "Username cannot be blank.";

                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("Username cannot be blank."));
            }

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
            {
                fault.problemType = "Username can only contain letters and numbers.";
                log("Attempt to join with username \"" + username + "\" failed. Usernames can only include letters and numbers.");
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("Username can only contain letters and numbers."));
            }

            try
            {
                lock (userConnectionsLock)
                {
                    userConnections.Add(username, lobbyServer);
                }
            }
            catch (ArgumentException)
            {
                // User not unique, so deny
                // TODO: Might rename to UserAuthorisationFault later?
                log("Attempt to join with username \"" + username + "\" failed. Username is taken by another user.");
                fault.problemType = "Username is taken.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("Username is taken."));
            }
            log("User \""+ username +"\" has joined the lobby.");
        }

        public void Leave(string username)
        {
            // Regardless of existence, remove from everywhere
            lock (roomsLock)
            {
                foreach (Room room in rooms.Values) 
                { 
                    room.Leave(username); 
                    log("User \""+ username + "\" has been removed from room " + room.GetName());
                }
            }
            lock (userConnectionsLock)
            {
                userConnections.Remove(username);
                log("User \"" + username + "\" has been removed from the lobby");
            }
        }

        public void MakeRoom(string roomName, string owner)
        {
            InvalidRoomFault fault = new InvalidRoomFault();
            try
            {
                Room room = new Room(this, roomName, owner);
                lock (roomsLock)
                {
                    rooms.Add(roomName, room);
                }
            }
            catch (ArgumentException)
            {
                // Room already exists
                fault.problemType = "Room already exists.";
                log("Attempt to create room \"" + roomName + "\" has failed. Room already exists.");
                throw new FaultException<InvalidRoomFault>(fault, new FaultReason("Room already exists."));
            }
            log("Room \"" + roomName + "\" has been created.");
        }

        public void FetchRoomData(out List<string> roomNames, out List<uint> userCounts)
        {
            roomNames = new List<string>();
            userCounts = new List<uint>();
            lock (roomsLock)
            {
                foreach (KeyValuePair<string, Room> entry in rooms)
                {
                    roomNames.Add(entry.Key);
                    userCounts.Add((uint)entry.Value.Users().Count());
                }
            }
        }

        public Room FetchRoom(string roomName)
        {
            try
            {
                lock (roomsLock)
                {
                    return rooms[roomName];
                }
            }
            catch (KeyNotFoundException)
            {
                InvalidRoomFault fault = new InvalidRoomFault();
                fault.problemType = "Room does not exist.";
                throw new FaultException<InvalidRoomFault>(fault, new FaultReason("Room does not exist."));
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
