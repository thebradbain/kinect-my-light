using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;
using System.Threading;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
using System.IO;
using System.IO.Ports;

namespace KinectTest
{
    public partial class Form1 : Form
    {
        KinectManager kinect;
        public Form1()
        {



            KinectManager.getInstance().run();

            Console.WriteLine("Awaiting connection...");
            Thread clientConnectionThread = new Thread(() => TcpSocketManager.getInstance().connectClients(5555));
            clientConnectionThread.Name = "TCP Connection Thread";
            clientConnectionThread.IsBackground = true;

            Thread webClientConnectionThread = new Thread(() => WebSockeManager.getInstance().connectClients(5556));
            webClientConnectionThread.Name = "WebClient Connection Thread";
            clientConnectionThread.IsBackground = true;

            clientConnectionThread.Start();
            webClientConnectionThread.Start();





        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}
