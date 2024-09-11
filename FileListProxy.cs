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
using System.Windows;
using System.Threading;


namespace LobbyClient
{
    public class FileListProxy: IFileServerCallback
    {
        ChannelFactory<IFileServer> fileFactory;
        private IFileServer server;
        private string userName;
        private string roomName;
        private MainWindow window;
        private DownloadWindow downloadWindow;
        public FileListProxy(string userName, string roomName, MainWindow window)
        {
            string fileURL = "net.tcp://localhost:8100/file";
            NetTcpBinding binding = new NetTcpBinding();
            binding.MaxBufferSize = 5000000;
            binding.MaxReceivedMessageSize = 5000000;
            fileFactory = new DuplexChannelFactory<IFileServer>(this, binding, new EndpointAddress(fileURL)); //message factory
            server = fileFactory.CreateChannel();
            this.userName = userName;
            this.roomName = roomName;
            Application.Current.Dispatcher.Invoke(() =>
            {
                downloadWindow = new DownloadWindow(this);
            });
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
        public async Task DownloadFile(string fileName, IProgress<int> progress, CancellationToken token)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.FileName = fileName;
            saveFileDialog.Filter = "All files|*.*";
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                string savePath = saveFileDialog.FileName;
                await Task.Run(() =>
                {
                    RoomFile file = server.FetchFile(fileName);
                    //

                    
                    //Save the file to the chosen path
                    if (file.file is TextFileItem textFile)
                    {
                        
                        File.WriteAllText(savePath, textFile.TextContent);

                        for(int i = 0;i <= 10; i++)
                        {
                            token.ThrowIfCancellationRequested();
                            progress.Report(i*10);
                            Thread.Sleep(10);
                        }

                    }
                    else if (file.file is ImageFileItem imageFile)
                    {
                        imageFile.Bitmap.Save(savePath);
                        for (int i = 0; i <= 10; i++)
                        {
                            token.ThrowIfCancellationRequested();
                            progress.Report(i * 10);
                            Thread.Sleep(10);
                        }
                    }
                }, token);
            }
        }

        public DownloadWindow DownloadWindow { get { return downloadWindow; } }

    }
}
