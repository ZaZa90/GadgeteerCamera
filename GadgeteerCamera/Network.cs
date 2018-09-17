using System;
using Microsoft.SPOT;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using System.Threading;
namespace GadgeteerCamera
{
    class Network
    {

        //WIFI module
        Gadgeteer.Modules.GHIElectronics.WiFiRS21 wifiRS21;

        //Network info
        private static string SSID = "Robocar_WiFi";
        private static string PASSWORD = "robocar2018";
        private static string ip = "0.0.0.0";
        private static string dns = "0.0.0.0";
        //private static string dns2 = "0.0.0.0";

        public void connectWiFiLoop()
        {
            if (wifiRS21.NetworkInterface.Opened)
                wifiRS21.NetworkInterface.Close();
            if (!wifiRS21.NetworkInterface.Opened)
                wifiRS21.NetworkInterface.Open();
            if (!wifiRS21.NetworkInterface.IsDhcpEnabled)
                wifiRS21.NetworkInterface.EnableDhcp();
            if (!wifiRS21.NetworkInterface.IsDynamicDnsEnabled)
                wifiRS21.NetworkInterface.EnableDynamicDns();

            Debug.Print("[NETWORK] Connecting to: " + SSID);
            while (!wifiRS21.IsNetworkConnected)
            {
                try
                {
                    wifiRS21.NetworkInterface.Join(SSID, PASSWORD);
                }
                catch(Exception e) { Debug.Print(e.Message); }
            }
        }

        public void initWIFI(Gadgeteer.Modules.GHIElectronics.WiFiRS21 wifiRS21)
        {

            this.wifiRS21 = wifiRS21;
            
        }

        public void ConnectWIFI(){
            Debug.Print("[NETWORK] network up");
            Debug.Print("[NETWORK] contacting DHCP");

            int esc = 0;
            while (wifiRS21.NetworkInterface.IPAddress == "0.0.0.0")
            {
                //Thread.Sleep(200);
                TimeSpan difference;
                DateTime timeNow;
                DateTime timeStart = DateTime.Now;
                do
                {
                    timeNow = DateTime.Now;
                    difference = (timeNow - timeStart);
                }
                while (difference.Milliseconds >= 200);
                if (++esc > 50)
                    break;
            }
            if (esc < 50)
            {

                ip = wifiRS21.NetworkInterface.IPAddress;
                dns = wifiRS21.NetworkInterface.DnsAddresses[0];
                //dns2 = wifiRS21.NetworkInterface.DnsAddresses[1];
                Debug.Print("[NETWORK] IP Address: " + ip);
                Debug.Print("[NETWORK] DNS: " + dns);
               // Debug.Print("DNS2: " + dns2);
                Debug.Print("[NETWORK] Connected");
                 
            }
            else
            {
                Debug.Print("[NETWORK] Connection failed");
                Thread.Sleep(5000);
                ConnectWIFI();
            }
        }

        public void resetIp()
        {
            ip = "0.0.0.0";
            dns = "0.0.0.0";
        }

        public string getIp()
        {
            return ip;
        }

    }
}
