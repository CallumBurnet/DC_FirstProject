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
using Microsoft.Win32;
using System.Drawing;


namespace LobbyClient
{
    public class FileListProxy: IFileServerCallback
    {
        ChannelFactory<IFileServer> fileFactory;
        private readonly IFileServer server;
        private readonly string username;
        private readonly string roomName;
        private readonly MainWindow window;
        private DownloadWindow downloadWindow;  // A visual download indicator

        public FileListProxy(string username, string roomName, MainWindow window)
        {
            string fileURL = "net.tcp://localhost:8100/file";
            NetTcpBinding binding = new NetTcpBinding();
            binding.MaxBufferSize = 5000000;
            binding.MaxReceivedMessageSize = 5000000;
            fileFactory = new DuplexChannelFactory<IFileServer>(this, binding, new EndpointAddress(fileURL)); // file factory
            server = fileFactory.CreateChannel();

            this.username = username;
            this.roomName = roomName;
            this.window = window;
            Application.Current.Dispatcher.Invoke(() =>
            {
                downloadWindow = new DownloadWindow(this);
            });
            server.Join(roomName, username);

            // Immediately fetch files
            window.filesView.Dispatcher.Invoke(new Action(() => window.filesView.ItemsSource = new List<string>()));
            FetchNewFileList();
        }
        public void Leave()
        {
            // Unsubscribe and clean up display
            server.Leave();
            window.chatView.Dispatcher.Invoke(new Action(() => window.filesView.ItemsSource = null));
        }

        public async void FetchNewFileList()
        {
            await Task.Run(() =>
            {
                window.filesView.Dispatcher.Invoke(new Action(() => window.filesView.ItemsSource = server.FetchFileNames()));
            });
        }
        public RoomFile FetchFile(string fileName)
        {
            return server.FetchFile(fileName);
        }
        public void FileChanged()
        {
            FetchNewFileList();
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
                    
                    //Save the file to the chosen path
                    if (file.file is TextFileItem textFile)
                    {
                        File.WriteAllText(savePath, textFile.TextContent);

                        for(int i = 0; i <= 10; i++)
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

        public async void UploadFile()
        {
            await Task.Run(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                openFileDialog.Filter = "All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    RoomFile roomFile = CreateFileItem(selectedFilePath);

                    try
                    {
                        if (roomFile != null)
                        {
                            server.AddFile(roomFile);
                        }
                    }
                    catch (FaultException<InvalidFileFault> ex)
                    {
                        window.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(ex.Message);
                        });
                    }
                }
            });
        }

        private RoomFile CreateFileItem(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string extension = fileName.Length > 3 ? fileName.Substring(fileName.Length - 4).ToLower() : "";

            if (new FileInfo(filePath).Length <= 3000000)
            {
                if (extension == ".png" || extension == ".bmp")
                {
                    using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        Bitmap bitmap = new Bitmap(stream);
                        ImageFileItem fileItem = new ImageFileItem
                        {
                            fileName = fileName,
                            Bitmap = bitmap
                        };
                        return new RoomFile(fileName, extension, null, fileItem);
                    }
                }
                else if (extension == ".txt")
                {
                    string text = File.ReadAllText(filePath);
                    TextFileItem fileItem = new TextFileItem
                    {
                        fileName = fileName,
                        TextContent = text
                    };
                    return new RoomFile(fileName, extension, null, fileItem);
                }
                else
                {
                    Console.WriteLine("Unsupported file type. Upload failed.");
                    MessageBox.Show("Unsupported file type. Only bmp and png supported.");
                    return null;
                }
            }
            else
            {
                Console.WriteLine("File upload cannot exceed 3MB. Upload failed.");
                MessageBox.Show("File upload cannot exceed 3MB.");
                return null;
            }
        }

        public DownloadWindow DownloadWindow { get { return downloadWindow; } }

    }
}
