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
            DuplexChannelFactory<IFileServer> fileFactory;
            ChannelFactory<ILobbyServer> lobbyFactory;
            DuplexChannelFactory<IMessageServer> roomFactory;

            //Set up the connection
            NetTcpBinding tcp = new NetTcpBinding();
            string URL = "net.tcp://localhost:8100/MKXLobby";

            //Initialise the file, lobby and room factories.
            //Lobby and room to be duplex as goes both ways.
            fileFactory = new DuplexChannelFactory<IFileServer>(tcp, URL);
            lobbyFactory = new ChannelFactory<ILobbyServer>(tcp, URL);
            roomFactory = new DuplexChannelFactory<IMessageServer>(tcp, URL);

            //Create the factory channels.
            fileInterface = fileFactory.CreateChannel();
            lobbyInterface = lobbyFactory.CreateChannel();
            messageInterface = roomFactory.CreateChannel();

            

            //Set main window as collapsed and login window as visible by default
            mainScreen.Visibility = Visibility.Collapsed;
            loginScreen.Visibility = Visibility.Visible;
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Set errorbox to hidden by default
                ErrorBox.Visibility = Visibility.Collapsed;

                //get username from usernameBox and try to join lobby.
                if (usernameBox.Text.Equals("") || usernameBox.Text.Contains(" "))
                {
                    ErrorBox.Text = "Username not valid. Please try again.";
                    ErrorBox.Visibility = Visibility.Visible;
                }
                else
                {
                    //collapse login screen and make main window visible
                    loginScreen.Visibility = Visibility.Collapsed;
                    mainScreen.Visibility = Visibility.Visible;
                    userView.Text = usernameBox.Text;
                }
                

            }
            catch (Exception ex) 
            { 
                ErrorBox.Text = ex.Message;
            }
        }

        private void logoutButton_Click(Object sender, RoutedEventArgs e)
        {
            //collapse main window and make login screen visible
            mainScreen.Visibility = Visibility.Collapsed;
            loginScreen.Visibility = Visibility.Visible;

        }
    }

}
