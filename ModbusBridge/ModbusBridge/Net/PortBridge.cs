using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ModbusBridge.UI;

namespace ModbusBridge.Net {

    public class PortBridge{

        private object _lockProcessReceivedData = new object();

        private Thread _tcpThread;
        private TCPHandler _tcpHandler;

        private SerialHandler _serialHandler;

        private ConnectionUI _ui;
        private ConnectionModel _model;

        public PortBridge(ConnectionModel model, ConnectionUI ui) {
            _ui = ui;
            _model = model;
        }

        public void StartTcpHandler() {
            _tcpThread = new Thread(ListenerThread);
            _tcpThread.Start();
        }

        public void StartSerialHandler() {

            try {
                _serialHandler = new SerialHandler(_model.SerialPort, _model.BaudRate, _model.StopBits, _model.Parity);
            } catch (Exception e) {
                _ui.PortError(ConnectionUI.PortType.COM);
            }

        }

        public void Kill() {

            if (_tcpHandler != null) {
                _tcpHandler.Disconnect();
            }

            if (_serialHandler != null) {
                _serialHandler.Close();
            }

        }

        private void ListenerThread(){

            try {

                _tcpHandler = new TCPHandler(_model.TcpPort);
                _tcpHandler.dataChanged += new TCPHandler.DataChanged(ProcessReceivedData);

                _tcpHandler.Connect();

            } catch (Exception e) {
                _ui.PortError(ConnectionUI.PortType.TCP);
            }

        }

        private void ProcessReceivedData(object networkConnectionParameter){

            lock (_lockProcessReceivedData){

                Byte[] bytes = new byte[((TCPHandler.NetworkConnectionParameter)networkConnectionParameter).Bytes.Length];
                Array.Copy(((TCPHandler.NetworkConnectionParameter)networkConnectionParameter).Bytes, 0, bytes, 0, ((TCPHandler.NetworkConnectionParameter)networkConnectionParameter).Bytes.Length);

                _ui.Log("Yeni TCP istek -> " + BitConverter.ToString(bytes).Replace("-", " ")+" | cihaz: "+bytes[6]+" - komut:"+bytes[7]);

                Byte[] tcpHeader = new byte[4];

                int restLength = bytes.Length - 4;

                Byte[] theRest = new byte[restLength];

                try{

                    for (int i = 0; i < 4; i++){
                        tcpHeader[i] = bytes[i];
                    }

                    for (int i = 0; i < restLength; i++){

                        if (i + 6 > bytes.Length - 1){
                            continue;
                        }

                        theRest[i] = bytes[6 + i];
                    }

                    _serialHandler.SendRequest(theRest, (data) => {

                        byte[] response = new byte[data.Length + 4];

                        for (int i = 0; i < 4; i++)
                        {
                            response[i] = tcpHeader[i];
                        }

                        int len = data.Length - 2;

                        Byte[] responseLength = BitConverter.GetBytes(len).Reverse().ToArray();

                        response[4] = responseLength[2];
                        response[5] = responseLength[3];

                        for (int i = 0; i < len; i++)
                        {
                            response[6 + i] = data[i];
                        }

                        NetworkStream stream = ((TCPHandler.NetworkConnectionParameter)networkConnectionParameter).Stream;

                        _ui.Log("COM cevap -> " + BitConverter.ToString(bytes).Replace("-", " ") + " | cihaz: " + bytes[6] + " -  komut:" + bytes[7]);

                        if (stream.CanWrite){
                            stream.Write(response, 0, response.Length);
                        }

                    });

                }
                catch (Exception e){
                }

            }
        }
    }

}
