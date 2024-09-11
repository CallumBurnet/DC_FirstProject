using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using LobbyDLL;

namespace LobbyServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server starting...");

            ServiceHost host;
            ServiceHost messageHost;
            ServiceHost fileHost;
            NetTcpBinding tcp = new NetTcpBinding();

            host = new ServiceHost(typeof(LobbyServer));
            messageHost = new ServiceHost(typeof(MessageServer));
            fileHost = new ServiceHost(typeof(FileServer));

            host.AddServiceEndpoint(typeof(ILobbyServer), tcp, "net.tcp://localhost:8100/MKXLobby");
            messageHost.AddServiceEndpoint(typeof(IMessageServer), tcp, "net.tcp://localhost:8100/message");
            fileHost.AddServiceEndpoint(typeof(IFileServer), tcp, "net.tcp://localhost:8100/file");


            host.Open();
            messageHost.Open();
            fileHost.Open();

            Console.WriteLine("Server Online.");
            Console.ReadLine();
            host.Close();
            messageHost.Close();
            fileHost.Close();



        }
    }
}
