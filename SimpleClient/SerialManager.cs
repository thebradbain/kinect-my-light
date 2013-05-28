using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleClient
{
    class SerialManager
    {
        SerialPort serial;

        public SerialManager(string port)
        {
            this.serial = new SerialPort(port);
            if (!serial.IsOpen)
                serial.Open();
        }

        public void writeByte(byte message)
        {
            serial.Write(new byte[] { message }, 0, 1);
        }
        
    }
}
