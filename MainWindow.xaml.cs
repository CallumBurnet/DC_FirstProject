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
using System.ServiceModel.Security;
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
        private IMessageServer messageInterface;


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
            LobbyListView.MouseDoubleClick += LobbyListView_MouseDoubleClick;
            


        }
        private async void UpdateLobbyData()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await FetchandUpdateLobbyData();
                    await Task.Delay(TimeSpan.FromSeconds(5));
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
                await Task.Run(() => lobbyInterface.FetchRoomData(out roomNames, out userCounts, out users));
                Dispatcher.Invoke(() =>
                {
                    LobbyListView.ItemsSource = roomNames;
                    activeUsersView.ItemsSource = users;
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

                //Setup the message server factory and message endpoint
                string userName = lobbyInterface.getUserName();
                var proxy = new MessageProxy(messageInterface, userName);
                string messageURL = "net.tcp://localhost:8100/message";
                NetTcpBinding binding = new NetTcpBinding();
                ChannelFactory<IMessageServer> messageFactory;
                messageFactory = new DuplexChannelFactory<IMessageServer>(proxy,binding, new EndpointAddress("net.tcp://localhost:8100/message"));
                messageInterface = messageFactory.CreateChannel();


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
        private async void LobbyListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            if(LobbyListView.SelectedItem != null)
            {
               string roomName = LobbyListView.SelectedItem.ToString();
               string userName = lobbyInterface.getUserName();
               await JoinMessageServerAsync(roomName, userName);

            }
        }
        private Task JoinMessageServerAsync(string roomName, string userName) //async join - prevent ui freeze if any
        {
            return Task.Run(() =>
            {
                messageInterface.Join(roomName, userName);
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
