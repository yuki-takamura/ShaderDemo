using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameBaseLibrary
{
    public class ModelObject : Entity
    {
        Transform transform;
        public Transform Transform
        {
            set
            {
                transform = value;
            }
        }
        Mesh mesh;
        MeshRender render;

        public ModelObject()
        {
            transform = new Transform();
            mesh = new Mesh();
            render = new MeshRender();
        }

        public void Initialize(Model model, Effect effect, List<Texture2D> textures, Transform transform = null)
        {
            mesh.Initialize(model);
            render.Initialize(effect, textures);
            if (transform != null)
            {
                this.transform = transform;
            }
        }

        public void Draw(Camera camera)
        {
            render.Draw(mesh, transform.World, camera);
        }
    }
}