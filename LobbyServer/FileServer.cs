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
        private Dictionary<string, RoomFile> fileStorage;

        public FileServer()
        {
            room = null;
            username = "";
            fileStorage = new Dictionary<string, RoomFile>();
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
            // Extra disconnect logic should go here, though I think they would already be deauthorised...
        }

        public void AddFile(RoomFile file)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add file {ex.Message}");
                throw;
            }
        }

        internal void RelayFileChange()
        {
            OperationContext.Current.GetCallbackChannel<IFileServerCallback>().FileChanged();
        }
        internal void RelayFileDownload()
        {
            OperationContext.Current.GetCallbackChannel<IFileServerCallback>().DownloadProgress();

        }




        public List<string> FetchFileNames()
        {

            if (room == null || username == "")
            {
                Console.WriteLine("Couldnt fetch file names");
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.problemType = "User not in room.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in room."));
            }
            return room.FetchFilenames();
            //return new List<string>();

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
