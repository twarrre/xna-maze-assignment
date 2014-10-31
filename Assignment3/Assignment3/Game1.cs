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

namespace Assignment3
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        BasicEffect effect;

        Model floor;
        Model ceiling;
        Model wall;

        Texture2D wallDiffuse;
        Texture2D ceilingDiffuse;
        Texture2D floorDiffuse;

        Matrix view;
        Matrix projection;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            effect = new BasicEffect(graphics.GraphicsDevice);

            ceiling = Content.Load<Model>(@"Model\ceiling");
            floor = Content.Load<Model>(@"Model\floor");
            wall = Content.Load<Model>(@"Model\wall");

            wallDiffuse = Content.Load<Texture2D>(@"Texture\walltexture");
            ceilingDiffuse = Content.Load<Texture2D>(@"Texture\pavers1d2");
            floorDiffuse = Content.Load<Texture2D>(@"Texture\pavers1d2");

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                (float)this.Window.ClientBounds.Width / (float)this.Window.ClientBounds.Height,
                1.0f, 10.0f);
            effect.Projection = projection;
            view = Matrix.CreateTranslation(0.0f, 0.0f, -10.0f);
            effect.View = view;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            
            foreach(ModelMesh m in wall.Meshes)
            {
                foreach (BasicEffect be in m.Effects)
                {
                    be.TextureEnabled = true;
                    be.Texture = wallDiffuse;

                    be.EnableDefaultLighting();
                    be.World = Matrix.Identity * Matrix.CreateTranslation(0, 0, 50);
                    be.View = view;
                    be.Projection = projection;
                }
            }

            base.Draw(gameTime);
        }
    }
}
