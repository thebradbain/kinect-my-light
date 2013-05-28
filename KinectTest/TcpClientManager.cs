using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KinectTest
{
    class TcpSocketManager
    {
        private static TcpSocketManager server = new TcpSocketManager();
        private TcpSocketManager() : base() { clients = new List<TcpClient>(); }
        public static TcpSocketManager getInstance()
        {
            return server;
        }

        private List<TcpClient> clients;
        public void connectClients(int port) //listens for clients
        {
            

            TcpListener serverSocket = new TcpListener(IPAddress.Parse("192.168.1.33"), port);
            serverSocket.Start();

            

            while (true)
            {
                try
                {
                    TcpClient clientSocket = serverSocket.AcceptTcpClient();
                    clients.Add(clientSocket);

                    Console.WriteLine("Client coneccted!");
                }
                catch
                {
                    Console.WriteLine("Error connecting to TCP client.");
                }
                

            }
        }

        public void sendMessage(byte message)
        {
            foreach (TcpClient client in clients)
            {
                Thread tcpClientThread = new Thread(new ThreadStart(() => sendByte(client, message)));
                tcpClientThread.Start();
            }
 
        }

        private void sendByte(TcpClient client, byte message)
        {
            NetworkStream clientStream = client.GetStream();
            if (client.Connected)
            {
                clientStream.WriteByte(message);
                clientStream.Flush();
            }
            else
            {
                Console.WriteLine("Could not connect to TCP client. Removing from clients...");
                clients.Remove(client);
                clientStream.Close();
                client.Close();
            }


        }

        

    }
}
