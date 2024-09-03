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
        private IRoomServer roomInterface;
        public MainWindow()
        {
            InitializeComponent();

            //Declare the channel factories for file, lobby and room channels.
            ChannelFactory<IFileServer> fileFactory;
            ChannelFactory<ILobbyServer> lobbyFactory;
            ChannelFactory<IRoomServer> roomFactory;

            //Set up the connection
            NetTcpBinding tcp = new NetTcpBinding();
            string URL = "net.tcp://localhost:8100/MKXLobby";

            //Initialise the file, lobby and room factories.
            //Lobby and room to be duplex as goes both ways.
            fileFactory = new ChannelFactory<IFileServer>(tcp, URL);
            lobbyFactory = new DuplexChannelFactory<ILobbyServer>(tcp, URL);
            roomFactory = new DuplexChannelFactory<IRoomServer>(tcp, URL);

            //Create the factory channels.
            fileInterface = fileFactory.CreateChannel();
            lobbyInterface = lobbyFactory.CreateChannel();
            roomInterface = roomFactory.CreateChannel();



        }
    }
}
