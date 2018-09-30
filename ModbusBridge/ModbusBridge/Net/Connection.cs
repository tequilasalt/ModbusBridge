using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ModbusBridge.Net {

    public class Connection {

        private int _port;

        private object _lockProcessReceivedData = new object();

        private Thread _listenerThread;
        private TCPHandler _tcpHandler;
        private SerialHandler _serialHandler;

        public Connection(int port, string com, int baudRate, string stopBits, string parity) {
            _port = port;
            _serialHandler = new SerialHandler(com, baudRate, stopBits, parity);

            Listen();
        }

        private void Listen() {

            _listenerThread = new Thread(ListenerThread);
            _listenerThread.Start();

        }

        private void ListenerThread() {

            _tcpHandler = new TCPHandler(_port);
            _tcpHandler.dataChanged += new TCPHandler.DataChanged(ProcessReceivedData);

        }

        private void ProcessReceivedData(object networkConnectionParameter) {

            lock (_lockProcessReceivedData) {

                if (_serialHandler.Busy) {
                    Console.WriteLine("--------- Tcp request ignored");
                    return;
                }

                Byte[] bytes = new byte[((TCPHandler.NetworkConnectionParameter)networkConnectionParameter).Bytes.Length];
                Array.Copy(((TCPHandler.NetworkConnectionParameter)networkConnectionParameter).Bytes, 0, bytes, 0, ((TCPHandler.NetworkConnectionParameter)networkConnectionParameter).Bytes.Length);

                Console.WriteLine("From Tcp client - "+bytes[6]+" : " + BitConverter.ToString(bytes).Replace("-", " ")+ " Time:"+System.DateTime.Now.Minute+"."+ System.DateTime.Now.Second+"."+ System.DateTime.Now.Millisecond);

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

                        for (int i = 0; i < 4; i++){
                            response[i] = tcpHeader[i];
                        }

                        int len = data.Length - 2;

                        Byte[] responseLength = BitConverter.GetBytes(len).Reverse().ToArray();

                        response[4] = responseLength[2];
                        response[5] = responseLength[3];

                        for (int i = 0; i < len; i++){
                            response[6 + i] = data[i];
                        }

                        NetworkStream stream = ((TCPHandler.NetworkConnectionParameter)networkConnectionParameter).Stream;

                        Console.WriteLine("Tcp response - " + BitConverter.ToString(response).Replace("-", " ") + " Time:" + System.DateTime.Now.Minute + "." + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                        Console.WriteLine("--------------------------------------------------");
                        if (stream.CanWrite){
                            stream.Write(response, 0, response.Length);
                        }

                    });

                }
                catch (Exception e){
                    //TODO
                    Console.WriteLine("Error!");
                    Console.WriteLine(e);
                }
            }
        }

    }
}
