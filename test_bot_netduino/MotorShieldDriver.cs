using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;


namespace test_bot_netduino
{
    class MotorShieldDriver
    {
       /* public static double frequency = 490;

        private static object leftPWM;
        private static object leftDir;

        public MotorShieldDriver(string A, string B)
        {
            OutputPort leftDir = new OutputPort(Pins.GPIO_PIN_D12, false);
            PWM leftPWM = new PWM(PWMChannels.PWM_PIN_D3, frequency, 1, false);
            OutputPort rightDir = new OutputPort(Pins.GPIO_PIN_D13, false);
            PWM rightPWM = new PWM(PWMChannels.PWM_PIN_D11, frequency, 1, false);
        }

        public static void Forward(int s, bool r)
        {
            if (r == true && (s <= 255))// || (s > 0))
            {


                leftPWM .Write(true);
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

        /*void left1358014915(int s, bool readyy)
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
        */
        
    }
}
