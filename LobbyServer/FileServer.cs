using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using LobbyDLL;
namespace LobbyServer
{
    internal class FileServer : IFileServer
    {
        private Room room;
        private string username;
        private readonly IFileServerCallback callback;

        public FileServer()
        {
            room = null;
            username = "";
            callback = OperationContext.Current.GetCallbackChannel<IFileServerCallback>();
        }

        // Doesn't seem like it's possible to pass constructor params, so use a manual join and guards
        public void Join(string roomName, string username)
        {
            // Required setup, as other methods will throw on missing room
            room = Lobby.GetInstance().FetchRoom(roomName);  // May InvalidRoomFault
            room.Join(username, this);  // May UnauthorisedUserFault or DuplicateConnectionFault
            this.username = username;
        }

        // All these just forward the internal logic and should be one-way
        public void Leave()
        {
            room.Leave(username);
        }

        public void AddFile(RoomFile file)
        {
            if (room == null || string.IsNullOrEmpty(username))
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.problemType = "User not in room.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in room."));
            }
            else
            {
                room.AddFile(file);  // May InvalidFileFault
            }
        }

        internal void RelayFileChange()
        {
            // Just ping the user that file list changed
            callback.FileChanged();
        }

        public List<string> FetchFileNames()
        {
            if (room == null || username == "")
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.problemType = "User not in room.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in room."));
            }
            return room.FetchFilenames();
        }

        public RoomFile FetchFile(string filename)
        {
            if (room == null || username == "")
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.problemType = "User not in room.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in room."));
            }
            return room.FetchFile(filename);  // May InvalidFileFault
        }
    }
}
