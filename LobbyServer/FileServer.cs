﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    internal class FileServer
    {
        private Room room;
        private string username;

        public FileServer()
        {
            room = null;
            username = "";
        }

        // Doesn't seem like it's possible to pass constructor params, so use a manual join and guards
        public void Join(string roomName, string username)
        {
            // Required setup, as other methods will throw on missing room
            room = Lobby.GetInstance().FetchRoom(roomName);  // May RoomNotFoundFault
            room.Join(username, this);  // May UnauthorisedUserFault or DuplicateConnectionFault
            this.username = username;
        }

        // All these just forward the internal logic and should be one-way
        public void Leave()
        {
            room.Leave(username);
            // Extra disconnect logic should go here, though I think they would already be deauthorised...
        }

        public void AddFile(RoomFile file)
        {
            if (room == null || username == "")
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.ProblemType = "User not in room.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in room."));
            }

            room.AddFile(file);  // May InvalidFileFault
        }

        internal void RelayFileChange()
        {
            OperationContext.Current.GetCallbackChannel<IFileServerCallback>.FileChanged();
        }

        public List<string> FetchFilenames()
        {
            if (room == null || username == "")
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.ProblemType = "User not in room.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in room."));
            }

            return room.FetchFilenames();
        }

        public RoomFile FetchFile(string filename)
        {
            if (room == null || username == "")
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.ProblemType = "User not in room.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in room."));
            }

            return room.FetchFile(filename);  // May InvalidFileFault
        }
    }
}
