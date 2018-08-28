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
        private static string SSID = "Telecom-56888096";
        private static string PASSWORD = "Y9jc8D9FqXIhpnAj1mSs3uuz";
        private static string ip = "0.0.0.0";
        private static string dns = "0.0.0.0";
        private static string dns2 = "0.0.0.0";

        public void initWIFI(Gadgeteer.Modules.GHIElectronics.WiFiRS21 wifiRS21)
        {

            this.wifiRS21 = wifiRS21;

            if (wifiRS21.NetworkInterface.Opened)
                wifiRS21.NetworkInterface.Close();
            if (!wifiRS21.NetworkInterface.Opened)
                wifiRS21.NetworkInterface.Open();
            if (!wifiRS21.NetworkInterface.IsDhcpEnabled)
                wifiRS21.NetworkInterface.EnableDhcp();
            if (!wifiRS21.NetworkInterface.IsDynamicDnsEnabled)
                wifiRS21.NetworkInterface.EnableDynamicDns();

            Debug.Print("[NETWORK] Connecting to: " + SSID);
            wifiRS21.NetworkInterface.Join(SSID, PASSWORD);
        }

        public void ConnectWIFI(){
            Debug.Print("[NETWORK] network up");
            Debug.Print("[NETWORK] contacting DHCP");

            int esc = 0;
            while (wifiRS21.NetworkInterface.IPAddress == "0.0.0.0")
            {
                Thread.Sleep(200);
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
            }
        }

        public string getIp()
        {
            return ip;
        }

    }
}
