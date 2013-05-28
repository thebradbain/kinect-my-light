using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KinectTest
{
    interface ISocketManager
    {
        private List<TcpClient> clients;

        public static void getInstance;
        public void connectClients;
        public void sendMessage;
    }
}
