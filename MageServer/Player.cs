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
        private bool[] inputs;

        public Player (int _playerId, String _username, Vector3 _spawnPosition)
        {
            id = _playerId;
            username = _username;
            position = _spawnPosition;
            rotation = Quaternion.Identity;

            inputs = new bool[4];
        }

        public void Update()
        {
            Vector2 _inputDirection = Vector2.Zero;
            if (inputs[0]) _inputDirection.Y += 1;
            if (inputs[1]) _inputDirection.Y -= 1;
            if (inputs[2]) _inputDirection.X += 1;
            if (inputs[3]) _inputDirection.X -= 1;

            Move(_inputDirection);
        }

        private void Move(Vector2 _direction)
        {
            Vector3 _up = Vector3.UnitY;
            Vector3 _right = Vector3.UnitX;

            Vector3 moveDirection = _up * _direction.Y + _right * _direction.X;
            position += moveDirection * moveSpeed;

            ServerSend.PlayerPosition(this);
        }

        public void SetInputs(bool[] _inputs)
        {
            inputs = _inputs;
        }
    }
}
