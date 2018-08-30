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
        Thread CheckStopThread;
        Thread CheckSensorThread;

        //timers
        GT.Timer timer;

        //client
        static Client client;

        //Wifi
        Network network;

        //picture
        Picture picture;

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
            
            //event handlers
            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
            button.ButtonReleased += new Button.ButtonEventHandler(button_ButtonReleased);
            camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);
            wifiRS21.NetworkUp += new GTM.Module.NetworkModule.NetworkEventHandler(wifi_NetworkUp);
            wifiRS21.NetworkDown += new GTM.Module.NetworkModule.NetworkEventHandler(wifi_NetworkDown);

            //client
            client = new Client(multicolorLED2);
//            client.ConnectionEnd += c_ConnectionEnd;

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

            //timers
            timer = new GT.Timer(10);
            //timer.Tick += operation_timer;

            //pictures
            picture = new Picture(multicolorLED2);

            //breakout
            breakOut = new BreakOut();

            //motor
            motor = new Motor(motorDriverL298, breakOut, multicolorLED2);

            //flags
            isChecking = false;
            isTakingPicture = false;
        }

        private void operation_timer(GT.Timer timer)
        {
        }

//        static void c_ConnectionEnd(object sender, EventArgs e)
//        {
//            string conf = motor.getSlowSpeed().ToString() + "," + motor.getHighSpeed().ToString() + "," + motor.getLimitLines().ToString() + "," + motor.getTurnDeviation().ToString();
//            client.sendConfHTTP(conf);
//        }

        private void wifi_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("[PROGRAM] network down");
            multicolorLED.TurnRed();
        }

        private void wifi_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {

            network.ConnectWIFI();
            if (network.getIp() != "0.0.0.0")
            {
                multicolorLED.TurnGreen();
                client.SendIpHTTP(network.getIp());
                
                Thread t_op = new Thread(threadSendConfig);
                t_op.Start();
            }
        }

        private void threadSendConfig()
        {
            while (client.isProcessing()) ;
            string conf = motor.getSlowSpeed().ToString("F2") + "," + motor.getHighSpeed().ToString("F2") + "," + motor.getLimitLines().ToString("F2") + "," + motor.getTurnDeviation().ToString("F2");
            Debug.Print("[PROGRAM] Send Car's Config: "+ motor.getSlowSpeed().ToString("F2") + "," + motor.getHighSpeed().ToString("F2") + "," + motor.getLimitLines().ToString("F2") + "," + motor.getTurnDeviation().ToString("F2"));
            client.sendConfHTTP(conf);
        }

        private void button_ButtonReleased(Button sender, Button.ButtonState state)
        {
        }

        private void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            Thread t_op = new Thread(takePicAndGo);
            t_op.Start();
        }

        private void takePicAndGo()
        {
            if (!isTakingPicture)
            {
                //takePicture();
            }
 
            operationLoop();
        }

        private void operationLoop()
        {
            Thread t;
            //Thread t_cl;
            multicolorLED2.TurnBlue();
            client.RecvOperation();
            Debug.Print("[PROGRAM] waiting server response");
            while (client.isProcessing()) ; //wait the client receive the operation type from server to continue

            multicolorLED2.TurnWhite();
            currentOperation = client.getOperation(); //read the operation received before
            while (currentOperation != "NULL")
            {
                String angle = currentOperation.Substring(1);

                Debug.Print("[PROGRAM] Operation: " + currentOperation);

                if (currentOperation == "PICT")
                {
                    //since picture requires network can't read next op until this finish
                    isTakingPicture = true; //isTakingPicture set here to avoid race condition
                    t = new Thread(takePicture);
                    t.Start();
                    while (isTakingPicture) ;
                    currentOperation = "NULL";
                }

                else if (currentOperation == "STOP")
                {
                    t = new Thread(motor.moveStop);
                    t.Start();
                    currentOperation = "NULL";
                }

                else if (currentOperation[0] == 'F')
                {
                    t = new Thread(motor.moveForward);
                    t.Start();
                    currentOperation = "NULL";
                }

                else if (currentOperation[0] == 'R')
                {
                    motor.angle = int.Parse(angle);
                    t = new Thread(motor.moveRight);
                    t.Start();
                    currentOperation = "NULL";
                }
                else if (currentOperation[0] == 'L')
                {
                    motor.angle = int.Parse(angle);
                    t = new Thread(motor.moveLeft);
                    t.Start();
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
                    t = new Thread(motor.moveBackward2);
                    t.Start();
                    currentOperation = "NULL";
                }
                else if (currentOperation[0] == 'C')
                {
                    t = new Thread(configure);
                    t.Start();
                    currentOperation = "NULL";
                }

                while (motor.isMoving()) ;

                multicolorLED2.TurnBlue();
                client.RecvOperation();
                Debug.Print("[PROGRAM] waiting server response");
                while (client.isProcessing()) ; //wait the client receive the operation type from server to continue

                multicolorLED2.TurnWhite();
                currentOperation = client.getOperation(); //read the operation received before
            }
                multicolorLED2.TurnGreen();
        }

        private void configure()
        {
            String[] conf = currentOperation.Substring(1).Split('/');
            Debug.Print("[PROGRAM] SET SlowSpeed to "+ (float)Double.Parse(conf[0]));
            motor.setSlowSpeed((float)Double.Parse(conf[0]));
            Debug.Print("[PROGRAM] SET HighSpeed to " + (float)Double.Parse(conf[1]));
            motor.setHighSpeed((float)Double.Parse(conf[1]));
            Debug.Print("[PROGRAM] SET LimitLines to " + conf[2]);
            motor.setLimitLines((int)Double.Parse(conf[2]));
            Debug.Print("[PROGRAM] SET TurnDeviation to " + (float)Double.Parse(conf[3]));
            motor.setTurnDeviation((float)Double.Parse(conf[3]));

        }

        private void takePicture()
        {
            isTakingPicture = true;

            //wait camera is ready
            while (!camera.CameraReady) ;

            Debug.Print("[CAMERA] Try taking photo");
            //1st attempt
            camera.TakePicture(); //this cause the picture handler activation

            client.setProcessing(true); //set client busy here avoid race condition

            //other 4 attempt
            for (int recgn = 0; recgn < 4; recgn++)
            {
                multicolorLED2.TurnBlue();
                while (client.isProcessing()) ;
                Debug.Print("[CAMERA] Retry: " + (recgn + 1) + "/5");

                if (!client.isRecognized())
                {
                    while (!camera.CameraReady) ;
                    camera.TakePicture();
                    client.setProcessing(true);
                }
                else
                {
                    Debug.Print("[CAMERA] picture recognized");
                    multicolorLED2.TurnGreen();
                    isTakingPicture = false;
                    return;
                }
            }
            Debug.Print("[CAMERA] picture NOT recognized");
            multicolorLED2.TurnRed();
            isTakingPicture = false;
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
