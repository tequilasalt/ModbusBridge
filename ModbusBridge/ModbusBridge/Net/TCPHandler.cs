using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ModbusBridge.Net{

    public class TCPHandler{

        public struct NetworkConnectionParameter{
            public NetworkStream Stream;        
            public Byte[] Bytes;
        }

        public delegate void DataChanged(object networkConnectionParameter);

        public event DataChanged dataChanged;

        private TcpListener _server = null;

        private List<Client> _tcpClientLastRequestList = new List<Client>();

        public int NumberOfConnectedClients { get; set; }

        public string ipAddress = null;

        public TCPHandler(int port){

            IPAddress localAddr = IPAddress.Any;
            _server = new TcpListener(localAddr, port);

        }

        public TCPHandler(string ipAddress, int port){

            this.ipAddress = ipAddress;
            IPAddress localAddr = IPAddress.Any;
            _server = new TcpListener(localAddr, port);

        }

        public void Connect(){
            _server.Start();
            _server.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }

        public void Disconnect(){

            try{
                foreach (Client clientLoop in _tcpClientLastRequestList){
                    clientLoop.NetworkStream.Close(00);
                }
            }
            catch (Exception){
            }

            _server.Stop();

        }

        private void AcceptTcpClientCallback(IAsyncResult asyncResult){

            TcpClient tcpClient = new TcpClient();

            try{

                tcpClient = _server.EndAcceptTcpClient(asyncResult);
                tcpClient.ReceiveTimeout = 4000;

                if (ipAddress != null){

                    string ipEndpoint = tcpClient.Client.RemoteEndPoint.ToString();
                    ipEndpoint = ipEndpoint.Split(':')[0];

                    if (ipEndpoint != ipAddress)
                    {
                        tcpClient.Client.Disconnect(false);
                        return;
                    }
                }

            }
            catch (Exception){
            }

            try{

                _server.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
                Client client = new Client(tcpClient);
                NetworkStream networkStream = client.NetworkStream;
                networkStream.ReadTimeout = 4000;
                networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);

            }
            catch (Exception){
            }

        }

        private int GetAndCleanNumberOfConnectedClients(Client client){

            lock (this){

                int i = 0;

                bool objetExists = false;
                foreach (Client clientLoop in _tcpClientLastRequestList){
                    if (client.Equals(clientLoop))
                        objetExists = true;
                }

                try{
                    _tcpClientLastRequestList.RemoveAll(delegate (Client c) { return ((DateTime.Now.Ticks - c.Ticks) > 40000000); });
                }
                catch (Exception){
                }

                if (!objetExists) {
                    _tcpClientLastRequestList.Add(client);
                }

                return _tcpClientLastRequestList.Count;
            }
        }

        private void ReadCallback(IAsyncResult asyncResult){

            NetworkConnectionParameter networkConnectionParameter = new NetworkConnectionParameter();
            Client client = asyncResult.AsyncState as Client;
            client.Ticks = DateTime.Now.Ticks;

            NumberOfConnectedClients = GetAndCleanNumberOfConnectedClients(client);

            if (client != null){

                int read;
                NetworkStream networkStream = null;
                try{
                    networkStream = client.NetworkStream;

                    read = networkStream.EndRead(asyncResult);
                }
                catch (Exception ex){
                    return;
                }
                
                if (read == 0){
                    return;
                }

                byte[] data = new byte[read];
                Buffer.BlockCopy(client.Buffer, 0, data, 0, read);
                networkConnectionParameter.Bytes = data;
                networkConnectionParameter.Stream = networkStream;

                if (dataChanged != null) {
                    dataChanged(networkConnectionParameter);
                }

                try{
                    networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
                }
                catch (Exception){
                }
            }
        }

        

        private class Client{

            private readonly TcpClient _tcpClient;
            private readonly byte[] _buffer;

            public long Ticks { get; set; }

            public Client(TcpClient tcpClient){

                this._tcpClient = tcpClient;
                int bufferSize = tcpClient.ReceiveBufferSize;
                _buffer = new byte[bufferSize];
            }

            public TcpClient TcpClient {
                get { return _tcpClient; }
            }

            public byte[] Buffer {
                get { return _buffer; }
            }

            public NetworkStream NetworkStream {
                get { return _tcpClient.GetStream(); }
            }
        }

    }

}
