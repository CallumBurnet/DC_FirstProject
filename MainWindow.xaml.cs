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

                //try joining with username and start updating lobby data
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
            // Leave existing room by leaving the message proxy and file proxy
            messageProxy?.Leave();
            messageProxy = null;
            fileListProxy?.Leave();
            fileListProxy = null;

            // Leave lobby and stop updating lobby data by setting logged in as false which stops loop
            loggedIn = false;
            lobbyInterface.LeaveLobby();
            LobbyListView.SelectedItem = null;
            usernameBox.Text = "";

            // Collapse main window and make login screen visible, disable the send ui for when user logs back in
            disableSendUI(true);
            mainScreen.Visibility = Visibility.Collapsed;
            loginScreen.Visibility = Visibility.Visible;
        }

        // Listview click
        private async void LobbyListView_MouseDoubleClick(object sender, MouseEventArgs e) //Implementation of the double click listview listener
        {

            if(LobbyListView.SelectedItem != null)
            {
                //remove private pop up if any
                privateUserTo = "";
                PrivateMessageToggle(false);

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
            string selectedFileName = filesView.SelectedItem.ToString();

            fileListProxy.StartDownload(selectedFileName);
        }

        private void newLobbyButton_Click(Object sender, RoutedEventArgs e)
        {
            // Make lobby textbox visible when new lobby button is clicked
            newLobbyButton.Visibility = Visibility.Collapsed;
            lobbyNameBox.Text = "";
            NewLobbyOption.Visibility = Visibility.Visible;
        }

        private void newLobbyOption_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                //add new room and update list view
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

        //checks if user has double clicked on a player name, if so message will be sent as private, if not public
        private void sendMsg_Click(Object sender, RoutedEventArgs e)
        {
            string tempUser = privateUserTo; //for error message if player has left the room
            PrivateMessageToggle(false); //set private message pop up as closed by default
            try
            {
                //if private user var is not empty proceed with making pm pop up visible
                if (!privateUserTo.Equals(""))
                {
                    PrivateMessageToggle(true);
                }

                //send message, proxy will send as private if privateUserTo var is not empty
                messageProxy.SendMessage(messageBox.Text, privateUserTo);

                //reset ui so pop up is closed and message box is empty
                PrivateMessageToggle(false);
                messageBox.Text = "";
            }
            catch (FaultException<UserNotFoundFault>)
            {
                //if user is no longer in the room, message proxy will throw fault and pop up will show error
                PrivateMessagePopUpText.Dispatcher.BeginInvoke(new Action(() => { PrivateMessagePopUpText.Text = tempUser + " is not in the room. Message not sent."; }));
            }
            catch (Exception ex) 
            {
                //catch any other errors and show as pop up
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });
            }

            //reset private user var back to empty
            privateUserTo = "";
        }

        //when user selected in active user list, private user var will be set as user selected and private message pop up will become visible
        private void UserView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //only proceed is selected user isn't the user itself, otherwise set the var back to empty
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
            try
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
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });
            }
        }

        //disables the send ui when user not in a room and displays welcome messsage to prompt user to select a room
        private void disableSendUI(Boolean option)
        {
            if (option)
            {
                roomNameBox.Dispatcher.BeginInvoke(new Action(() => { roomNameBox.Text = "Welcome. Please select or create a lobby to join."; }));
            }
            sendMsgButton.Dispatcher.BeginInvoke(new Action(() => { sendMsgButton.IsEnabled = !option; }));
            sendFileButton.Dispatcher.BeginInvoke(new Action(() => { sendFileButton.IsEnabled = !option; }));
            messageBox.Dispatcher.BeginInvoke(new Action(() => {  messageBox.IsEnabled = !option; }));
        }

        //cancels sending a private message when cancel button pressed
        private void CancelPrivateButton_Click(object sender, RoutedEventArgs e)
        {
            PrivateMessageToggle(false);
            privateUserTo = "";
        }

        //controls visibility of the private message pop up box
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

        //clears message box when double clicked
        private void messageBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            messageBox.Text = "";
        }
    }
}
