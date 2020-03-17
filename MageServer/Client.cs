using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MageServer
{
    class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public TCP tcp;

        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private byte[] receiveBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the Server!");
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if(socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("Error sending data to player " + id + "using TCP: " + _ex);
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int byteLength = stream.EndRead(_result);
                    if (byteLength <= 0)
                    {
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("Error receiving TCP data: " + _ex);
                }
            }
        }
    }
}
