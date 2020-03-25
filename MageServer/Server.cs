using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MageServer
{
    class Server
    {
        public static int maxPlayers { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>(); //Dictionary of Clients connected to the server
        public static int port { get; private set; }
        public delegate void PacketHandler(int _fromClient, Packet _packet); //Delegate that handles packets sent by clients
        public static Dictionary<int, PacketHandler> packetHandlers; //Dictionary of packet handlers and their ids, ids from the packet handler enum

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static void Start(int _maxPlayers, int _port)
        {
            maxPlayers = _maxPlayers;
            port = _port;

            Console.WriteLine("Starting server...");
            InitializeServerData(); //Initialize client dictionary and packet handlers

            //Create a TCP listener on the port and accept clients 
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            //Start a UDP client on the port and begin receiving data
            udpListener = new UdpClient(port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine("Server started on " + port);
        }

        //Called when a client TCP connection is established
        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result); //Store the client being accepted
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null); //Start accepting again
            Console.WriteLine("Incoming connection from " + client.Client.RemoteEndPoint + "...");

            //Call the connect method of the TCP class of the first empty client
            for (int i = 1; i <= maxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine(client.Client.RemoteEndPoint + " failed to connect: Server full!");
        }

        //Called when the server receives UDP data
        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint); //Store the bytes received and the endpoint
                udpListener.BeginReceive(UDPReceiveCallback, null); //Start receiving again

                if(_data.Length < 4)
                {
                    return; //If there is not a full packet return out
                }

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0) return; //A client id of 0 cannot exist and would crash server

                    //If new UDP client call client's UDP class connect function
                    if(clients[_clientId].udp.endPoint == null)
                    {
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    //If the packet comes from the client it says it's from
                    if(clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        clients[_clientId].udp.HandleData(_packet); //Call the client's UDP class handle data function
                    }
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving UDP data: {_ex}"); 
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                //If the client's endpoint is set up
                if(_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null); //Send the packet from server's UDP listener
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }

        //Function that initializes the client dictionary and packet handlers
        private static void InitializeServerData()
        {
            for (int i = 1; i <= maxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int) ClientPackets.welcomeRecieved, ServerHandle.WelcomeReceived},
                {(int) ClientPackets.playerMovement, ServerHandle.PlayerMovement }
            };

            Console.WriteLine("Initialized packets.");
        }

    }
}
