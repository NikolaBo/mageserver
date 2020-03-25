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
            Vector2 _inputDirection = Vector2.Zero;
            if (inputs[0]) _inputDirection.Y += 1;
            if (inputs[1]) _inputDirection.Y -= 1;
            if (inputs[2]) _inputDirection.X += 1;
            if (inputs[3]) _inputDirection.X -= 1;

            Move(_inputDirection); //Move in the given direction
        }

        private void Move(Vector2 _direction)
        {
            //Not needed when not synchronizing rotation
            //Vector3 _up = Vector3.UnitY;
            //Vector3 _right = Vector3.UnitX;

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
