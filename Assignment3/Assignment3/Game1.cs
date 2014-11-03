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
        Matrix gameWorldRotation = Matrix.Identity;
        Vector3 Position;
        Model[] walls;
        const int MAZE_X = 10;
        const int MAZE_Y = 10;
        int camX;
        int camY;
        int camZ;
        int wallWidth;

        Matrix projection;
        Matrix view;
        int[,] mazeLayout;

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
            MazeGenerator m = new MazeGenerator(MAZE_X, MAZE_Y);
            m.CreateMaze();
            mazeLayout = m.ToIntMap();

            // TODO: Add your initialization logic here
            //mazeLayout = new int[MAZE_X, MAZE_Y]
            //   {{1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, 
            //    {1, 1, 0, 1, 1, 0, 1, 0, 1, 1},
            //    {1, 0, 1, 0, 1, 0, 0, 0, 0, 1},
            //    {1, 0, 0, 0, 1, 0, 1, 0, 0, 1},
            //    {1, 0, 1, 0, 0, 0, 1, 0, 0, 1},
            //    {1, 0, 1, 0, 1, 0, 1, 1, 0, 1},
            //    {1, 0, 1, 0, 1, 0, 1, 0, 0, 1},
            //    {1, 0, 1, 0, 0, 1, 1, 0, 1, 1},
            //    {1, 0, 1, 0, 1, 0, 1, 0, 0, 1},
            //    {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}};
            wallWidth = 8;
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // TODO: Add your update logic here
            if(Keyboard.GetState().IsKeyDown(Keys.W))
            {
                camZ++;
            }

            if(Keyboard.GetState().IsKeyDown(Keys.S))
            {
                camZ--;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                camX++;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                camX--;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                camY--;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
            {
                camY++;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawMaze(wall, wallDiffuse, floor, floorDiffuse, ceiling, ceilingDiffuse, mazeLayout);
            //foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            //{
            //    Matrix[] groundMatrix = new Matrix[wall.Bones.Count];
            //    foreach (ModelMesh m in wall.Meshes)
            //    {
            //        foreach (ModelMeshPart part in m.MeshParts)
            //        {
            //            part.Effect = effect;
            //        }
            //        foreach (BasicEffect be in m.Effects)
            //        {
            //            be.TextureEnabled = true;
            //            be.Texture = wallDiffuse;

            //            be.EnableDefaultLighting();
            //            be.World = groundMatrix[m.ParentBone.Index] * Matrix.Identity;
            //            be.View = effect.View;
            //            be.Projection = effect.Projection;
            //            m.Draw();
            //        }
            //        //m.Draw();
            //    }
            //}

            base.Draw(gameTime);
        }

        private void DrawMaze(Model w, Texture2D wt, Model f, Texture2D ft, Model c, Texture2D ct, int[,] maze)
        {
            DrawModel(f, ft, new Vector3(0, 0, 0));
            DrawModel(c, ct, new Vector3(0, 0, 0));
            for (int x = 0; x < MAZE_X; x++)
            {
                for (int y = 0; y < MAZE_Y; y++)
                {
                    if (maze[x, y] == 1)
                    {
                        DrawModel(w, wt, new Vector3(-360 - (-80 * x), 0, 360 - (80 * y)));
                    }
                }
            }
        }
        private void CreateWall(Model m, int coordinate)
        {

        }

        private void DrawModel(Model m, Texture2D t, Vector3 pos)
        {
            Matrix[] transforms = new Matrix[m.Bones.Count];
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            m.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                aspectRatio, 1.0f, 10000.0f);
            Matrix view = Matrix.CreateLookAt(new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 0.0f),
                Vector3.Zero, Vector3.Up) * 
                Matrix.CreateTranslation(camX, camY, camZ); 

            foreach (ModelMesh mesh in m.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.Texture = t;

                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = gameWorldRotation *
                        transforms[mesh.ParentBone.Index] *
                        Matrix.CreateTranslation(pos);
                }
                mesh.Draw();
            }
        }
    }
}
