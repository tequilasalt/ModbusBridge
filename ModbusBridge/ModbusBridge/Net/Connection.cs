using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ModbusBridge.Net {

    public class Connection {

        private int _port;

        private object _lockProcessReceivedData = new object();

        private Thread _listenerThread;
        private TCPHandler _tcpHandler;
        private SerialHandler _serialHandler;

        //Logger
        //private Panel _panel;
        //private Label _label;
        //private TextBox _logBox;

        public Connection(int port, string com, int baudRate, string stopBits, string parity, int index) {

            _port = port;
            _serialHandler = new SerialHandler(com, baudRate, stopBits, parity, this);

            Listen();

            //CreateLogPanel(port, com, index);
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

                Byte[] bytes = new byte[((TCPHandler.NetworkConnectionParameter)networkConnectionParameter).Bytes.Length];
                Array.Copy(((TCPHandler.NetworkConnectionParameter)networkConnectionParameter).Bytes, 0, bytes, 0, ((TCPHandler.NetworkConnectionParameter)networkConnectionParameter).Bytes.Length);

                Console.WriteLine("From Tcp client - "+bytes[6]+" : " + BitConverter.ToString(bytes).Replace("-", " ")+ " Time:"+System.DateTime.Now.Minute+"."+ System.DateTime.Now.Second+"."+ System.DateTime.Now.Millisecond);
                //Log("From Tcp client - "+bytes[6]+" : " + BitConverter.ToString(bytes).Replace("-", " ")+ " Time:"+System.DateTime.Now.Minute+"."+ System.DateTime.Now.Second+"."+ System.DateTime.Now.Millisecond);

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
                        //Log("Tcp response - " + BitConverter.ToString(response).Replace("-", " ") + " Time:" + System.DateTime.Now.Minute + "." + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                        //Log("\n");
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
        /*
        private void CreateLogPanel(int port, string com, int index){

            _panel = new Panel();

            _panel.Location = new System.Drawing.Point(index*600, 0);
            _panel.Name = "LogPanel"+port;
            _panel.Size = new System.Drawing.Size(600, 400);
            _panel.TabIndex = 0;

            MainForm.Instance.Controls.Add(_panel);

            _label = new Label();
            _label.AutoSize = false;
            _label.Location = new System.Drawing.Point(20, 20);
            _label.Name = "Label";
            _label.Size = new System.Drawing.Size(120, 13);
            _label.TabIndex = 0;
            _label.Text = "TCP "+port+ " => "+com;

            _panel.Controls.Add(_label);

            _logBox = new TextBox();

            _logBox.Location = new System.Drawing.Point(20, 40);
            _logBox.Multiline = true;
            _logBox.Name = "LogBox"+port;
            _logBox.Size = new System.Drawing.Size(560, 360);
            _logBox.TabIndex = 0;
            _logBox.ScrollBars = ScrollBars.Vertical;
            //_logBox.Font = new Font(_logBox.Font.FontFamily, 7);

            _panel.Controls.Add(_logBox);

        }

        public void Log(string input){

            if (MainForm.Instance.InvokeRequired){
                MainForm.Instance.Invoke(new Action<string>(Log), new object[] { input });
                return;
            }

            _logBox.AppendText(input);
            _logBox.AppendText("\n");

        }
        */
    }
}
