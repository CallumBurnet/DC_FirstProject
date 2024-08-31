﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    internal class Lobby  // Won't move to DLL until we need it to be public
    {
        // For lack of better options to enforce data model privacy, make this a singleton
        // Roughly equivalent to just making a data server with hardcoded IP
        private static readonly Lobby instance = new Lobby();
        private Dictionary<string, LobbyServer> userConnections;
        private Dictionary<string, Room> rooms;

        private Lobby() { }
        public static Lobby GetInstance() { return instance; }

        internal bool ValidateUser(string username)
        {
            // Check that the user exists at lobby-level
            return userConnections.ContainsKey(username);
        }

        public void Join(string username, LobbyServer lobbyServer)
        {
            if (userConnections.ContainsKey(username))
            {
                // User not unique, so deny
                // TODO: Might rename to UserAuthorisationFault later?
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.ProblemType = "Username is taken.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("Username is taken."));
            }
            else
            {
                // Can add them safely
                userConnections.Add(username, lobbyServer);
            }

        }

        public void Leave(string username)
        {
            // Regardless of existence, remove from everywhere
            foreach (Room room in rooms.Values) { room.Leave(username); }
            userConnections.Remove(username);
        }

        public void MakeRoom(string roomName, string owner)
        {
            try
            {
                rooms.Add(roomName, new Room(this, roomName, owner));
            }
            catch (ArgumentException)
            {
                // Room already exists
                InvalidRoomFault fault = new InvalidRoomFault();
                fault.ProblemType = "Room already exists.";
                throw new FaultException<InvalidRoomFault>(fault, new FaultReason("Room already exists."));
            }
        }

        public void FetchRoomData(out List<string> roomNames, out List<uint> userCounts)
        {
            roomNames = new List<string>();
            userCounts = new List<uint>();
            foreach (KeyValuePair<string, Room> entry in rooms)
            {
                roomNames.Add(entry.Key);
                userCounts.Add((uint)entry.Value.Users().Count());
            }
        }

        public Room FetchRoom(string roomName)
        {
            try
            {
                return rooms[roomName];
            }
            catch (KeyNotFoundException)
            {
                RoomNotFoundFault fault = new RoomNotFoundFault();
                fault.ProblemType = "User not in lobby.";
                throw new FaultException<RoomNotFoundFault>(fault, new FaultReason("Room does not exist."));
            }
        }
    }
}
