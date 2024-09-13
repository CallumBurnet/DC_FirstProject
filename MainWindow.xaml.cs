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
using System.ServiceModel;

using LobbyDLL;
using LobbyClient;


namespace LobbyCLient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Declare interface variables for file, lobby and room servers.
        private ILobbyServer lobbyInterface;
        private MessageProxy messageProxy;
        private FileListProxy fileListProxy;
        DownloadWindow downloadWindow;
        private Boolean loggedIn;
        private string privateUserTo;

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

            //Create a listener for the double click on a room or room items
            LobbyListView.MouseDoubleClick += LobbyListView_MouseDoubleClick;
            activeUsersView.MouseDoubleClick += UserView_MouseDoubleClick;
            filesView.MouseDoubleClick += filesView_MouseDoubleClick;

            // Reset message text box status
            disableSendUI(true);
            privateUserTo = "";
        }

        private async void UpdateLobbyData()  // Thread that periodically updates the full room list
        {
            loggedIn = true;
            await Task.Run(async () =>
            {
                while (loggedIn)
                {
                    await FetchandUpdateLobbyData();
                    await Task.Delay(TimeSpan.FromSeconds(1)); //every 1 seconds
                }
            });
        }

        private async Task FetchandUpdateLobbyData()
        {
            try
            {
                List<string> roomNames = null;
                List<uint> userCounts = null;
                await Task.Run(() => lobbyInterface.FetchRoomData(out roomNames, out userCounts)); //fetches the lobby data as a task
                Dispatcher.Invoke(() => LobbyListView.ItemsSource = roomNames);
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

                //collapse login screen and make main window visible
                loginScreen.Visibility = Visibility.Collapsed;
                mainScreen.Visibility = Visibility.Visible;
                userView.Text = usernameBox.Text;
            }
            catch (FaultException<UnauthorisedUserFault> ex)
            {
                ErrorBox.Visibility = Visibility.Visible;
                ErrorBox.Text = ex.Message + " Please try again.";
            }
            catch (Exception ex) 
            { 
                ErrorBox.Visibility = Visibility.Visible;
                ErrorBox.Text = " Please try again." + ex.Message;
            }
        }

        private void logoutButton_Click(Object sender, RoutedEventArgs e)
        {
            // Leave existing room
            messageProxy?.Leave();
            messageProxy = null;
            fileListProxy?.Leave();
            fileListProxy = null;

            // Leave lobby
            loggedIn = false;
            disableSendUI(true);
            lobbyInterface.LeaveLobby();
            LobbyListView.SelectedItem = null;
            usernameBox.Text = "";

            // Collapse main window and make login screen visible
            mainScreen.Visibility = Visibility.Collapsed;
            loginScreen.Visibility = Visibility.Visible;
        }

        // Listview click
        private async void LobbyListView_MouseDoubleClick(object sender, MouseEventArgs e) //Implementation of the double click listview listener
        {

            if(LobbyListView.SelectedItem != null)
            {
                // Room selection
                string userName = lobbyInterface.Username; // Lobby interface method to return username
                string roomName = LobbyListView.SelectedItem.ToString();
                roomNameBox.Text = roomName;  // Update room name label

                // Join one at a time to avoid race conditions (should be fixed anyways once server locks are added)
                await Task.Run(() => {
                    messageProxy?.Leave();
                    messageProxy = new MessageProxy(userName, roomName, this);
                }); ;
                await Task.Run(() => {
                    fileListProxy?.Leave();
                    fileListProxy = new FileListProxy(userName, roomName, this);
                });
            }
            disableSendUI(false);
        }

        private void filesView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (filesView.SelectedItem != null)
            {
                string selectedFileName = filesView.SelectedItem.ToString();
                downloadWindow = fileListProxy.DownloadWindow;
                downloadWindow.StartDownload(selectedFileName);
                downloadWindow.Show();
            }
        }

        private void newLobbyButton_Click(Object sender, RoutedEventArgs e)
        {
            // Make lobby textbox visible
            newLobbyButton.Visibility = Visibility.Collapsed;
            lobbyNameBox.Text = "";
            NewLobbyOption.Visibility = Visibility.Visible;
        }

        private void newLobbyOption_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                //add new room
                lobbyInterface.MakeRoom(lobbyNameBox.Text);
                List<string> roomNames;
                List<uint> userCount;
                lobbyInterface.FetchRoomData(out roomNames, out userCount);
                LobbyListView.ItemsSource = roomNames;

                //hide lobby textbox
                newLobbyButton.Visibility = Visibility.Visible;
                NewLobbyOption.Visibility = Visibility.Hidden;
            }
            catch (Exception eee) 
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(eee.Message);
                });
            }
        }

        private void sendMsg_Click(Object sender, RoutedEventArgs e)
        {
            string tempUser = privateUserTo;
            PrivateMessageToggle(false);
            try
            {
                if (!privateUserTo.Equals(""))
                {
                    PrivateMessageToggle(true);
                }
                messageProxy.SendMessage(messageBox.Text, privateUserTo);
                PrivateMessageToggle(false);
                messageBox.Text = "";
            }
            catch (FaultException<UserNotFoundFault>)
            {
                PrivateMessagePopUpText.Dispatcher.BeginInvoke(new Action(() => { PrivateMessagePopUpText.Text = tempUser + " is not in the room. Message not sent."; }));
            }
            catch (Exception) { }
            privateUserTo = "";
        }

        private void UserView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!activeUsersView.SelectedItem.ToString().Equals(usernameBox.Text))
            {
                privateUserTo = activeUsersView.SelectedItem.ToString();
                PrivateMessageToggle(true);
            }
            else
            {
                privateUserTo = "";
                PrivateMessageToggle(false);
            }
        }


        private void sendFile_Click(Object sender, RoutedEventArgs e)
        {
            if (fileListProxy == null)
            {
                MessageBox.Show("Please join a room before sending a file.");
            }
            else
            {
                fileListProxy.UploadFile();
            }
        }

        private void disableSendUI(Boolean option)
        {
            if (option)
            {
                roomNameBox.Dispatcher.BeginInvoke(new Action(() => { roomNameBox.Text = "Welcome. Please select lobby to join."; }));
            }
            sendMsgButton.Dispatcher.BeginInvoke(new Action(() => { sendMsgButton.IsEnabled = !option; }));
            sendFileButton.Dispatcher.BeginInvoke(new Action(() => { sendFileButton.IsEnabled = !option; }));
            messageBox.Dispatcher.BeginInvoke(new Action(() => {  messageBox.IsEnabled = !option; }));
        }

        private void CancelPrivateButton_Click(object sender, RoutedEventArgs e)
        {
            PrivateMessageToggle(false);
            privateUserTo = "";
        }

        private void PrivateMessageToggle(Boolean option)
        {
            if (option)
            {
                PrivateMessagePopUp.Dispatcher.BeginInvoke(new Action(() => { PrivateMessagePopUp.Visibility = Visibility.Visible; }));
                PrivateMessagePopUpText.Dispatcher.BeginInvoke(new Action(() => { PrivateMessagePopUpText.Text = "Sending private message to @" + privateUserTo; }));
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => { PrivateMessagePopUp.Visibility = Visibility.Collapsed; }));
            }
        }

        private void messageBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            messageBox.Text = "";
        }
    }
}
