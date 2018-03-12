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

        float angle = 0;

        public MainCamera()
        {
            transform = new Transform();
            camera = new Camera();
        }

        public void Initialize()
        {
            camera.Initialize(transform.position);
        }

        public void Update()
        {
            const float distance = 5;
            const float rotateAmount = 0.01f;

            //angle += rotateAmount;
            transform.position = distance
                *(Vector3.UnitX * (float)Math.Sin(angle)
                + Vector3.UnitY * 0.7f
                + Vector3.UnitZ * (float)Math.Cos(angle));
            camera.Update(transform.position);
        }
    }
}