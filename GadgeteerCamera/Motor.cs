using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;
using System;
using System.Threading;
using Gadgeteer;

namespace GadgeteerCamera
{
    class Motor
    {

        private Gadgeteer.Modules.GHIElectronics.MotorDriverL298 motorDriverL298;
        private Gadgeteer.Timer moveTimer = new Gadgeteer.Timer(100);
        private Gadgeteer.Timer stopTimer = new Gadgeteer.Timer(200);
        private float currentSpeedR, currentSpeedL;
        private int lastAction; // -1:Left 0:Straight 1:Right
        private BreakOut breakOut;
        private int counter;
        private bool stop;
        private bool moving;
        public int angle;
        private MulticolorLED multicolorLED2;
        //IR Sensors Variables
        private bool rfSensor, lfSensor;
        //Configuration Variables
        private static int limitLine = 5; // Number of lines between QR codes
        private static float regimeSlowSpeed = (float)0.3; // Motor speed in stop state
        private static float regimeHighSpeed = (float)0.5; // Motor speed in mobile state
        private static float turnDeviation = (float)0.03; // Deviation factor for speed during turns following line
        private static int time_s = 1; // Time to turn by 90 degrees in seconds
      
        public void setSlowSpeed(float s) { regimeSlowSpeed = s; }
        public float getSlowSpeed() { return regimeSlowSpeed; }

        public void setHighSpeed(float s) { regimeHighSpeed = s; }
        public float getHighSpeed() { return regimeHighSpeed; }

        public void setLimitLines(int l) { limitLine = l; }
        public int getLimitLines() { return limitLine; }

        public void setTurnDeviation(float d) { turnDeviation = d; }
        public float getTurnDeviation() { return turnDeviation; }

        public bool isMoving() { return moving; }

        public Motor(Gadgeteer.Modules.GHIElectronics.MotorDriverL298 motorDriverL298, BreakOut breakOut)
        {
            this.motorDriverL298 = motorDriverL298;
            this.breakOut = breakOut;
            this.lastAction = 0;
            this.counter = 0;
            this.moveTimer.Tick += new Gadgeteer.Timer.TickEventHandler(moveTimer_Tick);
            this.stopTimer.Tick += new Gadgeteer.Timer.TickEventHandler(stopTimer_Tick);
        }

        public Motor(MotorDriverL298 motorDriverL298, BreakOut breakOut, MulticolorLED multicolorLED2)
        {
            // TODO: Complete member initialization
            this.motorDriverL298 = motorDriverL298;
            this.breakOut = breakOut;
            this.multicolorLED2 = multicolorLED2;
            this.lastAction = 0;
            this.counter = 0;
            this.moveTimer.Tick += new Gadgeteer.Timer.TickEventHandler(moveTimer_Tick);
            this.stopTimer.Tick += new Gadgeteer.Timer.TickEventHandler(stopTimer_Tick);
        }
        void stopTimer_Tick(Gadgeteer.Timer timer)
        {
            currentSpeedR -= 0.1f;
            currentSpeedL -= 0.1f;
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, currentSpeedR);
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, currentSpeedL);
            if(currentSpeedR <= regimeSlowSpeed)
            {
                currentSpeedR = regimeSlowSpeed;
                currentSpeedL = regimeSlowSpeed;
                stopTimer.Stop();

            }
            Debug.Print("[MOTOR] New Speed - R:"+currentSpeedR+" L:"+currentSpeedL);
        }

        void moveTimer_Tick(Gadgeteer.Timer timer)
        {
            //readSensors(true, true);
            if (isMoving())
            {
                if (breakOut.rightForwardSensor.Read())
                {
                    if (breakOut.leftForwardSensor.Read())
                    {
                        if (++counter == limitLine)
                        {
                            Debug.Print("[MOTOR] STOP");

                            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeSlowSpeed);
                            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeSlowSpeed);
                            multicolorLED2.TurnGreen();
                            counter = 0;
                            //WE ARE ON QR
                            moveTimer.Stop();
                            moving = false;
                        }
                    }
                    else
                    {
                        if (lastAction == -1)
                        {
                            //Changed Direction
                            Debug.Print("[MOTOR] Change! Previous Action:"+lastAction);
                            currentSpeedR = regimeHighSpeed;
                            currentSpeedL = regimeHighSpeed;
                        }
                        currentSpeedR -= (currentSpeedR - regimeSlowSpeed) * turnDeviation;
                        //currentSpeedR = regimeSlowSpeed;
                        currentSpeedL += (1 - currentSpeedL) * turnDeviation;
                        Debug.Print("[MOTOR] Turn Right - R:" + currentSpeedR + " L:" + currentSpeedL);
                        this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, currentSpeedR);
                        this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, currentSpeedL);
                        lastAction = 1;
                        /* 
                         //slow right
                         if (currentSpeedR > regimeSlowSpeed)
                         {
                             this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeSlowSpeed);
                             currentSpeedR = regimeSlowSpeed;
                         }

                         //fast right
                         else
                         {
                             this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeHighSpeed);
                             currentSpeedR = regimeHighSpeed;
                         } */
                    }
                }
                else if (breakOut.leftForwardSensor.Read())
                {
                    if (lastAction == 1)
                    {
                        //Changed Direction
                        Debug.Print("[MOTOR] Change!");
                        currentSpeedR = regimeHighSpeed;
                        currentSpeedL = regimeHighSpeed;
                    }
                    currentSpeedR += (1 - currentSpeedR) * turnDeviation;
                    currentSpeedL -= (currentSpeedL - regimeSlowSpeed) * turnDeviation;
                    //currentSpeedL = regimeSlowSpeed;
                    Debug.Print("[MOTOR] Turn Left - R:" + currentSpeedR + " L:" + currentSpeedL);
                    this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, currentSpeedR);
                    this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, currentSpeedL);
                    lastAction = -1;
                    /*
                    //slow left
                    if (currentSpeedL > regimeSlowSpeed)
                    {
                        this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeSlowSpeed);
                        currentSpeedL = regimeSlowSpeed;
                    }

                    //fast left
                    if (currentSpeedL < regimeHighSpeed)
                    {
                        this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeHighSpeed);
                        currentSpeedL = regimeHighSpeed;
                    } */
                }
                else
                {
                    // Go straight
                    lastAction = 0;
                    currentSpeedR = regimeHighSpeed;
                    currentSpeedL = regimeHighSpeed;
                    this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, currentSpeedR);
                    this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, currentSpeedL);
                }
            }
        }

        public void move()
        {
            moving = true;
            multicolorLED2.TurnBlue();
            currentSpeedR = regimeHighSpeed;
            currentSpeedL = regimeHighSpeed;

            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeHighSpeed);
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeHighSpeed);
            moveTimer.Start();
            //stopTimer.Start();            
        }
/*
        public void readSensors(bool RightForward, bool LeftForward)
        {
            rfSensor = (RightForward && breakOut.rightForwardSensor.Read()) ? 1 : -1;
            lfSensor = (LeftForward && breakOut.leftForwardSensor.Read()) ? 1 : -1;
            Debug.Print("[MOTOR] Sensors RF:" + rfSensor + " LF:" + lfSensor);
        }
*/
        public void moveForward()
        {
            stop = false;
            Debug.Print("[MOTOR] move forward");
            Thread t_forward = new Thread(move);
            //Thread t_forward = new Thread(ForwardThread);
            t_forward.Start();
            //Debug.Print("[MOTOR] FR: " + breakOut.rightForwardSensor.Read() + " FL: " + breakOut.leftForwardSensor.Read() + " BR: " + breakOut.rightBackwardSensor.Read() + " BL: " + breakOut.leftBackwardSensor.Read());
        }

        public void moveStop()
        {
            stop = true;
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, 0.1);
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, 0.1);
        }

        // angle for rotation
        public void moveRight()
        {
            stop = false;
            //Debug.Print("[MOTOR] move right");
            // inserire la proporzione qui e aggiungerla a time
            MoveSpeedTiming((float)0.4, (float)-0.4, time_s, 0);
        }

        internal void moveLeft()
        {
            stop = false;
            //Debug.Print("[MOTOR] move right");
            // inserire la proporzione qui e aggiungerla a time
            MoveSpeedTiming((float)-0.4, (float)0.4, time_s, 0);
        }


        internal void moveForward2()
        {
            moving = true;
            stop = false;
            //MoveSpeedTiming((float)0.3, (float)0.4, 3, 0);
            MoveSpeedTiming((float)0.5, (float)0.5, 3, 0);
            moving = false;

        }

        internal void moveRight2()
        {
            moving = true;
            stop = false;
            MoveSpeedTiming((float)0.5, (float)-0.5, 3, 0);
            moving = false;

        }

        internal void moveLeft2()
        {
            moving = true;
            stop = false;
            MoveSpeedTiming(-(float)0.5, +(float)0.5, 3, 0);
            moving = false;

        }

        internal void moveBackward2()
        {
            moving = true;
            stop = false;
            MoveSpeedTiming(-(float)0.5, -(float)0.5, 3, 0);
            moving = false;

        }


        //brake test
        public void ForwardThread()
        {
            multicolorLED2.TurnBlue();
            float currentSpeedR = regimeHighSpeed;
            float currentSpeedL = regimeHighSpeed;
            
            Thread t_fwdSensor = new Thread(ForwardSensorThread);
            t_fwdSensor.Start();

            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeHighSpeed);
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeHighSpeed);

            while (!stop)
            {
                Debug.Print("[MOTOR T1] FR: " + breakOut.rightForwardSensor.Read() + " FL: " + breakOut.leftForwardSensor.Read());

                //slow right
                if (breakOut.rightForwardSensor.Read() && currentSpeedR > regimeSlowSpeed)
                {
                    this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeSlowSpeed);
                    currentSpeedR = regimeSlowSpeed;
                }

                //fast right
                if (!breakOut.rightForwardSensor.Read() && currentSpeedR < regimeHighSpeed)
                {
                    this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeHighSpeed);
                    currentSpeedR = regimeHighSpeed;
                }

                //slow left
                if (breakOut.leftForwardSensor.Read() && currentSpeedL > regimeSlowSpeed)
                {
                    this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeSlowSpeed);
                    currentSpeedL = regimeSlowSpeed;
                }

                //fast left
                if (!breakOut.leftForwardSensor.Read() && currentSpeedL < regimeHighSpeed)
                {
                    this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeHighSpeed);
                    currentSpeedL = regimeHighSpeed;
                }
            }
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeSlowSpeed);
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeSlowSpeed);
            Debug.Print("[MOTOR] exit forward");
            multicolorLED2.TurnGreen();
        }

        /*
        public void ForwardThread()
        {
            multicolorLED2.TurnBlue();
            float currentSpeedR = 0;
            float currentSpeedL = 0;
            const float regimeSlowSpeed = (float)0.1;
            const float regimeHighSpeed = (float)0.4;
            const float regimeMidSpeed = (float)0.35;

            //leave first balck line
            while (breakOut.rightForwardSensor.Read() && breakOut.leftForwardSensor.Read())
            {
                this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeHighSpeed);
                this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeHighSpeed);
            }

            Debug.Print("[MOTOR] leave first balck line");
            Thread t_allSensor = new Thread(AllSensorThread);
            t_allSensor.Start();

            while (!stop)
            {
                Debug.Print("[MOTOR] FR: " + breakOut.rightForwardSensor.Read() + " FL: " + breakOut.leftForwardSensor.Read() + " BR: " + breakOut.rightBackwardSensor.Read() + " BL: " + breakOut.leftBackwardSensor.Read());

                //first line reached
                if (breakOut.rightForwardSensor.Read() && breakOut.leftForwardSensor.Read() && ((currentSpeedL != regimeMidSpeed) || (currentSpeedR != regimeMidSpeed)))
                {
                    this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeMidSpeed);
                    this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeMidSpeed);
                    Debug.Print("[MOTOR] first step reached");
                }
                else
                {
                    //slow
                    if (breakOut.rightForwardSensor.Read() && currentSpeedR > regimeSlowSpeed)
                    {
                        this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeSlowSpeed);
                        currentSpeedR = regimeSlowSpeed;
                    }

                    //fast
                    if (!breakOut.rightForwardSensor.Read() && currentSpeedR < regimeHighSpeed)
                    {
                        this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, regimeHighSpeed);
                        currentSpeedR = regimeHighSpeed;
                    }

                    //slow
                    if (breakOut.leftForwardSensor.Read() && currentSpeedL > regimeSlowSpeed)
                    {
                        this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeSlowSpeed);
                        currentSpeedL = regimeSlowSpeed;
                    }

                    //fast
                    if (!breakOut.leftForwardSensor.Read() && currentSpeedL < regimeHighSpeed)
                    {
                        this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, regimeHighSpeed);
                        currentSpeedL = regimeHighSpeed;
                    }
                }
            }
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, 0.1);
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, 0.1);
            Debug.Print("[MOTOR] exit forward");
            multicolorLED2.TurnGreen();
        }*/

        //brake test
        public void ForwardSensorThread()
        {
            stop = false;
            int cnt = 0;
            while (!stop)
            {
                Debug.Print("[MOTOR T2] FR: " + breakOut.rightForwardSensor.Read() + " FL: " + breakOut.leftForwardSensor.Read());
                if (breakOut.rightForwardSensor.Read() && breakOut.leftForwardSensor.Read())
                {
                    cnt++;
                    Debug.Print("[MOTOR] Road Checkpoint"+cnt+" detected");
                    if (cnt>9) stop = true;
                }
            }
            Debug.Print("[MOTOR] stop detected");
        }

        public void AllSensorThread()
        {
            stop = false;
            while (true)
            {
                int cnt = 0;
                while (breakOut.rightForwardSensor.Read() && breakOut.leftForwardSensor.Read())
                {
                    if (cnt > 9)
                    {
                        stop = true;
                        break;
                    }
                    cnt++;
                }

                if (cnt > 9)
                {
                    stop = true;
                    break;
                }
            }
            Debug.Print("[MOTOR] stop detected");
        }




        public void MoveSpeedTiming(float vm1, float vm2, int s, int ms)
        {
            multicolorLED2.TurnBlue();
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, vm2);
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, vm1);

            //wait 500 millissec
            DateTime timeStart = DateTime.Now;
            //Z: Why not Thread.Sleep(s * 1000 + ms); ? Stops Motors?
            while (true)
            {
                DateTime timeNow = DateTime.Now;
                TimeSpan difference = (timeNow - timeStart);
                if (difference.Milliseconds >= ms && difference.Seconds >= s) { break; }
            }
            Debug.Print("[MOTOR] stop");

            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, 0.1);
            this.motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, 0.1);
            multicolorLED2.TurnGreen();
        }



    }
}
