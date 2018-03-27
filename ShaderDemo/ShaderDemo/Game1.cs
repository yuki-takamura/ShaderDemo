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
        const int windowWidth = 1280;
        const int windowHegiht = 720;
        SpriteBatch spriteBatch;

        MainCamera mainCamera;
        BoundingFrustum cameraFrustum = new BoundingFrustum(Matrix.Identity);

        ModelObject sphereModel;
        ModelObject planeModel;
        ModelObject cylinderModel;
        ModelObject skyBoxModel;

        /// <summary>
        /// シャドウマップのレンダーターゲット
        /// </summary>
        RenderTarget2D shadowRenderTarget;
        const int shadowMapResolution = 4096;

        RenderTarget2D postEffectRenderTarget;
        Vector2 postEffectRenderSize;
        Effect postEffect;

        Light light;

        Texture2D sketchTexture;
        Texture2D noiseTexture;

        int switching = 0;
        const int max = 6;
        bool isToonRendering = true;

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

            sphereModel = new ModelObject();
            planeModel = new ModelObject();
            cylinderModel = new ModelObject();
            skyBoxModel = new ModelObject();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Model model = Content.Load<Model>("Sphere");
            Effect effect = Content.Load<Effect>("DrawModel");
            List<Texture2D> sphereTextures = LoadTexture("Diffuse_Blue", "NormalMap");
            Transform transform = new Transform(Vector3.UnitY * 10, Vector3.Zero, Vector3.One * 0.5f);
            sphereModel.Initialize(model, effect, sphereTextures, transform);

            model = Content.Load<Model>("Plane");
            List<Texture2D> planeTextures = LoadTexture("Tile", "Tile_N");
            transform = new Transform(Vector3.Zero, Vector3.Zero, Vector3.One * 5);
            planeModel.Initialize(model, effect, planeTextures, transform);

            model = Content.Load<Model>("Cylinder");
            List<Texture2D> cylinderTextures = LoadTexture("GrassGreen", "NormalMap");
            transform = new Transform(new Vector3(-15, 10, 15), Vector3.Zero, Vector3.One * 0.5f);
            cylinderModel.Initialize(model, effect, cylinderTextures, transform);

            model = Content.Load<Model>("SkyBox");
            transform = new Transform(Vector3.Zero, Vector3.Zero, Vector3.One * 100);
            skyBoxModel.Initialize(model, null, null, transform);

            shadowRenderTarget = new RenderTarget2D(graphics.GraphicsDevice,
                shadowMapResolution,
                shadowMapResolution,
                false,
                SurfaceFormat.Single,
                DepthFormat.Depth24);

            postEffect = Content.Load<Effect>("PostProcessEffect");
            postEffectRenderTarget = new RenderTarget2D(graphics.GraphicsDevice,
                graphics.PreferredBackBufferWidth,
                graphics.PreferredBackBufferHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24);
            postEffectRenderSize = new Vector2(postEffectRenderTarget.Width, postEffectRenderTarget.Height);

            light.direction = new Vector3(-0.3333333f, 0.6666667f, 0.6666667f);

            sketchTexture = Content.Load<Texture2D>("Canvas_N");
            noiseTexture = Content.Load<Texture2D>("DistortionTexture");
            //noiseTexture = Content.Load<Texture2D>("SketchTexture");
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

            //ポストエフェクトの切り替えを行う
            if (InputManager.IsJustKeyDown(Keys.Z))
            {
                switching++;
                if (switching > max)
                    switching = 0;

                if (switching == 6)
                    isToonRendering = false;
                else
                    isToonRendering = true;
            }

            mainCamera.Update(gameTime);
            cameraFrustum.Matrix = mainCamera.Camera.View * mainCamera.Camera.Projection;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            light.viewProjection = CreateLightViewProjectionMatrix();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            CreateShadowMap();
            for(int i = 0; i< 4;i++)
                GraphicsDevice.SamplerStates[i] = SamplerState.PointClamp;

            GraphicsDevice.SetRenderTarget(postEffectRenderTarget);

            sphereModel.Draw(mainCamera.Camera, light,isToonRendering, shadowRenderTarget, false);
            planeModel.Draw(mainCamera.Camera, light, isToonRendering, shadowRenderTarget, false);
            cylinderModel.Draw(mainCamera.Camera, light, isToonRendering, shadowRenderTarget, false);
            skyBoxModel.Draw(mainCamera.Camera, light, isToonRendering, shadowRenderTarget, false);

            GraphicsDevice.SetRenderTarget(null);

            switch (switching)
            {
                case 0:
                    GraphicsDevice.Clear(Color.DimGray);
                    SetPostEffect("Sketch");
                    break;
                case 1:
                    GraphicsDevice.Clear(Color.CadetBlue);
                    SetPostEffect("Flip");
                    break;
                case 2:
                    GraphicsDevice.Clear(Color.DimGray);
                    SetPostEffect("Mono");
                    break;
                case 3:
                    GraphicsDevice.Clear(Color.DimGray);
                    SetPostEffect("Sepia");
                    break;
                case 4:
                    GraphicsDevice.Clear(Color.DimGray);
                    SetPostEffect("Noise");
                    break;
                case 5:
                    GraphicsDevice.Clear(Color.DimGray);
                    SetPostEffect();
                    break;
                case 6:
                    GraphicsDevice.Clear(Color.DimGray);
                    SetPostEffect();
                    break;
                default:
                    GraphicsDevice.Clear(Color.DimGray);
                    SetPostEffect();
                    break;
            }

            base.Draw(gameTime);
        }

        private void SetPostEffect(string techniqueName = null)
        {
            if (techniqueName != null)
            {
                postEffect.Parameters["SamplerSize"].SetValue(postEffectRenderSize);
                postEffect.Parameters["SketchThreshold"].SetValue(0.5f);
                postEffect.Parameters["SketchBrightness"].SetValue(0.4f);
                postEffect.Parameters["SketchJitter"].SetValue(0.1f);
                postEffect.Parameters["SketchTexture"].SetValue(sketchTexture);
                postEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);

                postEffect.CurrentTechnique = postEffect.Techniques[techniqueName];
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None,
                    RasterizerState.CullNone, postEffect);
                spriteBatch.Draw(postEffectRenderTarget, Vector2.Zero, Color.White);
            }
            else
            {
                spriteBatch.Begin();
                spriteBatch.Draw(postEffectRenderTarget, Vector2.Zero, Color.White);
            }
            spriteBatch.End();
        }

        Matrix CreateLightViewProjectionMatrix()
        {
            Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero,
                -light.direction,
                Vector3.Up);

            //錐台のコーナーを取得
            Vector3[] frustumCorners = cameraFrustum.GetCorners();

            for (int i = 0; i < frustumCorners.Length; i++)
            {
                frustumCorners[i] = Vector3.Transform(frustumCorners[i], lightRotation);
            }

            //ポイントの周りの最小ボックスを見つける
            BoundingBox lightBox = BoundingBox.CreateFromPoints(frustumCorners);

            Vector3 boxSize = lightBox.Max - lightBox.Min;
            Vector3 halfBoxSize = boxSize * 0.5f;

            //光源の位置はボックスのバックパネルの中心におく必要がある
            Vector3 lightPosition = lightBox.Min + halfBoxSize;
            lightPosition.Z = lightBox.Min.Z;

            //ワールド座標に位置を戻す必要があるので、高原の回転の逆行列で
            //光源の位置をトランスフォームする
            lightPosition = Vector3.Transform(lightPosition, Matrix.Invert(lightRotation));

            //光源のビュー行列を作成
            Matrix lightView = Matrix.CreateLookAt(lightPosition,
                 lightPosition - light.direction,
                 Vector3.Up);

            //光源の射影行列を作成する
            //指向性光源を使用しているので、射影は正射影
            Matrix lightProjection = Matrix.CreateOrthographic(boxSize.X, boxSize.Y,
                -boxSize.Z, boxSize.Z);

            return lightView * lightProjection;
        }

        /// <summary>
        /// 浮動小数点レンダリングターゲットにシーンを描画してから、
        /// シーンを描画するときに使用するテクスチャを設定する
        /// </summary>
        private void CreateShadowMap()
        {
            GraphicsDevice.SetRenderTarget(shadowRenderTarget);
            GraphicsDevice.Clear(Color.White);

            sphereModel.Draw(mainCamera.Camera, light, isToonRendering);
            cylinderModel.Draw(mainCamera.Camera, light, isToonRendering);

            //レンダーターゲットを再びバックバッファーに設定する
            GraphicsDevice.SetRenderTarget(null);
        }
    }
}