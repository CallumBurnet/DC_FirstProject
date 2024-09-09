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
        private async void UpdateLobbyData() //async implementation of updating the lobby data
        {
            await Task.Run(async () =>
            {
                while (true)
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

                //get username from usernameBox and try to join lobby.
                /*if (usernameBox.Text.Equals("") || usernameBox.Text.Contains(" "))
                {
                    ErrorBox.Text = "Username not valid. Please try again.";
                    ErrorBox.Visibility = Visibility.Visible;
                }
                else
                {
                    //try joining with username
                    lobbyInterface.JoinLobby(usernameBox.Text);
                    

                    //collapse login screen and make main window visible
                    loginScreen.Visibility = Visibility.Collapsed;
                    mainScreen.Visibility = Visibility.Visible;
                    userView.Text = usernameBox.Text;
                }*/

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

            }
        }
        private Task JoinMessageServerAsync(string roomName, string userName) //async join - prevent ui freeze if any
        {
            return Task.Run(() => { 
                messageProxy = new MessageProxy(userName, roomName);
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
    }

}
