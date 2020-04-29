using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace MageServer
{
    class Player
    {
        public int id;
        public string username;

        public Vector3 position;
        public Vector2 direction;
        public Quaternion rotation;

        private float moveSpeed = 5f / Constants.TICKS_PER_SEC;
        private bool[] inputs; //The array of inputs set by the client

        public Player (int _playerId, String _username, Vector3 _spawnPosition)
        {
            //Set initial values
            id = _playerId;
            username = _username;
            position = _spawnPosition;
            rotation = Quaternion.Identity;

            inputs = new bool[4];
        }

        //Called once every tick from the game loop
        public void Update()
        {
            //Calculate the direction of the movement vector from player inputs
            direction = Vector2.Zero;
            if (inputs[0]) direction.Y += 1;
            if (inputs[1]) direction.Y -= 1;
            if (inputs[2]) direction.X += 1;
            if (inputs[3]) direction.X -= 1;

            Move(direction); //Move in the given direction
        }

        private void Move(Vector2 _direction)
        {

            Vector3 moveDirection = Vector3.UnitY * _direction.Y + Vector3.UnitX * _direction.X;
            position += moveDirection * moveSpeed;

            ServerSend.PlayerPosition(this); //Send the player position packet
        }

        //Set inputs to a bool array, called when handling the client player movement packet
        public void SetInputs(bool[] _inputs)
        {
            inputs = _inputs;
        }
    }
}
