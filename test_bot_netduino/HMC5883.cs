using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace test_bot_netduino
{
    public class HMC5883L
    {
        /// Klasse für die I²C Verbindung
        private I2CDevice _i2CDevice;
        /// Die Variabeln zum Speichern der Ergebnisse nach der Messung.
        private byte[] _Data = new byte[6];
        /// Ruft die Achse X ab oder legt diese fest.
        public int AxisX { get; set; }
        /// Ruft die Achse Y ab oder legt diese fest.
        public int AxisY { get; set; }
        /// Ruft die Achse Z ab oder legt diese fest.
        public int AxisZ { get; set; }
        /// Der Konstruktor Initialisiert die Verbindung her und 
        /// stellt den Sensor mit einer Standard Konfiguration ein.
        public HMC5883L()
        {
            // I²C Bus verbindung herstellen mit 100kHz
            _i2CDevice = new I2CDevice(new I2CDevice.Configuration(0x1E, 100));
            // Operating Mode (0x02):
            // Continuous-Measurement Mode (0x00)
            StatusMessage(Write(new byte[] { 0x02, 0x00 }));

            // Die Konfiguration besteht in zwei Abschnitten.
            // Der erste Byte bestimmt, wie viel Proben pro Messung vorgenommen
            // werden (Default = 1) sollen. In welcher Bit Rate an die Ausgänge
            // geschrieben wird (Default = 15Hz) und den Messmodus bestimmt 
            // die Vorspannung (Default = Normal)
            // Standard Einstellungen, siehe Datenblatt für weitere Einstellungen.
            StatusMessage(Write(new byte[] { 0x00, 0x10 }));
            // Standard Skalierung, siehe ggf. Datenblatt
            // +- 1.3 Ga, 1090 Gain(LSb/Gauss)
            StatusMessage(Write(new byte[] { 0x01, 0x20 }));
        }
        /// Liest die Sensor Messungen ein und schreibt diese in die Properties
        public void ReadMagnetic()
        {
            // Sendet das Byte für die erste Achse
            Write(new byte[] { 0x03 });
            // Nur wenn das Einlesen erfolgreich war.
            _Data[0] = 0x03;
            if (Read(_Data) != 0)
            {
                AxisX = (_Data[0] << 8) | _Data[1];
                AxisY = (_Data[2] << 8) | _Data[3];
                AxisZ = (_Data[4] << 8) | _Data[5];
            }
            else
            {
                Debug.Print("Fehler beim lesen!");
            }
        }
        /// Sendet das Byte Array zum Modul
        private int Write(byte[] buffer)
        {
            I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[]
            {
            I2CDevice.CreateWriteTransaction(buffer)
            };
            return _i2CDevice.Execute(transactions, 1000);
        }
        /// Liest mit den Byte Array die Daten vom Modul
        private int Read(byte[] buffer)
        {
            I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[]
            {
            I2CDevice.CreateWriteTransaction(new byte[] { 0x03 }),
            I2CDevice.CreateReadTransaction(buffer)
            };
            return _i2CDevice.Execute(transactions, 1000);
        }
        /// Gibt mit dem Ergebnis den Status des Vorgangs 
        /// über die Ausgabe(Output) in Visual Studio wieder.
        private void StatusMessage(int result)
        {
            if (result == 0)
            {
                Debug.Print("Status: Fehler beim Senden oder Empfangen");
            }
            else
            {
                Debug.Print("Status: OK");
            }
        }
    }
}
