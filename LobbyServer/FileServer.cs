using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    internal class FileServer
    {
        private readonly Room room;
        private readonly string username;

        public FileServer(string roomName, string username)
        {
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
            room.AddFile(file);  // May InvalidFileFault
        }

        internal void RelayFileChange()
        {
            OperationContext.Current.GetCallbackChannel<IFileServerCallback>.FileChanged();
        }

        public List<string> FetchFilenames()
        {
            return room.FetchFilenames();
        }

        public RoomFile FetchFile(string filename)
        {
            return room.FetchFile(filename);  // May InvalidFileFault
        }
    }
}
