using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace HA4IoT.Hardware.Knx
{
    public class KnxController
    {
        //var knxController = new KnxController("192.168.1.123","8150");
        //var lamp = new HA4IoT.Actuators.Lamp(new ComponentId("Lamp1"), knxController.CreateDigitalJoinEndpoint("d1"));
        SocketClient _socketClient;

        public KnxController(string hostName, string portNumber)
        {
            try
            {
                _socketClient = new SocketClient();
                Debug.WriteLine(">>>>>>>>>> knx <<<<<<<<<<");
                string result = _socketClient.Connect(hostName, Convert.ToInt32(portNumber));
                Debug.WriteLine("knx-open socket: " + result);

                CheckPasword();
                Initialisation();

                DigitalJoinToggle("d20");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public void CreateDigitalJoinEndpoint(string digitalJoin)
        {

        }


        public bool CheckPasword()
        {
            string password = "p=" + "" + "\x03";
            string result = _socketClient.Send(password);
            Debug.WriteLine("knx-pasword: " + result);

            if (result == "Success")
            {
                string sReceived = _socketClient.Receive();
                // p=ok\x03 or p=bad\x03
                Debug.WriteLine("knx-pasword answer: " + sReceived);
                if (sReceived == "p=ok\x03")
                {
                    return true;
                }
            }
            return false;

        }

        public void Initialisation()
        {
            string data = "i=1\x03";

            string result = _socketClient.Send(data);
            Debug.WriteLine("knx-init: " + result);

            if (result == "Success")
            {
                string sReceived = _socketClient.Receive();
                Debug.WriteLine("knx-init answer: " + sReceived);
            }
        }

        public void DigitalJoinToggle(string join)
        {
            string data = join + "=1\x03";
            string result = _socketClient.Send(data);
            Debug.WriteLine(result);

            data = join + "=0\x03";
            result = _socketClient.Send(data);
            Debug.WriteLine(result);
        }

        public void DigitalJoinOn(string join)
        {
            string data = join + "=1\x03";
            string result = _socketClient.Send(data);
            Debug.WriteLine(result);
        }

        public void DigitalJoinOff(string join)
        {
            string data = join + "=0\x03";
            string result = _socketClient.Send(data);
            Debug.WriteLine(result);
        }

        public void AnalogJoin(string join, double value)
        {
            if (value < 0)
                value = 0;

            if (value > 65535)
                value = 65535;

            string data = join + "=" + value + "\x03";
            string result = _socketClient.Send(data);
            Debug.WriteLine(result);
        }

        public void SerialJoin(string join, string value)
        {
            string data = join + "=" + value + "\x03";
            string result = _socketClient.Send(data);
            Debug.WriteLine(result);
        }

    }
}
