using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Numerics;
using System.Net.Sockets;

namespace MageServer
{
    class Client
    {
        //Max number of bytes expected in one NetworkStream Write call
        public static int dataBufferSize = 4096;

        public int id;
        public Player player;
        public TCP tcp;
        public UDP udp;

        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            //TCP class connect method, called when server TCP listener receives a connection
            public void Connect(TcpClient _socket)
            {
                //Assign the client
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream(); //Assign the socket's stream

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null); //Begin receiving TCP data

                ServerSend.Welcome(id, "Welcome to the Server!"); //Send the welcome packet to the client
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    //If there is an associated TCP client
                    if(socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null); //Send the packet as an array of bytes
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("Error sending data to player " + id + " using TCP: " + _ex);
                }
            }

            //Called when TCP data is recieved from the client
            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    //Set the byte length
                    int byteLength = stream.EndRead(_result);
                    if (byteLength <= 0)
                    {
                        return; //Return out if the packet is empty
                    }

                    //Copy the received data from the receive buffer to an array
                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data)); //Handle the received data, and reset received data if there are no incomplete packets
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null); //Begin receiving data again
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("Error receiving TCP data: " + _ex);
                }
            }

            //Method to handle data received via TCP
            private bool HandleData(byte[] _data)
            {
                int packetLength = 0;

                receivedData.SetBytes(_data); //Pass the received bytes to the received data packet
                
                //If there are at least 4 bytes(integer) left
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt(); //Read and set the packet length
                    if (packetLength <= 0)
                    {
                        return true; //If the packet is empty return out and reset
                    }
                }

                //While the packet is not empty and all the bytes have been received
                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength); //Create a byte array with the bytes in this packet
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        //Handle the packet on the Main thread
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            Server.packetHandlers[packetId](id, packet);
                        }
                    });

                    packetLength = 0;
                    //If there is anothe integer in the received data
                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLength = receivedData.ReadInt(); //Set the packet length
                        if (packetLength <= 0)
                        {
                            return true; //If empty return out and reset
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                //There are incomplete packets so do not reset
                return false;
            }
        }

        public class UDP
        {
            public IPEndPoint endPoint;

            private int id;

            public UDP (int _id)
            {
                id = _id;
            }

            //Called when receiving initial UDP packet from client
            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint; //Assign endpoint
            }

            //Send the packet via UDP to the endpoint
            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packetData)
            {
                //Copy packet bytes to array
                int packetLength = _packetData.ReadInt();
                Byte[] packetBytes = _packetData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    //Handle the packet on the Main thread
                    using (Packet _packet = new Packet(packetBytes))
                    {
                        int _id = _packet.ReadInt();
                        Server.packetHandlers[_id](id, _packet);
                    }
                });
            }
        }

        //Sends the client into game, called when handling welcome received packet
        public void SendIntoGame(string _playerName)
        {
            player = new Player(id, _playerName, Vector3.Zero); //Initialize the player

            //Spawn all other connected players on this client
            foreach(Client client in Server.clients.Values)
            {
                if(client.player != null && client.id != id)
                {
                    ServerSend.SpawnPlayer(id, client.player);
                }
            }

            //Spawn this player on all connected clients
            foreach(Client client in Server.clients.Values)
            {
                if(client.player != null)
                {
                    ServerSend.SpawnPlayer(client.id, player);
                }
            }
        }
    }
}
