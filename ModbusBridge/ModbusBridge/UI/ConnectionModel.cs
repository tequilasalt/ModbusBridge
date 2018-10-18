﻿using System.IO.Ports;
using SimpleJSON;

namespace ModbusBridge.UI {

    public class ConnectionModel {

        private int _tcpPort;
        private int _baudRate;

        private string _serialPort;

        private Parity _parity;
        private StopBits _stopBits;

        public ConnectionModel(JSONNode node) {

            _tcpPort = node["port"].AsInt;
            _baudRate = node["baudrate"].AsInt;
            _serialPort = node["com"].Value;

            _parity = Parity.None;

            switch (node["parity"].Value){
                case "odd":
                    _parity = Parity.Odd;
                    break;
                case "even":
                    _parity = Parity.Even;
                    break;
                case "mark":
                    _parity = Parity.Mark;
                    break;
                case "space":
                    _parity = Parity.Space;
                    break;
            }

            _stopBits = StopBits.One;

            switch (node["stopBits"].Value){

                case "1.5":
                    _stopBits = StopBits.OnePointFive;
                    break;
                case "none":
                    _stopBits = StopBits.None;
                    break;
                case "2":
                    _stopBits = StopBits.Two;
                    break;
            }

        }

        public int TcpPort {
            get => _tcpPort;
            set => _tcpPort = value;
        }

        public int BaudRate {
            get => _baudRate;
            set => _baudRate = value;
        }

        public string SerialPort {
            get => _serialPort;
            set => _serialPort = value;
        }

        public Parity Parity {
            get => _parity;
            set => _parity = value;
        }

        public StopBits StopBits {
            get => _stopBits;
            set => _stopBits = value;
        }

        public string Dump {
            get { return ""; }
        }

    }

}
