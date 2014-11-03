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
        Model home;

        Texture2D wallDiffuse;
        Texture2D ceilingDiffuse;
        Texture2D floorDiffuse;
        Texture2D homeDiffuse;

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
        BoundingBox[,] mazeBoxes;
        int[,] mazeLayout;
        BoundingSphere camBox;
        Boolean collided;
        
        FirstPersonCamera camera;
        Vector3 startingPosition;
        Vector3 prevCamPosition;

        Boolean collisionOn;

        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;
        private GamePadState previousGamePadState;
        private GamePadState currentGamePadState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            camera = new FirstPersonCamera(this);
            Components.Add(camera);
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
            collided = false;
            collisionOn = true;

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
            startingPosition = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * 1), 50, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * (MAZE_Y - 2)));
            camera.Position = startingPosition;
            camera.Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(180)));
            camBox = new BoundingSphere(camera.Position, 4);

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

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
            home = Content.Load<Model>(@"Model\home");

            wallDiffuse = Content.Load<Texture2D>(@"Texture\walltexture");
            ceilingDiffuse = Content.Load<Texture2D>(@"Texture\pavers1d2");
            floorDiffuse = Content.Load<Texture2D>(@"Texture\pavers1d2");
            homeDiffuse = Content.Load<Texture2D>(@"Texture\home");

            effect.AmbientLightColor = new Vector3(1f, 1f, 1f);
            effect.DiffuseColor = new Vector3(0.1f, 0.1f, 0.1f);
            effect.Projection = camera.ProjectionMatrix;
            effect.View = camera.ViewMatrix;
            effect.LightingEnabled = true;

            BuildMaze(wall, wallDiffuse, mazeLayout);
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
            previousGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            collided = false;

            // Allows the game to exit
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed
                || currentKeyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            if (currentGamePadState.Buttons.Start == ButtonState.Pressed
                || currentKeyboardState.IsKeyDown(Keys.Home))
            {
                camera.Position = startingPosition;
                camera.Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(180)));
            }

            if (((previousGamePadState.Buttons.Y == ButtonState.Released) && (currentGamePadState.Buttons.Y == ButtonState.Pressed))
                || ((currentKeyboardState.IsKeyDown(Keys.W) && previousKeyboardState.IsKeyUp(Keys.W))))
            {
                collisionOn = !collisionOn;
            }


            if (collisionOn)
            {
                for (int i = 0; i < MAZE_X; i++)
                {
                    for (int j = 0; j < MAZE_Y; j++)
                    {
                        CollisionCheck(UpdateBox(camBox), mazeBoxes[i, j]);
                    }
                }
            }

            if (!collided)
            {
                prevCamPosition = camera.Position;
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

            DrawMaze(wall, wallDiffuse, floor, floorDiffuse, ceiling, ceilingDiffuse, home, homeDiffuse, mazeLayout);

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

        private void DrawMaze(Model w, Texture2D wt, Model f, Texture2D ft, Model c, Texture2D ct, Model h, Texture2D ht, int[,] maze)
        {
            DrawModel(f, ft, new Vector3(0, 0, 0));
            DrawModel(c, ct, new Vector3(0, 0, 0));
            DrawModel(h, ht, startingPosition - new Vector3(0, 49, 0));
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
            m.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in m.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.Texture = t;

                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
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

        private BoundingSphere UpdateBox(BoundingSphere box)
        {
            box.Center = camera.Position;
            //Vector3[] boxCorners = box.GetCorners();
            //for (int i = 0; i < boxCorners.Length; i++)
            //{
            //    boxCorners[i].X += camera.Position.X;
            //    boxCorners[i].Y += camera.Position.Y;
            //    boxCorners[i].Z += camera.Position.Z;
            //}

            //BoundingBox newbox = BoundingBox.CreateFromPoints(boxCorners);
            return box;
        }

        private void CollisionCheck(BoundingSphere cam, BoundingBox wall)
        {
            if (cam.Intersects(wall))
            {
                collided = true;
                camera.Position = prevCamPosition;
            }            
        }
    }
}
