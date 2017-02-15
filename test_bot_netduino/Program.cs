using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using RockSatC_2016.Utility;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

using TA.NetMF;


namespace test_bot_netduino
{
    public class Program

    {
        public static bool readyy = false;
        public static int sensorValue = 0, oldSenV = 0;

        public static double frequency = 490;

        static OutputPort RedLED = new OutputPort(Pins.GPIO_PIN_D4, false);
        static OutputPort YellowLED = new OutputPort(Pins.GPIO_PIN_D5, false);
        static OutputPort GreenLED = new OutputPort(Pins.GPIO_PIN_D6, false);
        static OutputPort BlueLED = new OutputPort(Pins.GPIO_PIN_D7, false);

        
        static OutputPort leftDir = new OutputPort(Pins.GPIO_PIN_D12, false);
        static PWM leftPWM = new PWM(PWMChannels.PWM_PIN_D3, frequency, 1, false);
        static OutputPort rightDir = new OutputPort(Pins.GPIO_PIN_D13, false);
        static PWM rightPWM = new PWM(PWMChannels.PWM_PIN_D11, frequency, 1, false);

        
        
        static AnalogInput irPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);
        static AnalogInput activePin = new AnalogInput(AnalogChannels.ANALOG_PIN_A2);

        static HMC5883L sensor = new HMC5883L();

        //static Receiver Beacon = new Receiver();

        static SerialPort dataLine = new SerialPort(SerialPorts.COM1, 57600);
        /*
        public class receiver
        {
            private I2CDevice.Configuration _configuration;
            private int[] _busData;
            private byte _address = 0x08;
            public receiver(int clockRate, int packetCount)
            {
                _configuration = new I2CDevice.Configuration(_address, clockRate);
                _busData = new int[packetCount];
               
                I2CDevice.I2CTransaction[] xActions = new I2CDevice.I2CTransaction[2];
            }

            public bool beaconCheck()
            {
                
                return true;
            }

            public int getHeading()
            {
                return 1;
            }
            //beacon 
            
        }*/

        public static void Main()
        {
            setup();
            //loop();
            //Loop2();
            //Loop3();
            //Loop4();
            compassTest();
        }

        public static void compassTest()
        {
            while (true)
            {
                // Einlesen
                sensor.ReadMagnetic();
                // Ergebnisse ausgeben
                Debug.Print("Raw Measurement: X " + sensor.AxisX +
                            "\tY" + sensor.AxisY +
                            "\tZ" + sensor.AxisZ);

                // Kurz warten
                Thread.Sleep(100);
            }
        }

        public static void Loop4()
        {
            while (true)
            {
                // wait a little for the buffer to fill
                Thread.Sleep(100);

                // create an array for the incoming bytes
                byte[] bytes = new byte[dataLine.BytesToRead];
                // read the bytes
                dataLine.Read(bytes, 0, bytes.Length);
                // convert the bytes into a string
                //string line = Encoding.UTF8.GetString(bytes);
                string line = bytes.ToString();

                // write the received bytes, as a string, to the console
                Debug.Print("echo: " + line);

            }
        }

        static void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // wait a little for the buffer to fill
            Thread.Sleep(100);

            // create an array for the incoming bytes
            byte[] bytes = new byte[dataLine.BytesToRead];
            // read the bytes
            dataLine.Read(bytes, 0, bytes.Length);
            // convert the bytes into a string
            //string line = Encoding.UTF8.GetString(bytes);
            string line = bytes.ToString();

            // write the received bytes, as a string, to the console
            Debug.Print("echo: " + line);
        }

        public static void Loop3()
        {
            while (true)
            {
                byte[] bufferRead = new byte[20];
                int i = 0;
                Debug.Print("Recieving...");
                while (dataLine.BytesToRead > 0)//While data available
                {
                    dataLine.Read(bufferRead, 0, bufferRead.Length);
                    i++;
                }
                char[] cc = Encoding.UTF8.GetChars(bufferRead, 0, bufferRead.Length);
                //cc[i] = '\0';
                string data = new string(cc);
                int heading = Convert.ToInt32(data);


                Debug.Print("I got: " + heading);

                Debug.Print("Complete...");

            }
        }

        /*public static void Loop2()
        {
            while (true)
            {
                Beacon.Location();
                Thread.Sleep(2000);
            }
        }*/

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
                            //Halt();
                            //MotorShieldDriver.Right(255, readyy);
                            Right(255, readyy);
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

        public static int setup()
        {
            int error = 0;
            //Thread.Sleep(10000);
            dataLine.Open();
            Debug.Print("Serial open...");
            //Enter setup statments here
            //Receiver.initialize();
            //var scout = new Receiver();
            return error;
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

        /*
        public class I2CBus : IDisposable
        {
            private static I2CBus _instance = null;
            private static readonly object LockObject = new object();

            public static I2CBus GetInstance()
            {
                lock (LockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new I2CBus();
                    }
                    return _instance;
                }
            }


            private I2CDevice _slaveDevice;

            private I2CBus()
            {
                this._slaveDevice = new I2CDevice(new I2CDevice.Configuration(0, 0));
            }

            public void Dispose()
            {
                this._slaveDevice.Dispose();
            }

            /// <summary>
            /// Generic write operation to I2C slave device.
            /// </summary>
            /// <param name="config">I2C slave device configuration.</param>
            /// <param name="writeBuffer">The array of bytes that will be sent to the device.</param>
            /// <param name="transactionTimeout">The amount of time the system will wait before resuming execution of the transaction.</param>
            public void Write(I2CDevice.Configuration config, byte[] writeBuffer, int transactionTimeout)
            {
                // Set i2c device configuration.
                _slaveDevice.Config = config;

                // create an i2c write transaction to be sent to the device.
                I2CDevice.I2CTransaction[] writeXAction = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(writeBuffer) };

                lock (_slaveDevice)
                {
                    // the i2c data is sent here to the device.
                    int transferred = _slaveDevice.Execute(writeXAction, transactionTimeout);

                    // make sure the data was sent.
                    if (transferred != writeBuffer.Length)
                        throw new Exception("Could not write to device.");
                }
            }

            /// <summary>
            /// Generic read operation from I2C slave device.
            /// </summary>
            /// <param name="config">I2C slave device configuration.</param>
            /// <param name="readBuffer">The array of bytes that will contain the data read from the device.</param>
            /// <param name="transactionTimeout">The amount of time the system will wait before resuming execution of the transaction.</param>
            public void Read(I2CDevice.Configuration config, byte[] readBuffer, int transactionTimeout)
            {
                // Set i2c device configuration.
                _slaveDevice.Config = config;

                // create an i2c read transaction to be sent to the device.
                I2CDevice.I2CTransaction[] readXAction = new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(readBuffer) };

                lock (_slaveDevice)
                {
                    // the i2c data is received here from the device.
                    int transferred = _slaveDevice.Execute(readXAction, transactionTimeout);

                    // make sure the data was received.
                    if (transferred != readBuffer.Length)
                        throw new Exception("Could not read from device.");
                }
            }

            /// <summary>
            /// Read array of bytes at specific register from the I2C slave device.
            /// </summary>
            /// <param name="config">I2C slave device configuration.</param>
            /// <param name="register">The register to read bytes from.</param>
            /// <param name="readBuffer">The array of bytes that will contain the data read from the device.</param>
            /// <param name="transactionTimeout">The amount of time the system will wait before resuming execution of the transaction.</param>
            public void ReadRegister(I2CDevice.Configuration config, byte register, byte[] readBuffer, int transactionTimeout)
            {
                byte[] registerBuffer = {register};
                Write(config, registerBuffer, transactionTimeout);
                Read(config, readBuffer, transactionTimeout);
            }

            /// <summary>
            /// Write array of bytes value to a specific register on the I2C slave device.
            /// </summary>
            /// <param name="config">I2C slave device configuration.</param>
            /// <param name="register">The register to send bytes to.</param>
            /// <param name="writeBuffer">The array of bytes that will be sent to the device.</param>
            /// <param name="transactionTimeout">The amount of time the system will wait before resuming execution of the transaction.</param>
            public void WriteRegister(I2CDevice.Configuration config, byte register, byte[] writeBuffer, int transactionTimeout)
            {
                byte[] registerBuffer = { register };
                Write(config, registerBuffer, transactionTimeout);
                Write(config, writeBuffer, transactionTimeout);
            }

            /// <summary>
            /// Write a byte value to a specific register on the I2C slave device.
            /// </summary>
            /// <param name="config">I2C slave device configuration.</param>
            /// <param name="register">The register to send bytes to.</param>
            /// <param name="value">The byte that will be sent to the device.</param>
            /// <param name="transactionTimeout">The amount of time the system will wait before resuming execution of the transaction.</param>
            public void WriteRegister(I2CDevice.Configuration config, byte register, byte value, int transactionTimeout)
            {
                byte[] writeBuffer = { register, value };
                Write(config, writeBuffer, transactionTimeout);
            }

        }

        public class Reciever
        {
            private I2CDevice.Configuration _slaveConfig;
            private const int TransactionTimeout = 1000; // ms
            private const byte ClockRateKHz = 100;
            public byte Address { get; private set; }

            /// <summary>
            /// Example sensor constructor
            /// </summary>
            /// <param name="address">I2C device address of the example sensor</param>
            public Reciever(byte address)
            {
                Address = address;
                _slaveConfig = new I2CDevice.Configuration(address, ClockRateKHz);
            }


            public byte[] ReadBus()
            {
                // write register address
                I2CBus.GetInstance().Write(_slaveConfig, new byte[] { 0xF2 }, TransactionTimeout);

                // get MSB and LSB result
                byte[] data = new byte[2];
                I2CBus.GetInstance().Read(_slaveConfig, data, TransactionTimeout);

                return data;
            }

            public byte[] ReadFromRegister()
            {
                // get MSB and LSB result
                byte[] data = new byte[2];
                I2CBus.GetInstance().ReadRegister(_slaveConfig, 0xF1, data, TransactionTimeout);

                return data;
            }

            public void WriteToRegister()
            {
                I2CBus.GetInstance().WriteRegister(_slaveConfig, 0x3C, new byte[2] { 0xF4, 0x2E }, TransactionTimeout);
            }


        }
        

        I2CDevice.I2CWriteTransaction CreateWriteTransaction(byte[] buffer, uint internalAddress, byte internalAddressSize)
        {
            I2CDevice.I2CWriteTransaction writeTransaction = I2CDevice.CreateWriteTransaction(buffer);
            Type writeTransactionType = typeof(I2CDevice.I2CWriteTransaction);

            FieldInfo fieldInfo = writeTransactionType.GetField("Custom_InternalAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(writeTransaction, internalAddress);

            fieldInfo = writeTransactionType.GetField("Custom_InternalAddressSize", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(writeTransaction, internalAddressSize);

            return writeTransaction;
        }

        I2CDevice.I2CReadTransaction CreateReadTransaction(byte[] buffer, uint internalAddress, byte internalAddressSize)
        {
            I2CDevice.I2CReadTransaction readTransaction = I2CDevice.CreateReadTransaction(buffer);
            Type readTransactionType = typeof(I2CDevice.I2CReadTransaction);

            FieldInfo fieldInfo = readTransactionType.GetField("Custom_InternalAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(readTransaction, internalAddress);

            fieldInfo = readTransactionType.GetField("Custom_InternalAddressSize", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(readTransaction, internalAddressSize);

            return readTransaction;
        }

    */
    }
}