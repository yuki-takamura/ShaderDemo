using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using GameBaseLibrary;

namespace ShaderDemo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        const int windowWidth = 1600;
        const int windowHegiht = 960;
        SpriteBatch spriteBatch;

        MainCamera mainCamera;
        ModelObject sphere;
        ModelObject plane;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHegiht;
            graphics.PreferMultiSampling = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            InputManager.Initialize();
            mainCamera = new MainCamera();
            mainCamera.Initialize();

            sphere = new ModelObject();
            plane = new ModelObject();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Model model = Content.Load<Model>("Sphere");
            Effect effect = Content.Load<Effect>("Diffuse");
            List<Texture2D> sphereTextures = LoadTexture("DiffuseMap", "NormalMap");
            Transform transform = new Transform(Vector3.UnitY * 2, Vector3.Zero, Vector3.One * 0.5f);
            sphere.Initialize(model, effect, sphereTextures, transform);

            model = Content.Load<Model>("Plane");
            List<Texture2D> planeTextures = LoadTexture("Tile", "Tile_N");
            transform = new Transform(Vector3.Zero, Vector3.Zero, Vector3.One * 5);
            plane.Initialize(model, effect, planeTextures, transform);
        }

        /// <summary>
        /// マテリアルテクスチャを読み取り、Listを作成する
        /// </summary>
        /// <param name="textureName"></param>
        /// <returns></returns>
        private List<Texture2D> LoadTexture(params string[] textureName)
        {
            List<Texture2D> textures = new List<Texture2D>();
            for (int i = 0; i < textureName.Length; i++)
            {
                textures.Add(Content.Load<Texture2D>(textureName[i]));
            }

            return textures;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (InputManager.IsJustKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            mainCamera.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            sphere.Draw(mainCamera.Camera);
            plane.Draw(mainCamera.Camera);

            base.Draw(gameTime);
        }
    }
}