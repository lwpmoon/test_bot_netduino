using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

//No longer using this driver
//using HydraMF.Hardware.SparkFun;

namespace test_bot_netduino
{
    public class Program
    {
        public static bool readyy = false;
        public static int sensorValue = 0, oldSenV = 0;

        public static double frequency = 490;

        static OutputPort RedLED = new OutputPort(Pins.GPIO_PIN_D4, true);
        static OutputPort YellowLED = new OutputPort(Pins.GPIO_PIN_D5, true);
        static OutputPort GreenLED = new OutputPort(Pins.GPIO_PIN_D6, true);
        static OutputPort BlueLED = new OutputPort(Pins.GPIO_PIN_D7, true);

        
        static OutputPort leftDir = new OutputPort(Pins.GPIO_PIN_D12, false);
        static PWM leftPWM = new PWM(PWMChannels.PWM_PIN_D3, frequency, 1, false);
        static OutputPort rightDir = new OutputPort(Pins.GPIO_PIN_D13, false);
        static PWM rightPWM = new PWM(PWMChannels.PWM_PIN_D11, frequency, 1, false);
        

        //This is next to integrate
        //static MotorShieldDriver _motorShield = new MotorShieldDriver();

        //Hydra driver
        //static DcMotor leftMotor = new DcMotor(new SimpleHBridge(PWMChannels.PWM_PIN_D3, Pins.GPIO_PIN_D12),speed);
        //static DcMotor rightMotor = new DcMotor(new SimpleHBridge(PWMChannels.PWM_PIN_D11, Pins.GPIO_PIN_D13),speed);

        //static Ardumoto motorShield = new Ardumoto(Cpu.Pin.GPIO_Pin12, Cpu.Pin.GPIO_Pin3, Cpu.Pin.GPIO_Pin13, Cpu.Pin.GPIO_Pin11);
        //static Ardumoto.Motor leftMotor = new Ardumoto.Motor(Cpu.Pin.GPIO_Pin12, Cpu.Pin.GPIO_Pin3);

        static AnalogInput irPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);
        static AnalogInput activePin = new AnalogInput(AnalogChannels.ANALOG_PIN_A2);

        public static void Main()
        {
            loop();
        }

        public static void loop()
        {
            while (true)
            {
                readyy = CheckReady();
                if (readyy == true) GreenLED.Write(true);
                else GreenLED.Write(false);
                sensorValue = Look();
                if(oldSenV != sensorValue)
                {
                    Debug.Print("Sensorvalue = " + sensorValue);
                    Debug.Print("Readyy Value = " + readyy);
                    RedLED.Write(false);
                    YellowLED.Write(false);
                    BlueLED.Write(false);

                    //MotorShieldDriver.Forward(255, readyy);
                    Forward(255, readyy);

                    if (sensorValue >= 1000)BlueLED.Write(true);
                    if (sensorValue >= 2000)YellowLED.Write(true);
                    if (sensorValue >= 3000)
                    {
                        RedLED.Write(true);
                        readyy = CheckReady();
                        //MotorShieldDriver.Halt();
                        Halt();
                        //MotorShieldDriver.Backward(255, readyy);
                        Backward(255, readyy);
                        Thread.Sleep(100);
                        //MotorShieldDriver.Halt();
                        while (sensorValue >= 2000)
                        {
                            //do nothing
                            Debug.Print("Too close...");
                            readyy = CheckReady();
                            Halt();
                            //MotorShieldDriver.Right(255, readyy);
                            sensorValue = Look();
                        }
                    }
                }
                sensorValue = oldSenV;
            }
        }

        public static bool CheckReady()
        {
            bool state = false;
            if (activePin.Read() >= 0.9) state = true;
            return state;
        }

        public static int Look()
        {
            int reading = irPin.ReadRaw();
            return reading;
        }

        public static void Forward(int s, bool r)
        {
            if (r == true && (s <= 255))// || (s > 0))
            {


                leftDir.Write(true);
                rightDir.Write(true);

                leftPWM.DutyCycle = (s / 255);
                rightPWM.DutyCycle = (s / 255);

                leftPWM.Start();
                rightPWM.Start();
            }
            else Halt();
        }

        public static void Backward(int s, bool r)
        {
            if (r == true && (s <= 255))
            {
                Halt();

                leftDir.Write(false);
                rightDir.Write(false);

                leftPWM.DutyCycle = (s / 255);
                rightPWM.DutyCycle = (s / 255);

                leftPWM.Start();
                rightPWM.Start();
            }
            else Halt();
        }

        public static void Right(int s, bool readyy)
        {
            if (readyy == true)
            {
                leftDir.Write(true);
                rightDir.Write(false);

                leftPWM.DutyCycle = (s / 255);
                rightPWM.DutyCycle = (s / 255);

                leftPWM.Start();
                rightPWM.Start();
            }
            else Halt();
        }

        void left1358014915(int s, bool readyy)
        {
            if (readyy == true)
            {
                leftDir.Write(false);
                rightDir.Write(true);

                leftPWM.Start();
                rightPWM.Start();
            }
            else Halt();
        }
        

        public static void Halt()
        {
            leftPWM.Stop();
            rightPWM.Stop();
        }

        /* Building in progress
        private void left(int s, bool readyy)
        {
            if (readyy == true)
            {
                leftDir.Write(false);
                rightDir.Write(true);

                leftPWM.Start();
                rightPWM.Start();
            }
            else MotorShieldDriver.Halt();
        }*/
    }
}