using LobbyDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using LobbyCLient;



namespace LobbyClient
{
    public class FileListProxy: IFileServerCallback
    {
        ChannelFactory<IFileServer> fileFactory;
        private IFileServer server;
        private string userName;
        private string roomName;
        private MainWindow window;
        public FileListProxy(string userName, string roomName, MainWindow window)
        {
            string fileURL = "net.tcp://localhost:8100/file";
            NetTcpBinding binding = new NetTcpBinding();
            fileFactory = new DuplexChannelFactory<IFileServer>(this, binding, new EndpointAddress(fileURL)); //message factory
            server = fileFactory.CreateChannel();
            this.userName = userName;
            this.roomName = roomName;
            server.Join(roomName, userName);
        }
        public List<string> FetchNewFileList()
        {
           
            return server.FetchFileNames();

        }
        public RoomFile FetchFile(string fileName) { 

            return server.FetchFile(fileName);
        }
        public void AddFile(RoomFile file) {
            try
            {
                server.AddFile(file);
            }
            catch (FaultException<InvalidFileFault> ex)
            {
                Console.WriteLine($"File fault: {ex.Detail.problemType}");
            }
            catch (FaultException<InvalidRoomFault> ex)
            {
                Console.WriteLine($"Room fault: {ex.Detail.problemType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
        public void FileChanged()
        {
            Console.WriteLine("A File has been changed");
        }
    }
}
