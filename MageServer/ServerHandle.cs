using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

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

            Server.clients[_fromClient].SendIntoGame(_username);
        }

        public static void PlayerMovement(int _fromClient, Packet _packet)
        {
            bool[] inputs = new bool[_packet.ReadInt()];
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = _packet.ReadBool();
            }

            Server.clients[_fromClient].player.SetInputs(inputs);
        }
    }
}
