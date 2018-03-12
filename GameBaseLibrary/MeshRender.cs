using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameBaseLibrary
{
    public class MeshRender : Component
    {
        bool enableBasicEffect = false;
        Effect effect;
        TextureMap textureMap;

        public void Initialize(Effect effect, List<Texture2D> texture)
        {
            base.Initialize();

            if (effect != null)
                this.effect = effect;
            else
                enableBasicEffect = true;
            textureMap = new TextureMap();
            textureMap.textures = texture;
        }

        public void Draw(Mesh mesh, Matrix world, Camera camera)
        {
            base.Update();

            if (enableBasicEffect)
                DrawWithBasicEffect(mesh, world, camera);

            else
                DrawWithCustomEffect(mesh, world, camera);
        }

        private void DrawWithBasicEffect(Mesh mesh, Matrix world, Camera camera)
        {
            foreach (ModelMesh modelMesh in mesh.Model.Meshes)
            {
                foreach (BasicEffect effect in modelMesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = world;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                modelMesh.Draw();
            }
        }

        private void DrawWithCustomEffect(Mesh mesh, Matrix world, Camera camera)
        {
            foreach (ModelMesh modelMesh in mesh.Model.Meshes)
            {
                foreach (ModelMeshPart part in modelMesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(camera.View);
                    effect.Parameters["Projection"].SetValue(camera.Projection);
                    effect.Parameters["ViewVector"].SetValue(camera.ViewVector);

                    Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(world));
                    effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

                    try
                    {
                        effect.Parameters["ModelTexture"].SetValue(textureMap.textures[(int)Map.Diffuse]);
                    }
                    catch
                    {
                        Console.WriteLine("DiffuseMapがありません");
                    }

                    try
                    {
                        effect.Parameters["NormalMap"].SetValue(textureMap.textures[(int)Map.Normal]);
                    }
                    catch
                    {
                        Console.WriteLine("NormalMapがありません");
                    }
                }
                modelMesh.Draw();
            }
        }
    }
}