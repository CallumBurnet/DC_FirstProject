using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LobbyDLL;
using System.ServiceModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LobbyClient;
using Microsoft.Win32;
using System.IO;
using System.Drawing;



namespace LobbyCLient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Declare interface variables for file, lobby and room servers.
        private IFileServer fileInterface;
        private ILobbyServer lobbyInterface;
        private MessageProxy messageProxy;
        private FileListProxy fileListProxy;
        private RoomFile roomFile;
       


        public MainWindow()
        {
            InitializeComponent();
            
            //Declare the channel factories for file, lobby and room channels.
            ChannelFactory<ILobbyServer> lobbyFactory;

            //Set up the connection
            NetTcpBinding tcp = new NetTcpBinding();
            string URL = "net.tcp://localhost:8100/MKXLobby";

            //Initialise the file, lobby and room factories.
            lobbyFactory = new ChannelFactory<ILobbyServer>(tcp, URL);

           

            //Create the factory channels.
            lobbyInterface = lobbyFactory.CreateChannel();

            //Set main window as collapsed and login window as visible by default
            mainScreen.Visibility = Visibility.Collapsed;
            loginScreen.Visibility = Visibility.Visible;

            //Create a listener for the double click on a room
            LobbyListView.MouseDoubleClick += LobbyListView_MouseDoubleClick;
            


        }
  
        private async void updateFileData()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await FetchFileData();
                    await Task.Delay(TimeSpan.FromSeconds(6)); //every 5 seconds

                }
            });
        }
        private async Task FetchFileData()
        {
            List<string> fileNames = new List<string>();
            fileNames = await Task.Run(() => fileListProxy.FetchNewFileList());

            Dispatcher.Invoke(() =>
            {
                filesView.ItemsSource = fileNames;
            });
                
        }
         
        private async void UpdateLobbyData() //async implementation of updating the lobby data
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await FetchandUpdateLobbyData();
                    await Task.Delay(TimeSpan.FromSeconds(1)); //every 5 seconds
                }
            });
        }
        private async Task FetchandUpdateLobbyData()
        {
            try
            {
                List<string> roomNames = null;
                List<uint> userCounts = null;
                List<string> users = null;
                await Task.Run(() => lobbyInterface.FetchRoomData(out roomNames, out userCounts, out users)); //fetches the lobby data as a task
                Dispatcher.Invoke(() =>
                {
                    LobbyListView.ItemsSource = roomNames; //sets the listview content
                    activeUsersView.ItemsSource = users; //sets the user listview content
                });
            }
            catch (Exception ex) {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });

            }
        }
      
        

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Set errorbox to hidden by default
                ErrorBox.Visibility = Visibility.Collapsed;

                //try joining with username
                lobbyInterface.JoinLobby(usernameBox.Text);
                UpdateLobbyData();

                //Create the message server factory 
                

                //collapse login screen and make main window visible
                loginScreen.Visibility = Visibility.Collapsed;
                mainScreen.Visibility = Visibility.Visible;
                userView.Text = usernameBox.Text;
            }
            catch (FaultException<UnauthorisedUserFault> ex)
            {
                ErrorBox.Text = ex.Message + "Please try again.";
            }
            catch (Exception ex) 
            { 
                ErrorBox.Text = "Please try again.";
            }
        }

        private void logoutButton_Click(Object sender, RoutedEventArgs e)
        {
            //leave lobby
            lobbyInterface.LeaveLobby();
            //collapse main window and make login screen visible
            mainScreen.Visibility = Visibility.Collapsed;
            loginScreen.Visibility = Visibility.Visible;



        }
        // Listview click
        private async void LobbyListView_MouseDoubleClick(object sender, MouseEventArgs e) //Implementation of the double click listview listener
        {

            if(LobbyListView.SelectedItem != null)
            {
                //Room selection
               string userName = lobbyInterface.Username; //Lobby interface method to return username
               string roomName = LobbyListView.SelectedItem.ToString();
               await JoinMessageServerAsync(roomName, userName);
               await JoinFileServerAsync(roomName, userName);
               updateFileData();


            }
        }
        private async Task JoinMessageServerAsync(string roomName, string userName) //async join - prevent ui freeze if any
        {
            await Task.Run(() => { 
                messageProxy = new MessageProxy(userName, roomName);

            }); 
            
        }
        private async Task JoinFileServerAsync(string roomName, string userName) //async join - prevent ui freeze if any
        {
            await Task.Run(() => {
                fileListProxy = new FileListProxy(userName, roomName, this);

            });

        }

        private void newLobbyButton_Click(Object sender, RoutedEventArgs e)
        {
            try
            {

                //make lobby textbox visible
                newLobbyButton.Visibility = Visibility.Collapsed;
                NewLobbyOption.Visibility= Visibility.Visible;
            }
            catch (Exception) { }
        }

        private void newLobbyOption_Click(Object sender, RoutedEventArgs e)
        {
            try
            {

                //add new room
                lobbyInterface.MakeRoom(lobbyNameBox.Text);

                //hide lobby textbox
                newLobbyButton.Visibility = Visibility.Visible;
                NewLobbyOption.Visibility = Visibility.Hidden;
                
            }
            catch (Exception) {
            }
        }

        private void sendMsg_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                if (roomFile != null && !string.IsNullOrEmpty(roomFile.name))
                {
                    fileListProxy.AddFile(roomFile);
                }
                else
                {
                    Console.WriteLine("Could not retrieve valid RoomFile or file name.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exceptionss");
                
                
            }
        }


        private void attachMsg_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                openFileDialog.Filter = "All files (*.*)|*.*";
                if(openFileDialog.ShowDialog()== true)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    
                    roomFile = createFileItem(selectedFilePath);
                   
                    Console.WriteLine(roomFile.name + "--  ex " + roomFile.extension + " -- file" + roomFile.file);



                }
            }
            catch (Exception) { }
        }

        private void lobbyNameGo_Click(object sender, RoutedEventArgs e)
        {

        }
        private RoomFile createFileItem(string filePath)
        {
            
            string fileName = System.IO.Path.GetFileName(filePath);
            string extension = fileName.Length > 3 ? fileName.Substring(fileName.Length - 4).ToLower() : "";


            if (extension == ".png" || extension == ".jpg" || extension == ".bmp" || extension == ".gif")
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
            else if(extension == ".txt")
            {
                string text = File.ReadAllText(filePath);
                TextFileItem fileItem =  new TextFileItem
                {
                    fileName = fileName,
                    TextContent = text
                };
                return new RoomFile(fileName, extension,null , fileItem);
            }
            else
            {
                MessageBox.Show("Unsupported file Type");
                return null;
            }


        }

    }

}
