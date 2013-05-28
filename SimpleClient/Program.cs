using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleClient
{
    class Program
    {
        public static char input;
        static void Main(string[] args)
        {
            Thread inputThread = new Thread(new ThreadStart(inputThreader.run));
            inputThread.IsBackground = true;

            try {
                //SerialManager serialManager = new SerialManager("COM3"); //initialize helper class

                Console.WriteLine("Input DNS");
                string dns = Console.ReadLine(); //allow user to input dns
                using (TcpClient client = new TcpClient(dns, 5555)) //start the client
                {
                    Console.WriteLine("Connected!");
                    inputThread.Start();

                    // Get a client stream for reading. 
                    using (NetworkStream stream = client.GetStream())
                    {
                        while (input != 'q')
                        {
                            int responseData = stream.ReadByte();
                            if (responseData >= 0)
                            {
                                Console.WriteLine("Received: {0}", responseData);
                                //serialManager.writeByte((byte)responseData);
                            }
                        }
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        public class inputThreader
        {
            public static void run()
            {
                    input = Console.ReadKey().KeyChar;
            }
        }
    }
}
