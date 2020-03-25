using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace MageServer
{
    class ServerHandle
    {
        /// <summary>Handles the client's welcome received packet.</summary>
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            //Read the client's id and username
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            //Log the player's connection
            Console.WriteLine(Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint + " connected successfully and is now player " + _fromClient);

            //Check the player's claimed id against the id according to server
            if(_clientIdCheck != _fromClient)
            {
                Console.WriteLine("Player " + _username + " ID: " + _fromClient + " has assumed the wrong client ID " + _clientIdCheck + "!");
            }

            //Send the client into game
            Server.clients[_fromClient].SendIntoGame(_username);
        }

        /// <summary>Handles the packet with a player's movement inputs.</summary>
        public static void PlayerMovement(int _fromClient, Packet _packet)
        {
            //Initialize and populate the array of inputs
            bool[] inputs = new bool[_packet.ReadInt()];
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = _packet.ReadBool();
            }

            //Call the player's set input function
            Server.clients[_fromClient].player.SetInputs(inputs);
        }
    }
}
