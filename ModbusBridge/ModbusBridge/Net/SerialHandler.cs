using System;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ModbusBridge.Net{

    public class SerialHandler {

        private bool _busy;

        private int _bytesToRead;
        private byte _requestOwner = 0;
        private Timer _timer;
        private Connection _parent;
        private SerialPort _serialPort;
        private SerialCallback _callback;

        public delegate void SerialCallback(Byte[] bytes);

        public SerialHandler(string com, int baudRate, string stopBits, string parity, Connection parent) {

            _parent = parent;

            Parity parsedParity = Parity.None;

            switch (parity){
                case "odd":
                    parsedParity = Parity.Odd;
                    break;
                case "even":
                    parsedParity = Parity.Even;
                    break;
                case "mark":
                    parsedParity = Parity.Mark;
                    break;
                case "space":
                    parsedParity = Parity.Space;
                    break;
            }

            StopBits parsedStopBits = StopBits.One;

            switch (stopBits){
                
                case "1.5":
                    parsedStopBits = StopBits.OnePointFive;
                    break;
                case "none":
                    parsedStopBits = StopBits.None;
                    break;
                case "2":
                    parsedStopBits = StopBits.Two;
                    break;
            }


            _serialPort = new SerialPort();
            _serialPort.PortName = com;
            _serialPort.BaudRate = baudRate;
            _serialPort.Parity = parsedParity;
            _serialPort.StopBits = parsedStopBits;
            _serialPort.WriteTimeout = 10000;
            _serialPort.ReadTimeout = 500;

            _serialPort.DataReceived += SerialportOnDataReceived;

            try {
                _serialPort.Open();
            } catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public bool Busy => _busy;

        public void SendRequest(byte[] bytes, SerialCallback callback) {

            if (Busy){
                return;
            }

            _busy = true;
            _timer = new Timer(500);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();

            //Console.WriteLine("Send to serial - " + BitConverter.ToString(bytes).Replace("-", " "));
            _parent.Log("Send to serial - " + BitConverter.ToString(bytes).Replace("-", " ")+ " Time:" + System.DateTime.Now.Minute + "." + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);

            _callback = callback;
            _requestOwner = bytes[0];

            byte function = bytes[1];
            int quantity = 0;

            byte[] quantityBytes = null;

            switch (function){
                case 1:
                case 2:

                    quantity = Quantity(bytes);

                    if (quantity % 8 == 0){
                        _bytesToRead = 5 + quantity / 8;
                    }else{
                        _bytesToRead = 6 + quantity / 8;
                    }

                    break;
                case 3:
                case 4:

                    quantity = Quantity(bytes);
                    _bytesToRead = 5 + 2 * quantity;

                    break;
                case 5:
                case 6:
                case 15:
                case 16:

                    _bytesToRead = 8;

                    break;

            }

            var crc = BitConverter.GetBytes(CalculateCRC(bytes, (ushort)(bytes.Length - 2), 0));

            bytes[bytes.Length - 2] = crc[0];
            bytes[bytes.Length - 1] = crc[1];

            _serialPort.Write(bytes, 0, bytes.Length);

        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e) {
            //Console.WriteLine("-----------Timer End");
            _parent.Log("--- Request Timeout");
            EndRequest();
        }

        private void SerialportOnDataReceived(object sender, SerialDataReceivedEventArgs e) {

            _serialPort.DataReceived -= SerialportOnDataReceived;

            const long ticksWait = TimeSpan.TicksPerMillisecond * 2000;

            if (_bytesToRead == 0){

                _serialPort.DiscardInBuffer();
                _serialPort.DataReceived += SerialportOnDataReceived;

                return;
            }

            Byte[] readBuffer = new byte[256];
            int numBytes = 0;
            int actualPositionToRead = 0;

            DateTime dateTimeLastRead = DateTime.Now;

            do{

                try{

                    dateTimeLastRead = DateTime.Now;

                    while ((_serialPort.BytesToRead) == 0){

                        System.Threading.Thread.Sleep(10);

                        if ((DateTime.Now.Ticks - dateTimeLastRead.Ticks) > ticksWait){
                            break;
                        }

                    }
                    numBytes = _serialPort.BytesToRead;

                    byte[] rxbytearray = new byte[numBytes];
                    _serialPort.Read(rxbytearray, 0, numBytes);

                    Array.Copy(rxbytearray, 0, readBuffer, actualPositionToRead, (actualPositionToRead + rxbytearray.Length) <= _bytesToRead ? rxbytearray.Length : _bytesToRead - actualPositionToRead);

                    actualPositionToRead = actualPositionToRead + rxbytearray.Length;

                }
                catch (Exception){
                }

                if (_bytesToRead <= actualPositionToRead){
                    break;
                }

                if (DetectValidModbusFrame(readBuffer, (actualPositionToRead < readBuffer.Length) ? actualPositionToRead : readBuffer.Length) | _bytesToRead <= actualPositionToRead){
                    break;
                }

            }
            while ((DateTime.Now.Ticks - dateTimeLastRead.Ticks) < ticksWait);

            var receiveData = new byte[actualPositionToRead];
            Array.Copy(readBuffer, 0, receiveData, 0, (actualPositionToRead < readBuffer.Length) ? actualPositionToRead : readBuffer.Length);

            EndRequest(receiveData);

        }

        private void EndRequest(byte[] receiveData = null) {
            
            if (receiveData != null && receiveData.Length != 0 && receiveData.Length == _bytesToRead && receiveData[0] == _requestOwner) {
                _callback(receiveData);
            } else {
                //Console.WriteLine("Serial response is missing or not verified" + " Time:" + System.DateTime.Now.Minute + "." + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                _parent.Log("Serial response is missing or not verified" + " Time:" + System.DateTime.Now.Minute + "." + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                _parent.Log("\n");
                //Console.WriteLine("--------------------------------------------------");
            }

            if (_timer != null) {

                _timer.Stop();
                _timer = null;

            }

            _bytesToRead = 0;
            _serialPort.DataReceived += SerialportOnDataReceived;

            _busy = false;

        }

        private static int Quantity(byte[] bytes){

            byte[] quantityBytes = new byte[2];
            quantityBytes[0] = bytes[4];
            quantityBytes[1] = bytes[5];

            return int.Parse(BitConverter.ToString(quantityBytes).Replace("-", ""), NumberStyles.HexNumber);

        }

        private static UInt16 CalculateCRC(byte[] data, UInt16 numberOfBytes, int startByte){

            byte[] auchCRCHi = {
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,
            0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,
            0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
            0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,
            0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,
            0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
            0x40
            };

            byte[] auchCRCLo = {
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7, 0x05, 0xC5, 0xC4,
            0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
            0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD,
            0x1D, 0x1C, 0xDC, 0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
            0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32, 0x36, 0xF6, 0xF7,
            0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
            0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE,
            0x2E, 0x2F, 0xEF, 0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
            0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1, 0x63, 0xA3, 0xA2,
            0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
            0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB,
            0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
            0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0, 0x50, 0x90, 0x91,
            0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
            0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88,
            0x48, 0x49, 0x89, 0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83, 0x41, 0x81, 0x80,
            0x40
            };
            UInt16 usDataLen = numberOfBytes;
            byte uchCRCHi = 0xFF;
            byte uchCRCLo = 0xFF;
            int i = 0;
            int uIndex;

            while (usDataLen > 0){
                usDataLen--;

                if ((i + startByte) < data.Length){
                    uIndex = uchCRCLo ^ data[i + startByte];
                    uchCRCLo = (byte)(uchCRCHi ^ auchCRCHi[uIndex]);
                    uchCRCHi = auchCRCLo[uIndex];
                }
                i++;
            }
            return (UInt16)((UInt16)uchCRCHi << 8 | uchCRCLo);
        }

        public static bool DetectValidModbusFrame(byte[] readBuffer, int length){
            // minimum length 6 bytes
            if (length < 6){
                return false;
            }

            //SlaveID correct
            if ((readBuffer[0] < 1) | (readBuffer[0] > 247)){
                return false;
            }

            //CRC correct?
            byte[] crc = new byte[2];
            crc = BitConverter.GetBytes(CalculateCRC(readBuffer, (ushort)(length - 2), 0));

            if (crc[0] != readBuffer[length - 2] | crc[1] != readBuffer[length - 1]){
                return false;
            }

            return true;
        }
    }
}
