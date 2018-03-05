using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace GameBaseLibrary
{
    public class Camera : Component
    {
        public float Near { get; set; }
        public float Far { get; set; }

        public Vector3 Target { get; set; }
        public Vector3 ViewVector { get; set; }
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public Camera()
        {
            Near = 0.1f;
            Far = 1000.0f;
            //TODO:マジナンどうにかする
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 8f / 6f, Near, Far);
        }

        public void Initialize(Vector3 position)
        {
            Update(position);
        }

        public void Update(Vector3 position)
        {
            ViewVector = Vector3.Transform(Target - position, Matrix.CreateRotationY(0));
            ViewVector.Normalize();
            View = Matrix.CreateLookAt(position, Target, Vector3.UnitY);
            base.Update();
        }
    }
}
