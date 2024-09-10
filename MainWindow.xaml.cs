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
        private IFileServer fileInterface;
        private ILobbyServer lobbyInterface;
        private MessageProxy messageProxy;
        private Boolean loggedIn;

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

            disableSendUI(true);
        }

        private async void UpdateLobbyData()  // Thread that periodically updates the full room list
        {
            loggedIn = true;
            await Task.Run(async () =>
            {
                while (loggedIn)
                {
                    await FetchandUpdateLobbyData();
                    await Task.Delay(TimeSpan.FromSeconds(5)); //every 5 seconds
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
                ErrorBox.Text = ex.Message + "Username not valid. Please try again.";
            }
            catch (Exception ee)  // TODO: Should not catch all, instead debug why it fails and add a dedicated catch if reasonable
            { 
                ErrorBox.Visibility = Visibility.Visible;
                ErrorBox.Text = "Please try again." + ee.Message;
            }
        }

        private void logoutButton_Click(Object sender, RoutedEventArgs e)
        {
            // Leave existing room
            messageProxy?.Leave();
            messageProxy = null;

            // Leave lobby
            loggedIn = false;
            disableSendUI(true);
            lobbyInterface.LeaveLobby();
            LobbyListView.SelectedItem = null;

            // Collapse main window and make login screen visible
            mainScreen.Visibility = Visibility.Collapsed;
            loginScreen.Visibility = Visibility.Visible;

            // TODO: Call leave on message and file servers and set them to null locally 
            // TODO: Send cancellation token to the lobby periodic updater
        }

        // Listview click
        private async void LobbyListView_MouseDoubleClick(object sender, MouseEventArgs e) //Implementation of the double click listview listener
        {

            if(LobbyListView.SelectedItem != null)
            {
                UpdateLobbyData();
                // Leave existing room
                messageProxy?.Leave();
                messageProxy = null;  // Let it GC

                // Room selection
                string userName = lobbyInterface.Username; // Lobby interface method to return username
                string roomName = LobbyListView.SelectedItem.ToString();
                await Task.Run(() => {  // Prevent any UI freeze
                    messageProxy = new MessageProxy(userName, roomName, this);
                });
                roomNameBox.Text = LobbyListView.SelectedItem.ToString();  // Update room name label
            }
            disableSendUI(false);
        }

        private void newLobbyButton_Click(Object sender, RoutedEventArgs e)
        {
            // Make lobby textbox visible
            newLobbyButton.Visibility = Visibility.Collapsed;
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
            catch (Exception) {
            }
        }

        private void sendMsg_Click(Object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception) { }
        }

        private void attachMsg_Click(Object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception) { }
        }

        private void lobbyNameGo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chatView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void disableSendUI(Boolean option)
        {
            if (option)
            {
                roomNameBox.Dispatcher.BeginInvoke(new Action(() => { roomNameBox.Text = "Welcome. Please select lobby to join."; }));
            }
            sendMsgButton.Dispatcher.BeginInvoke(new Action(() => { sendMsgButton.IsEnabled = !option; }));
            attachFileButton.Dispatcher.BeginInvoke(new Action(() => { attachFileButton.IsEnabled = !option; }));
            messageBox.Dispatcher.BeginInvoke(new Action(() => {  messageBox.IsEnabled = !option; }));
        }

        private void activeUsersView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

}
