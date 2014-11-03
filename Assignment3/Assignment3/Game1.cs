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
        int maze_min_x;
        int maze_max_z;
        const int WALL_MIN_X = -40;
        const int WALL_MAX_Z = 40;
        const int WALL_WIDTH = 80;
        float camRotX;
        float camRotY;
        float camRotZ;
        int camX;
        int camY;
        int camZ;

        Matrix projection;
        Matrix view;
        BoundingBox[,] mazeBoxes;
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
            maze_max_z = WALL_MAX_Z * MAZE_Y;
            maze_min_x = WALL_MIN_X * MAZE_X;
            MazeGenerator m = new MazeGenerator(MAZE_X, MAZE_Y);
            m.CreateMaze();
            mazeLayout = m.ToIntMap();

            // TODO: Add your initialization logic here

            //            mazeLayout = new int[MAZE_X, MAZE_Y]
            //               {{1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, 
            //                {1, 1, 0, 1, 1, 0, 1, 0, 1, 1},
            //                {1, 0, 1, 0, 1, 0, 0, 0, 0, 1},
            //                {1, 0, 0, 0, 1, 0, 1, 0, 0, 1},
            //                {1, 0, 1, 0, 0, 0, 1, 0, 0, 1},
            //                {1, 0, 1, 0, 1, 0, 1, 1, 0, 1},
            //                {1, 0, 1, 0, 1, 0, 1, 0, 0, 1},
            //                {1, 0, 1, 0, 0, 1, 1, 0, 1, 1},
            //                {1, 0, 1, 0, 1, 0, 1, 0, 0, 1},
            //                {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}};
            mazeBoxes = new BoundingBox[MAZE_X, MAZE_Y];

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
            view = Matrix.CreateTranslation(camX, camY, camZ);
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
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                camZ++;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
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

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                camRotY += 0.5f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                camRotY -= 0.5f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                camRotX += 0.5f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                camRotX -= 0.5f; ;
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

            base.Draw(gameTime);
        }

        private void BuildMaze(Model w, Texture2D wt, int[,] maze)
        {
            for (int x = 0; x < MAZE_X; x++)
            {
                for (int y = 0; y < MAZE_Y; y++)
                {
                    if (maze[x, y] == 1)
                    {
                        mazeBoxes[x, y] = CalBoundingBox(w, new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * x), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * y)));
                    }
                }
            }
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
                        DrawModel(w, wt, new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * x), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * y)));
                    }
                }
            }
        }

        private void DrawModel(Model m, Texture2D t, Vector3 pos)
        {
            Matrix[] transforms = new Matrix[m.Bones.Count];
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            m.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                aspectRatio, 1.0f, 10000.0f);
            Matrix view =
                //Matrix.CreateRotationX(camRotX) 
                //* Matrix.CreateRotationY(camRotY) 
                //* Matrix.CreateRotationZ(camRotZ) 
                //* Matrix.CreateTranslation(camX, camY, camZ); 
                Matrix.CreateLookAt(new Vector3(camRotX, camRotY, camRotZ),
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

        private BoundingBox CalBoundingBox(Model mod, Vector3 worldPos)
        {
            List<Vector3> points = new List<Vector3>();
            BoundingBox box;

            Matrix[] boneTransforms = new Matrix[mod.Bones.Count];
            mod.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in mod.Meshes)
            {
                foreach (ModelMeshPart mmp in mesh.MeshParts)
                {
                    VertexPositionNormalTexture[] vertices =
                        new VertexPositionNormalTexture[mmp.VertexBuffer.VertexCount];

                    mmp.VertexBuffer.GetData<VertexPositionNormalTexture>(vertices);

                    foreach (VertexPositionNormalTexture vertex in vertices)
                    {
                        Vector3 point = Vector3.Transform(vertex.Position,
                            boneTransforms[mesh.ParentBone.Index]);

                        Matrix mat = Matrix.CreateTranslation(worldPos);
                        point = Vector3.Transform(point, mat);

                        points.Add(point);
                    }
                }
            }
            box = BoundingBox.CreateFromPoints(points);
            return box;
        }
    }
}
