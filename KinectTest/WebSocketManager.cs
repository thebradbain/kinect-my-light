using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace KinectTest
{
    class WebSockeManager
    {
        static WebSockeManager webSocketManager = new WebSockeManager();
        private WebSockeManager() { this.clients = new List<TcpClient>(); }
        public static WebSockeManager getInstance()
        {
            return webSocketManager;
        }

        private List<TcpClient> clients;
        
        public void connectClients(int port)
        {
 	        TcpListener serverSocket = new TcpListener(IPAddress.Parse("192.168.1.33"), port); //listen for web clients at this address and port
            serverSocket.Start();

            while (true)
            {
                try
                {
                    TcpClient clientSocket = serverSocket.AcceptTcpClient();
                    NetworkStream clientStream = clientSocket.GetStream();
                    StreamReader sr = new StreamReader(clientStream);
                    StreamWriter sw = new StreamWriter(clientStream);

                    EstablishHandshake(sr, sw);
                    sendString(clientSocket, KinectManager.Result.ToString()); //make sure its updated on first launch

                    Thread clientHandler = new Thread(new ThreadStart(new WebClientHandler(clientSocket).run));
                    clientHandler.Start(); //handles all messages sent from the clients. Each client is on a seperate thread.

                    clients.Add(clientSocket);
                    Console.WriteLine("Client coneccted!");
                }
                catch
                {
                    Console.WriteLine("Unable to connect web client.");
                }
                

            }
        }

        public void EstablishHandshake(StreamReader reader, StreamWriter writer) //credit to some post on StackExchange
        {

            string line = null, key = "", responseKey = "";
            string MAGIC_STRING = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            while (line != "")
            {
                try
                {
                    line = reader.ReadLine();
                    if (line.StartsWith("Sec-WebSocket-Key:"))
                    {
                        key = line.Split(':')[1].Trim();
                    }
                }
                catch { line = ""; }
            }

            if (key != "")
            {
                key += MAGIC_STRING;
                using (var sha1 = SHA1.Create())
                {
                    responseKey = Convert.ToBase64String(sha1.ComputeHash(Encoding.ASCII.GetBytes(key)));
                }
            }
            else
                return;

            // send handshake to the client
            writer.WriteLine("HTTP/1.1 101 Web Socket Protocol Handshake");
            writer.WriteLine("Upgrade: WebSocket");
            writer.WriteLine("Connection: Upgrade");
            writer.WriteLine("WebSocket-Origin: http://192.168.1.33");
            writer.WriteLine("WebSocket-Location: ws://localhost:5556/websession");
            if (!String.IsNullOrEmpty(responseKey))
                writer.WriteLine("Sec-WebSocket-Accept: " + responseKey);
            writer.WriteLine("");

            writer.Flush();


            Console.WriteLine("Finished Handshake");
        }

        public void sendMessage(byte message) //for every client in clients, start a new thread and send out a message
        {
            foreach(TcpClient client in this.clients) {
                Thread clientThread = new Thread(new ThreadStart(() => sendString(client, message.ToString()))); //start a new thread
                clientThread.Start();
            }
        }

        private void sendString(TcpClient webClient, string str)
        {
            try {
                NetworkStream clientStream = webClient.GetStream(); 
                var buf = Encoding.UTF8.GetBytes(str);
                int frameSize = 64;

                var parts = buf.Select((b, i) => new { b, i })
                               .GroupBy(x => x.i / (frameSize - 1))
                               .Select(x => x.Select(y => y.b).ToArray())
                               .ToList();

                for (int i = 0; i < parts.Count; i++)
                {
                    byte cmd = 0;
                    if (i == 0) cmd |= 1;
                    if (i == parts.Count - 1) cmd |= 0x80;

                    clientStream.WriteByte(cmd);
                    clientStream.WriteByte((byte)parts[i].Length);
                    clientStream.Write(parts[i], 0, parts[i].Length);

                }
                clientStream.Flush();
            }
            catch {
                Console.WriteLine("Could not connect to web client. Removing client"); //if it can't write to the stream, it must have disconnected.
                clients.Remove(webClient);
                webClient.Close();
            }
        }

        class WebClientHandler //handles received messages from each client
        {
            TcpClient webClient;
            NetworkStream clientStream;
            
            public WebClientHandler(TcpClient webClient)
            {
                this.webClient = webClient;
                this.clientStream = this.webClient.GetStream();
            }

            public void run() //reads from the client's stream and does with it accordingly
            {
                try
                {
                    //TODO: Read from client stream
                    //and then do something with the response... 
                }
                catch
                {
                    Console.WriteLine("Unable to read message. Terminating thread.");
                    clientStream.Close();
                    webClient.Close();
                }
            }
        }
    }

}
