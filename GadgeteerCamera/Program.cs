using System.Threading;
using Microsoft.SPOT;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using System;
using System.Globalization;

// public enum Operation { STOP, FORWARD, RIGHT, LEFT, PICTURE, NULL, ERROR, FORWARD2, BACKWARD2, RIGHT2, LEFT2 };

namespace GadgeteerCamera
{

    public partial class Program
    {
        
        // threads
        //Thread CheckStopThread;
        //Thread CheckSensorThread;

        //client
        static Client client;

        //Wifi
        Network network;

        //picture
        Picture picture;
        int picNumber;

        //breakout
        BreakOut breakOut;

        //motor
        static Motor motor;

        //directions
        static String currentOperation;

        //flags
        static Boolean isChecking;
        static Boolean isTakingPicture;

        void ProgramStarted()
        {
            
            // Gadgeteer event handlers
            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
            button.ButtonReleased += new Button.ButtonEventHandler(button_ButtonReleased);
            camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);
            wifiRS21.NetworkUp += new GTM.Module.NetworkModule.NetworkEventHandler(wifi_NetworkUp);
            wifiRS21.NetworkDown += new GTM.Module.NetworkModule.NetworkEventHandler(wifi_NetworkDown);

            //client
            client = new Client(multicolorLED2);

            //client event handlers
            client.OnConnectionEnd += c_ConnectionEnd;
            client.OnOperationReceived += c_OperationReceived;
            client.OnPictureAnalyzed += c_PictureAnalyzed;

            //network 
            network = new Network();
            network.initWIFI(wifiRS21);

            //threads
            // CheckStopThread = new Thread(checkStopCondition);
            // CheckSensorThread = new Thread(checkSensor);


            //init operation
            currentOperation = "NULL";

            //leds
            multicolorLED.TurnRed(); //network off
            multicolorLED2.TurnRed(); //no operation executed

            //pictures
            picture = new Picture(multicolorLED2);
            picNumber = 0;

            //breakout
            breakOut = new BreakOut();

            //motor
            motor = new Motor(motorDriverL298, breakOut, multicolorLED2);

            //motor events
            motor.OnStop += m_Stop;
            motor.OnTimeEnd += m_TakePic;

            //flags
            isChecking = false;
            isTakingPicture = false;
        }


        void c_ConnectionEnd(object sender)
        {
            // Car's IP Received Correctely
            threadSendConfig();
            //            string conf = motor.getSlowSpeed().ToString() + "," + motor.getHighSpeed().ToString() + "," + motor.getLimitLines().ToString() + "," + motor.getTurnDeviation().ToString();
            //            client.sendConfHTTP(conf);
        }

        void c_OperationReceived(object sender)
        {
            //operation correctly received
            operationReceived();
        }

        void c_PictureAnalyzed(object sender)
        {
            // Picture analyzed by the server
            if (client.isRecognized())
            {
                Debug.Print("[CAMERA] picture recognized");
                multicolorLED2.TurnGreen();
                currentOperation = "NULL";
                picNumber = 0;
                // I can call getOperation() and go on
                getOperation();
            }
            else if (picNumber == 1) m_TakePic(this);
            else if (picNumber < 4)
            {
                int n = -1;
                int exp = 1;
                for (int i = 0; i < picNumber; i++) exp *= n;
                motor.MoveSpeedTiming(exp*motor.getHighSpeed(), exp*motor.getHighSpeed(), 0, (int)50*(picNumber-1), 1);
            }
            else
            {
                Debug.Print("[CAMERA] picture NOT recognized");
                multicolorLED2.TurnRed();
                currentOperation = "NULL";
                picNumber = 0;
                // TODO Not sure i have to call getOperation on FAIL
                //getOperation();
            }

//            if (!client.isRecognized())
//                {
//                    while (!camera.CameraReady) ;
//                    camera.TakePicture();
//                   client.setProcessing(true);
//                }
//                else
//                {
//                    Debug.Print("[CAMERA] picture recognized");
//                    multicolorLED2.TurnGreen();
//                    isTakingPicture = false;
//                    currentOperation = "NULL";
//                    return;
//                }
//            }
//            Debug.Print("[CAMERA] picture NOT recognized");
//            multicolorLED2.TurnRed();
//            isTakingPicture = false;
//            currentOperation = "NULL";
        }

        void m_Stop(object sender)
        {
            getOperation();
        }

        void m_TakePic(object sender)
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
            while (difference.Seconds < 1);
            takePicture();
        }

        private void wifi_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("[PROGRAM] network down");
            multicolorLED.TurnRed();
            multicolorLED2.TurnRed();
            network.resetIp();
            network.connectWiFiLoop();
        }

        private void wifi_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {

            network.ConnectWIFI();
            if (network.getIp() != "0.0.0.0")
            {
                multicolorLED.TurnGreen();
                client.SendIpHTTP(network.getIp());
                
                //Thread t_op = new Thread(threadSendConfig);
                //t_op.Start();
            }
        }

        private void threadSendConfig()
        {
            //TODO
            //while (client.isProcessing()) ;
            string conf = motor.getSlowSpeed().ToString("F2") + "," + motor.getHighSpeed().ToString("F2") + "," + motor.getLimitLines().ToString("F2") + "," + motor.getTurnDeviation().ToString("F2");
            Debug.Print("[PROGRAM] Send Car's Config: "+ motor.getSlowSpeed().ToString("F2") + "," + motor.getHighSpeed().ToString("F2") + "," + motor.getLimitLines().ToString("F2") + "," + motor.getTurnDeviation().ToString("F2"));
            client.sendConfHTTP(conf);
        }

        private void button_ButtonReleased(Button sender, Button.ButtonState state)
        {
        }

        private void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            //Thread t_op = new Thread(takePicAndGo);
            //t_op.Start();
            if (!isTakingPicture) takePicture();
        }


        private void getOperation()
        {
            //Thread t_cl;
            multicolorLED2.TurnBlue();
            client.RecvOperation();
            Debug.Print("[PROGRAM] waiting server response");
            //TODO
            //while (client.isProcessing()) ; //wait the client receive the operation type from server to continue
        }

        private void operationReceived()
        {
            //Thread t;
            multicolorLED2.TurnWhite();
            currentOperation = client.getOperation(); //read the operation received before
            if (currentOperation != "NULL")
            {
                String angle = currentOperation.Substring(1);

                Debug.Print("[PROGRAM] Operation: " + currentOperation);

                if (currentOperation == "PICT")
                {
                    //since picture requires network can't read next op until this finish

                    isTakingPicture = true; //isTakingPicture set here to avoid race condition
                    //t = new Thread(takePicture);
                    //t.Start();
                    takePicture();
                    //TODO
                    //while (isTakingPicture) ;
                }

                else if (currentOperation == "STOP")
                {
                    //t = new Thread(motor.moveStop);
                    //t.Start();
                    motor.moveStop();
                    currentOperation = "NULL";
                }

                else if (currentOperation[0] == 'F')
                {
                    //Thread t = new Thread(motor.move);
                    //t.Start();
                    motor.move();
                    currentOperation = "NULL";
                }

                else if (currentOperation[0] == 'R')
                {
                    motor.angle = int.Parse(angle);
                    //t = new Thread(motor.moveRight);
                    //t.Start();
                    motor.moveRight();
                    currentOperation = "NULL";
                }
                else if (currentOperation[0] == 'L')
                {
                    motor.angle = int.Parse(angle);
                    //t = new Thread(motor.moveLeft);
                    //t.Start();
                    motor.moveLeft();
                    currentOperation = "NULL";
                }

                /*else if (currentOperation.Equals(Operation.FORWARD2))
                {
                    t = new Thread(motor.moveForward2);
                    t.Start();
                    currentOperation = Operation.NULL;
                }

                else if (currentOperation.Equals(Operation.RIGHT2))
                {
                    t = new Thread(motor.moveRight2);
                    t.Start();
                    currentOperation = Operation.NULL;
                }
                else if (currentOperation.Equals(Operation.LEFT2))
                {
                    t = new Thread(motor.moveLeft2);
                    t.Start();
                    currentOperation = Operation.NULL;
                }*/

                else if (currentOperation[0] == 'B')
                {
                    //t = new Thread(motor.moveBackward2);
                    //t.Start();
                    motor.moveBackward();
                    currentOperation = "NULL";
                }
                else if (currentOperation[0] == 'C')
                {
                    //t = new Thread(configure);
                    //t.Start();
                    configure();
                    currentOperation = "NULL";
                }

                //TODO
                //while (motor.isMoving()) ;
                //getOperation();
                //TODO
                //while (client.isProcessing()) ; //wait the client receive the operation type from server to continue

                //multicolorLED2.TurnWhite();
                //currentOperation = client.getOperation(); //read the operation received before
            }
            else getOperation();
            multicolorLED2.TurnGreen();
        }

        private void configure()
        {
            String[] conf = currentOperation.Substring(1).Split('/');
            if (conf[0] != "0.00")
            {
                Debug.Print("[PROGRAM] SET SlowSpeed to " + (float)Double.Parse(conf[0]));
                motor.setSlowSpeed((float)Double.Parse(conf[0]));
            }
            if (conf[1] != "0.00")
            {
                Debug.Print("[PROGRAM] SET HighSpeed to " + (float)Double.Parse(conf[1]));
                motor.setHighSpeed((float)Double.Parse(conf[1]));
            }
            if (conf[2] != "0.00")
            {
                Debug.Print("[PROGRAM] SET LimitLines to " + conf[2]);
                motor.setLimitLines((int)Double.Parse(conf[2]));
            }
            if (conf[3] != "0.00")
            {
                Debug.Print("[PROGRAM] SET TurnDeviation to " + (float)Double.Parse(conf[3]));
                motor.setTurnDeviation((float)Double.Parse(conf[3]));
            }
            getOperation();
        }

        private void takePicture()
        {
            isTakingPicture = true;

            //wait camera is ready
            while (!camera.CameraReady) ;
            picNumber++;
            Debug.Print("[CAMERA] Try taking photo");
            multicolorLED2.TurnBlue();
            //1st attempt
            camera.TakePicture(); //this cause the picture handler activation

//            client.setProcessing(true); //set client busy here avoid race condition

            //other 4 attempt
//            for (int recgn = 0; recgn < 4; recgn++)
//            {
//                multicolorLED2.TurnBlue();
                //TODO
//                while (client.isProcessing()) ;
//                Debug.Print("[CAMERA] Retry: " + (recgn + 1) + "/5");

//                if (!client.isRecognized())
//                {
//                    while (!camera.CameraReady) ;
//                    camera.TakePicture();
//                   client.setProcessing(true);
//                }
//                else
//                {
//                    Debug.Print("[CAMERA] picture recognized");
//                    multicolorLED2.TurnGreen();
//                    isTakingPicture = false;
//                    currentOperation = "NULL";
//                    return;
//                }
//            }
//            Debug.Print("[CAMERA] picture NOT recognized");
//            multicolorLED2.TurnRed();
//            isTakingPicture = false;
//            currentOperation = "NULL";
        }


        private void camera_PictureCaptured(Camera sender, GT.Picture e)
        {
            Debug.Print("[CAMERA] picture Captured");
            picture.setPicture(e);
            byte[] bmpBuffer = picture.PictureToBytes();
            client.sendPictureTCP(bmpBuffer);
        }


        //timer check path
    }
}
