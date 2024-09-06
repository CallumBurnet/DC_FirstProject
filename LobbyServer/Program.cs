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
            NetTcpBinding tcp = new NetTcpBinding();

            host = new ServiceHost(typeof(LobbyServer));

            host.AddServiceEndpoint(typeof(ILobbyServer), tcp, "net.tcp://localhost:8100/MKXLobby");
            host.Open();

            Console.WriteLine("Server Online.");
            Console.ReadLine();
            host.Close();



        }
    }
}
