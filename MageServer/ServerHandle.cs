using System;
using System.Collections.Generic;
using System.Text;

namespace MageServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            Console.WriteLine(Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint + " connected successfully and is now player " + _fromClient);
            //Console.WriteLine("Player " + _fromClient + " username is " + _username);
            if(_clientIdCheck != _fromClient)
            {
                Console.WriteLine("Player " + _username + " ID: " + _fromClient + " has assumed the wrong client ID " + _clientIdCheck + "!");
            }
        }

        public static void UDPTestReceived(int _fromClient, Packet _packet)
        {
            String msg = _packet.ReadString();
            Console.WriteLine($"Message received from client {_fromClient} via udp: {msg}");
        }
    }
}
