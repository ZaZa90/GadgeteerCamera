using System;
using Microsoft.SPOT;
using Gadgeteer.Networking;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Xml;

namespace GadgeteerCamera
{
    class Client
    {

        private const string serverIp = "192.168.0.100";
        private const string serverPort = "8080";
        private const string serverAddress = "http://" + serverIp + ":" + serverPort + "/Service.svc/";
        private Byte[] pictureBytes;
        private Gadgeteer.Modules.GHIElectronics.MulticolorLED multicolorLED2;
        private static Boolean processing;
        private static Boolean pictureRecognized;
        private static Operation currentOperation;

        public Boolean isProcessing(){ return processing; }
        public Boolean isRecognized(){ return pictureRecognized; }
        public void setProcessing(Boolean b) { processing = b; }
        public Operation getOperation() { return currentOperation; }


        public Client(Gadgeteer.Modules.GHIElectronics.MulticolorLED multicolorLED2)
        {
            this.multicolorLED2 = multicolorLED2;
        }


        public void SendIpHTTP(string ip)
        {
            setProcessing(true);
            POSTContent emptyPost = new POSTContent();
            var req = HttpHelper.CreateHttpPostRequest(serverAddress + "ip/" + ip, emptyPost, null);
            req.ResponseReceived += new HttpRequest.ResponseHandler(req_ResponseReceived);
            req.SendRequest();
            Debug.Print("[CLIENT] " + serverAddress + "ip/" + ip);
        }

        public void RecvOperation()
        {
            setProcessing(true);
            GETContent emptyGet = new GETContent();
            var req = HttpHelper.CreateHttpGetRequest(serverAddress + "operation", emptyGet);
            req.ResponseReceived += new HttpRequest.ResponseHandler(req_ResponseReceived_Operation);
            req.SendRequest();
            Debug.Print("[CLIENT] " + serverAddress + "operation");
        }

        private void req_ResponseReceived_Operation(HttpRequest sender, HttpResponse response)
        {
            Debug.Print("[SERVER] " + response.StatusCode + "," + response.Text);
            if (response.StatusCode == "200")
            {   
                
                String operation = (String)GetXmlElement(response.Text.ToString());   
                String angle = operation.Substring(1);
                Debug.Print("[SERVER] " + operation);
                
                if(operation.Equals("ERROR"))
                    currentOperation = Operation.ERROR;
                else if (operation[0].Equals("F"))
                    currentOperation = Operation.FORWARD;
                else if (operation[0].Equals("L"))
                    currentOperation = Operation.LEFT;
                else if (operation.Equals("NULL"))
                    currentOperation = Operation.NULL;
                else if (operation.Equals("PICTURE"))
                    currentOperation = Operation.PICTURE;
                else if (operation[0].Equals("R"))
                    currentOperation = Operation.RIGHT;
                else if (operation.Equals("STOP"))
                    currentOperation = Operation.STOP;
                else if (operation.Equals("FORWARD2"))
                    currentOperation = Operation.FORWARD2;
                else if (operation.Equals("B"))
                    currentOperation = Operation.BACKWARD2;
                else if (operation.Equals("RIGHT2"))
                    currentOperation = Operation.RIGHT2;
                else if (operation.Equals("LEFT2"))
                    currentOperation = Operation.LEFT2;

            }
            setProcessing(false);
        }


       /* public void SendPictureHTTP(string part, string totParts, string pictureCode)
        {

            string xml = "<string xmlns=" + '"' + "http://schemas.microsoft.com/2003/10/Serialization/" + '"' +
                ">" + pictureCode + "</string>";
            POSTContent postContent = POSTContent.CreateTextBasedContent(xml);

            var req =
                HttpHelper.CreateHttpPostRequest(serverAddress + "picturePart/" + part + "/" + totParts, postContent, "application/xml");
            req.SendRequest();
        }*/

        private void req_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            Debug.Print("[SERVER] " + response.StatusCode + "," + response.Text);
            setProcessing(false);
        }

        
        public void sendPictureTCP(byte[] pictureBytes)
        {

            this.setProcessing(true);
            pictureRecognized = false;

            this.pictureBytes = pictureBytes;
            POSTContent postContent = new POSTContent();
            var req = HttpHelper.CreateHttpPostRequest(
                serverAddress + "picture/TCP/" + pictureBytes.Length.ToString(), postContent, null);
            req.ResponseReceived += new HttpRequest.ResponseHandler(req_ResponseReceived_picture);
            req.SendRequest();
            Debug.Print("[CLIENT] " + serverAddress + "picture/TCP/" + pictureBytes.Length.ToString());
        }


        private void req_ResponseReceived_picture(HttpRequest sender, HttpResponse response)
        {

            Debug.Print("[SERVER] " + response.StatusCode.ToString());
            if (response.StatusCode == "200")
            {

                int serverTcpPort = Int32.Parse((String)GetXmlElement(response.Text.ToString()));

                Debug.Print("[SERVER] " + response.Text.ToString());
                Debug.Print("[SERVER] use port " + serverTcpPort);
                Debug.Print("[CLIENT] sending picture via TCP");


                Socket socket = ConnectedSocket(serverTcpPort);
                int current = 0;
                int tcpBufSize = 4096;
                int rcv;
                while (current < pictureBytes.Length)
                {
                    if ((current + tcpBufSize) > pictureBytes.Length)
                        tcpBufSize = pictureBytes.Length - current;

                    rcv = socket.Send(pictureBytes, current, tcpBufSize, SocketFlags.None);
                    current += rcv;
                    //Debug.Print("[CLIENT] " + current + "/" + pictureBytes.Length);
                }
                
                Debug.Print("[CLIENT] picture sent");

                Byte[] resp = new Byte[sizeof(UInt32)];
                socket.Receive(resp);
                int res = BitConverter.ToInt32(resp,0);

                if (res == 1)
                {
                    Debug.Print("[SERVER] picture recognized");
                    multicolorLED2.TurnGreen();
                    pictureRecognized = true;
                }
                else
                {
                    Debug.Print("[SERVER] picture not recognized");
                    multicolorLED2.TurnRed();
                    pictureRecognized = false;
                }
                socket.Close();
                
            }
            else
            {
                multicolorLED2.TurnRed();
            }
            setProcessing(false);
        }


        public Socket ConnectedSocket(int serverTcpPort)
        {
            int valid = 0;
            Socket socket = null;
            while (valid == 0)
            {
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(new IPEndPoint(IPAddress.Parse(serverIp), serverTcpPort));
                    valid = 1;
                }
                catch (SocketException e) {  valid = 0; }
                catch (Exception e) {  valid = 0; }
            }
            return socket;
        }

        public Object GetXmlElement(string xml){
            Object value=null ;
            byte[] bytes = Encoding.UTF8.GetBytes(xml);
                MemoryStream rms = new MemoryStream(bytes);
                XmlReaderSettings ss = new XmlReaderSettings();
                ss.IgnoreWhitespace = true;
                ss.IgnoreComments = false;
                //XmlException.XmlExceptionErrorCode.
                XmlReader xmlr = XmlReader.Create(rms,ss);
                xmlr.Read();

                while (!xmlr.EOF)
                {
                    xmlr.Read();
                    switch (xmlr.NodeType)
                    {
                        case XmlNodeType.Element:
                            //Debug.Print("element: " + xmlr.Name);
                            break;
                        case XmlNodeType.Text:
                           // Debug.Print("text: " + xmlr.Value);
                            value = xmlr.Value;
                            break;
                        case XmlNodeType.XmlDeclaration:
                            //Debug.Print("decl: " + xmlr.Name + ", " + xmlr.Value);
                            break;
                        case XmlNodeType.Comment:
                           // Debug.Print("comment " + xmlr.Value);
                            break;
                        case XmlNodeType.EndElement:
                            //Debug.Print("end element");
                            break;
                        case XmlNodeType.Whitespace:
                            //Debug.Print("white space");
                            break;
                        case XmlNodeType.None:
                            //Debug.Print("none");
                            break;
                        default:
                            //Debug.Print(xmlr.NodeType.ToString());
                            break;
                    }
                }
           return value;
        }
    }
}
