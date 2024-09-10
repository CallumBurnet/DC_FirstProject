using LobbyCLient;
using LobbyDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LobbyClient
{
    public class MessageProxy : IRoomServerCallback //message proxy is also the implementation of the message callback
    {
        
        ChannelFactory<IMessageServer> messageFactory;
        private IMessageServer server;
        private string username;
        private string roomName;
        private MainWindow window;
        private List<string> messages;
        CancellationTokenSource cancelTokenSource;  // For threading cleanup
        CancellationToken cancelToken;

        public MessageProxy(string username, string roomName, MainWindow window)
        {
            // Set up message server connection and immediately subscribe
            NetTcpBinding binding = new NetTcpBinding();
            messageFactory = new DuplexChannelFactory<IMessageServer>(this, binding, new EndpointAddress("net.tcp://localhost:8100/message")); //message factory
            server = messageFactory.CreateChannel();
            this.username = username;
            this.roomName = roomName;
            server.Join(roomName, username);
            messages = new List<string>();

            // Also periodically refresh room user list
            cancelTokenSource = new CancellationTokenSource();
            cancelToken = cancelTokenSource.Token;
            this.window = window;
            UpdateUserList();
        }
        public void SendPublic(string message)
        {
            server.SendPublicMessage(message, this.username);
        }
        public void PushMessage(string message)
        {
            messages.Add(message);
            window.chatView.Dispatcher.Invoke(new Action(() => window.chatView.ItemsSource = null));
            window.chatView.Dispatcher.Invoke(new Action(() => window.chatView.ItemsSource = messages));

        }

        public void Leave()
        {
            // Clean up local threads and then unsubscribe
            cancelTokenSource.Cancel();
            server.Leave();
            window.activeUsersView.Dispatcher.Invoke(new Action(() => window.activeUsersView.ItemsSource = null));
            window.chatView.Dispatcher.Invoke(new Action(() => window.chatView.ItemsSource = null));
        }

        private async void UpdateUserList()  // Thread that periodically updates the active users in the current room
        {
            await Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    // Fetch and display users in the current room
                    List<string> users = null;
                    await Task.Run(() => users = server.FetchRoomUsers());
                    window.activeUsersView.Dispatcher.Invoke(new Action(() => window.activeUsersView.ItemsSource = users));

                    await Task.Delay(TimeSpan.FromSeconds(5)); //every 5 seconds
                }
            }, cancelToken);
        }
    }
}
