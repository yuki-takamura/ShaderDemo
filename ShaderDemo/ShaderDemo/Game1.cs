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

        /// <summary>
        /// �V���h�E�}�b�v�̃����_�[�^�[�Q�b�g
        /// </summary>
        RenderTarget2D shadowRenderTarget;
        const int shadowMapResolution = 2048;

        //Vector3 lightDir = new Vector3(-0.3333333f, 0.6666667f, 0.6666667f);
        //public Matrix lightViewProjection;
        Light light;

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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Model model = Content.Load<Model>("Sphere");
            Effect effect = Content.Load<Effect>("DrawModel");
            List<Texture2D> sphereTextures = LoadTexture("DiffuseMap", "NormalMap");
            Transform transform = new Transform(Vector3.UnitY * 2, Vector3.Zero, Vector3.One * 0.5f);
            sphereModel.Initialize(model, effect, sphereTextures, transform);

            model = Content.Load<Model>("Plane");
            List<Texture2D> planeTextures = LoadTexture("Tile", "Tile_N");
            transform = new Transform(Vector3.Zero, Vector3.Zero, Vector3.One * 5);
            planeModel.Initialize(model, effect, planeTextures, transform);

            shadowRenderTarget = new RenderTarget2D(graphics.GraphicsDevice,
                shadowMapResolution,
                shadowMapResolution,
                false,
                SurfaceFormat.Single,
                DepthFormat.Depth24);

            light.dir = new Vector3(-0.3333333f, 0.6666667f, 0.6666667f);
        }

        /// <summary>
        /// �}�e���A���e�N�X�`����ǂݎ��AList���쐬����
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
            light.viewProjection = CreateLightViewProjectionMatrix();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            CreateShadowMap();

            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

            sphereModel.Draw(mainCamera.Camera, light, shadowRenderTarget, false);
            planeModel.Draw(mainCamera.Camera, light, shadowRenderTarget, false);

            DrawShadowMapToScreen();

            base.Draw(gameTime);
        }

        Matrix CreateLightViewProjectionMatrix()
        {
            Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero,
                -light.dir,
                Vector3.Up);

            //����̃R�[�i�[���擾
            Vector3[] frustumCorners = cameraFrustum.GetCorners();

            for (int i = 0; i < frustumCorners.Length; i++)
            {
                frustumCorners[i] = Vector3.Transform(frustumCorners[i], lightRotation);
            }

            //�|�C���g�̎���̍ŏ��{�b�N�X��������
            BoundingBox lightBox = BoundingBox.CreateFromPoints(frustumCorners);

            Vector3 boxSize = lightBox.Max - lightBox.Min;
            Vector3 halfBoxSize = boxSize * 0.5f;

            //�����̈ʒu�̓{�b�N�X�̃o�b�N�p�l���̒��S�ɂ����K�v������
            Vector3 lightPosition = lightBox.Min + halfBoxSize;
            lightPosition.Z = lightBox.Min.Z;

            //���[���h���W�Ɉʒu��߂��K�v������̂ŁA�����̉�]�̋t�s���
            //�����̈ʒu���g�����X�t�H�[������
            lightPosition = Vector3.Transform(lightPosition, Matrix.Invert(lightRotation));

            //�����̃r���[�s����쐬
            Matrix lightView = Matrix.CreateLookAt(lightPosition,
                 lightPosition - light.dir,
                 Vector3.Up);

            //�����̎ˉe�s����쐬����
            //�w�����������g�p���Ă���̂ŁA�ˉe�͐��ˉe
            Matrix lightProjection = Matrix.CreateOrthographic(boxSize.X, boxSize.Y,
                -boxSize.Z, boxSize.Z);

            return lightView * lightProjection;
        }

        /// <summary>
        /// ���������_�����_�����O�^�[�Q�b�g�ɃV�[����`�悵�Ă���A
        /// �V�[����`�悷��Ƃ��Ɏg�p����e�N�X�`����ݒ肷��
        /// </summary>
        private void CreateShadowMap()
        {
            GraphicsDevice.SetRenderTarget(shadowRenderTarget);
            GraphicsDevice.Clear(Color.White);

            sphereModel.Draw(mainCamera.Camera, light);

            //�����_�[�^�[�Q�b�g���Ăуo�b�N�o�b�t�@�[�ɐݒ肷��
            GraphicsDevice.SetRenderTarget(null);
        }

        void DrawShadowMapToScreen()
        {
            spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(shadowRenderTarget, new Rectangle(0, 0, 128, 128), Color.White);
            spriteBatch.End();

            GraphicsDevice.Textures[0] = null;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }
    }
}