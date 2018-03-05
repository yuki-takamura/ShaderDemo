using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace GameBaseLibrary
{
    public class Transform : Component
    {
        public Vector3 position = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;
        public Vector3 scale = Vector3.Zero;

        public Transform()
        {
        }

        public Transform(Vector3 vector)
        {
            position = vector;
            rotation = vector;
            scale    = vector;
        }

        public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }
}
