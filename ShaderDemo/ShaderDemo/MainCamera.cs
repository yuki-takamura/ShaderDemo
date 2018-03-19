using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GameBaseLibrary;

namespace ShaderDemo
{
    public class MainCamera : Entity
    {
        Transform transform;
        public Transform Transform
        {
            get
            {
                return transform;
            }
        }

        Camera camera;
        public Camera Camera
        {
            get
            {
                return camera;
            }
        }

        //float angle = 0;
        Vector3 cameraRight;

        static Vector3 firstPosition = new Vector3(0, 4, 7);
        static Vector3 firstForward = new Vector3(0, -0.4472136f, -0.8944272f);

        public MainCamera()
        {
            transform = new Transform();
            camera = new Camera();
            transform.position = firstPosition;
            camera.Forward = firstForward;
        }

        public void Initialize()
        {
            camera.Initialize(transform.position);
        }

        public void Update(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            //const float distance = 5;

            //transform.position = distance
            //    * (Vector3.UnitX * (float)Math.Sin(angle)
            //    + Vector3.UnitY * 0.7f
            //    + Vector3.UnitZ * (float)Math.Cos(angle));
            UpdateRotation(time);
            UpdateLocation(time);
            camera.Update(transform.position);
        }

        private void UpdateRotation(float time)
        {
            const float unit = 0.0005f;

            float pitch = 0;
            float turn = 0;

            if (InputManager.IsKeyDown(Keys.Up))
                pitch += time * unit;

            if (InputManager.IsKeyDown(Keys.Down))
                pitch -= time * unit;

            if (InputManager.IsKeyDown(Keys.Left))
                turn += time * unit;

            if (InputManager.IsKeyDown(Keys.Right))
                turn -= time * unit;

            cameraRight = Vector3.Cross(Vector3.Up, camera.Forward);
            Vector3 flatFront = Vector3.Cross(cameraRight, Vector3.Up);

            Matrix pitchMatrix = Matrix.CreateFromAxisAngle(cameraRight, pitch);
            Matrix turnMatrix = Matrix.CreateFromAxisAngle(Vector3.Up, turn);

            Vector3 titledFront = Vector3.TransformNormal(camera.Forward, pitchMatrix * turnMatrix);

            // 反転防止で角度チェック
            if (Vector3.Dot(titledFront, flatFront) > 0.001f)
            {
                camera.Forward = Vector3.Normalize(titledFront);
            }
        }

        private void UpdateLocation(float time)
        {
            const float unit = 0.005f;

            if (InputManager.IsKeyDown(Keys.W))
                transform.position += camera.Forward * time * unit;

            if (InputManager.IsKeyDown(Keys.S))
                transform.position -= camera.Forward * time * unit;

            if (InputManager.IsKeyDown(Keys.A))
                transform.position += cameraRight * time * unit;

            if (InputManager.IsKeyDown(Keys.D))
                transform.position -= cameraRight * time * unit;

            if (InputManager.IsKeyDown(Keys.R))
            {
                transform.position = firstPosition;
                camera.Forward = firstForward;
            }
        }
    }
}