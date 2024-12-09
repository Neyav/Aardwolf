using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aardwolf
{
    using OpenTK.Mathematics;

    public class Camera
    {
        public Vector3 Position { get; private set; }
        public Vector3 Front { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 WorldUp { get; private set; }

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public float MovementSpeed { get; set; } = 2.5f;
        public float MouseSensitivity { get; set; } = 0.1f;
        public float Zoom { get; set; } = 45.0f;

        public Camera(Vector3 position, Vector3 up, float yaw, float pitch)
        {
            Position = position;
            WorldUp = up;
            Yaw = yaw;
            Pitch = pitch;
            Front = new Vector3(0.0f, 0.0f, -1.0f);
            UpdateCameraVectors();
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public void ProcessKeyboard(Direction direction, float deltaTime)
        {
            float velocity = MovementSpeed * deltaTime;
            if (direction == Direction.Forward)
                Position += Front * velocity;
            if (direction == Direction.Backward)
                Position -= Front * velocity;
            if (direction == Direction.Left)
                Position -= Right * velocity;
            if (direction == Direction.Right)
                Position += Right * velocity;
        }

        public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
        {
            xOffset *= MouseSensitivity;
            yOffset *= MouseSensitivity;

            Yaw += xOffset;
            Pitch -= yOffset;

            if (constrainPitch)
            {
                if (Pitch > 89.0f)
                    Pitch = 89.0f;
                if (Pitch < -89.0f)
                    Pitch = -89.0f;
            }

            UpdateCameraVectors();
        }

        public void ProcessMouseScroll(float yOffset)
        {
            if (Zoom >= 1.0f && Zoom <= 45.0f)
                Zoom -= yOffset;
            if (Zoom <= 1.0f)
                Zoom = 1.0f;
            if (Zoom >= 45.0f)
                Zoom = 45.0f;
        }

        private void UpdateCameraVectors()
        {
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            Front = Vector3.Normalize(front);
            Right = Vector3.Normalize(Vector3.Cross(Front, WorldUp));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }

    public enum Direction
    {
        Forward,
        Backward,
        Left,
        Right
    }

}
