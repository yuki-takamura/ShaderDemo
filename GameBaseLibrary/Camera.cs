using Microsoft.Xna.Framework;

namespace GameBaseLibrary
{
    public class Camera : Component
    {
        public float Near { get; set; }
        public float Far { get; set; }

        public Vector3 Target { get; set; }
        public Vector3 Forward { get; set; }
        public Vector3 ViewVector { get; set; }
        public Matrix View { get; set; }

        public Matrix Projection { get; set; }
        public float FieldOfView { get; set; }
        public float AspectRatio { get; set; }

        public Camera()
        {
            //TODO:マジナン
            Near = 1.0f;
            Far = 1000.0f;
            FieldOfView = MathHelper.ToRadians(45);
            AspectRatio = 1600 / 960;
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, Near, Far);
        }

        public void Initialize(Vector3 position)
        {
            base.Initialize();

            Update(position);
        }

        public void Update(Vector3 position)
        {
            base.Update();

            ViewVector = Vector3.Transform(Target - position, Matrix.CreateRotationY(0));
            ViewVector.Normalize();
            Forward.Normalize();
            View = Matrix.CreateLookAt(position, position + Forward, Vector3.Up);
        }
    }
}
