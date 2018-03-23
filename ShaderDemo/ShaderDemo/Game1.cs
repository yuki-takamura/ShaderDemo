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
        Effect postEffect;

        Light light;

        Texture2D sketchTexture;
        Texture2D noiseTexture;

        int switching = 0;
        const int max = 5;

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
            List<Texture2D> sphereTextures = LoadTexture("DiffuseMap", "NormalMap");
            Transform transform = new Transform(Vector3.UnitY, Vector3.Zero, Vector3.One * 0.5f);
            sphereModel.Initialize(model, effect, sphereTextures, transform);

            model = Content.Load<Model>("Plane");
            List<Texture2D> planeTextures = LoadTexture("Tile", "Tile_N");
            transform = new Transform(Vector3.Zero, Vector3.Zero, Vector3.One * 5);
            planeModel.Initialize(model, effect, planeTextures, transform);

            model = Content.Load<Model>("Cylinder");
            List<Texture2D> cylinderTextures = LoadTexture("GrassGreen", "NormalMap");
            transform = new Transform(new Vector3(-2, 1, 1), Vector3.Zero, Vector3.One * 0.5f);
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
            }

            mainCamera.Update(gameTime);

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

            sphereModel.Draw(mainCamera.Camera, light, shadowRenderTarget, false);
            planeModel.Draw(mainCamera.Camera, light, shadowRenderTarget, false);
            cylinderModel.Draw(mainCamera.Camera, light, shadowRenderTarget, false);
            skyBoxModel.Draw(mainCamera.Camera, light, shadowRenderTarget, false);

            GraphicsDevice.SetRenderTarget(null);
            //GraphicsDevice.Clear(Color.CadetBlue);
            GraphicsDevice.Clear(Color.DimGray);

            switch (switching)
            {
                case 0:
                    SetPostEffect("Sketch");
                    break;
                case 1:
                    SetPostEffect("Flip");
                    break;
                case 2:
                    SetPostEffect("Mono");
                    break;
                case 3:
                    SetPostEffect("Sepia");
                    break;
                case 4:
                    SetPostEffect("Noise");
                    break;
                case 5:
                    SetPostEffect();
                    break;
                default:
                    SetPostEffect();
                    break;
            }

            DrawShadowMapToScreen();

            base.Draw(gameTime);
        }

        private void SetPostEffect(string techniqueName = null)
        {
            if (techniqueName != null)
            {
                EffectParameter weightsParameter, offsetsParameter;
                weightsParameter = postEffect.Parameters["SampleWeights"];
                offsetsParameter = postEffect.Parameters["SampleOffsets"];

                postEffect.Parameters["SamplerSize"].SetValue(new Vector2(postEffectRenderTarget.Width, postEffectRenderTarget.Height));
                postEffect.Parameters["SketchThreshold"].SetValue(0.5f);
                postEffect.Parameters["SketchBrightness"].SetValue(0.4f);
                postEffect.Parameters["SketchJitter"].SetValue(0.1f);
                postEffect.Parameters["SketchTexture"].SetValue(sketchTexture);
                postEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);

                SetBlur(weightsParameter, offsetsParameter);
                SetBlur2(weightsParameter, offsetsParameter);

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

        private void SetBlur(EffectParameter weight, EffectParameter offsets)
        {
            int sampleCount = weight.Elements.Count;

            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            float totalWeights = sampleWeights[0];

            for (int i = 0; i < sampleCount / 2; i++)
            {
                float weightGaussian = ComputeGaussian(i + 1);
                sampleWeights[i * 2 + 1] = weightGaussian;
                sampleWeights[i * 2 + 2] = weightGaussian;
                totalWeights += weightGaussian * 2;

                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(0, 1 / 900) * sampleOffset;

                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            weight.SetValue(sampleWeights);
            offsets.SetValue(sampleOffsets);
        }

        private void SetBlur2(EffectParameter weight, EffectParameter offsets)
        {
            int sampleCount = weight.Elements.Count;

            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            float totalWeights = sampleWeights[0];

            for (int i = 0; i < sampleCount / 2; i++)
            {
                float weightGaussian = ComputeGaussian(i + 1);
                sampleWeights[i * 2 + 1] = weightGaussian;
                sampleWeights[i * 2 + 2] = weightGaussian;
                totalWeights += weightGaussian * 2;

                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(1 / 1600, 0) * sampleOffset;

                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            weight.SetValue(sampleWeights);
            offsets.SetValue(sampleOffsets);
        }

        private float ComputeGaussian(float n)
        {
            float theta = 8;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                            Math.Exp(-(n * n) / (2 * theta * theta)));
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

            sphereModel.Draw(mainCamera.Camera, light);
            cylinderModel.Draw(mainCamera.Camera, light);

            //レンダーターゲットを再びバックバッファーに設定する
            GraphicsDevice.SetRenderTarget(null);
        }

        void DrawShadowMapToScreen()
        {
            spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(shadowRenderTarget, new Rectangle(20, 20, 128, 128), Color.White);
            spriteBatch.End();

            GraphicsDevice.Textures[0] = null;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }
    }
}