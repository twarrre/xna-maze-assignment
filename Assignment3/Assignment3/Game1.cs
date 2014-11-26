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
using SkinnedModel;

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
        Effect customeffect;
        Effect customEffectAnimation;
        TimeSpan extraMoveTime;

        Model floor;
        Model ceiling;
        Model wall;
        Model home;

        Texture2D wallDiffuse;
        Texture2D ceilingDiffuse;
        Texture2D floorDiffuse;
        Texture2D homeDiffuse;

        Matrix gameWorldRotation = Matrix.Identity;
        const int MAZE_X = 10;
        const int MAZE_Y = 10;
        int maze_min_x;
        int maze_max_z;
        int prevCycleDir;
        int openPaths;
        Vector3 distBetween;
        Boolean directionNorth;
        Boolean prevDirectionNorth;
        const int WALL_MIN_X = -40;
        const int WALL_MAX_Z = 40;
        const int WALL_WIDTH = 80;
        BoundingBox[,] mazeBoxes;
        int[,] mazeLayout;
        BoundingSphere camBox;
        BoundingSphere chickenSphere;
        Boolean collided;
        Boolean chickCollided;

        FirstPersonCamera camera;
        Vector3 startingPosition;
        Vector3 chickenPosition;
        Vector3 prevCamPosition;
        Vector3 heading;
        Vector3 prevHeading;
        Vector3 target;
        Vector3 prevTarget;
        Vector3 prevDistBetween;
        Vector3 viewVector;

        Boolean collisionOn;
        Boolean fogOn;
        Boolean day;

        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;
        private GamePadState previousGamePadState;
        private GamePadState currentGamePadState;

        Model chickenModel;
        Texture2D chickenDiffuse;
        AnimationPlayer chickenAnimationPlayer;

        SoundEffectInstance bounce;
        SoundEffectInstance walk;
        Song daySong;
        Song nightSong;
        Song currentSong;
        float volume = 1.0f;

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

            //mazeLayout = new int[,]{{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            //                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            //                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            //                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            //                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            //                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            //                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            //                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            //                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            //                        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}};

            collided = false;
            chickCollided = false;
            collisionOn = true;
            fogOn = false;
            day = true;
            openPaths = 0;
            heading = new Vector3(0, 0, 1);
            prevHeading = heading;
            mazeBoxes = new BoundingBox[MAZE_X, MAZE_Y];
            startingPosition = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * 1), 50, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * (MAZE_Y - 2)));
            chickenPosition = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * m.FurthestPoint.X), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * m.FurthestPoint.Y));
            camera.Position = startingPosition;
            camera.Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(180)));
            camBox = new BoundingSphere(camera.Position, 4);
            chickenSphere = new BoundingSphere(chickenPosition, 50);
            extraMoveTime = new TimeSpan(0, 0, 1);
            distBetween = new Vector3();

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

            customeffect = Content.Load<Effect>(@"Effects\Ambient");
            customEffectAnimation = Content.Load<Effect>(@"Effects\AmbientAnim");
            ceiling = Content.Load<Model>(@"Model\ceiling");
            floor = Content.Load<Model>(@"Model\floor");
            wall = Content.Load<Model>(@"Model\wall");
            home = Content.Load<Model>(@"Model\home");
            chickenModel = Content.Load<Model>(@"Model\chicken_animv2");

            wallDiffuse = Content.Load<Texture2D>(@"Texture\walltexture");
            ceilingDiffuse = Content.Load<Texture2D>(@"Texture\stone");
            floorDiffuse = Content.Load<Texture2D>(@"Texture\pavers1d2");
            homeDiffuse = Content.Load<Texture2D>(@"Texture\home");
            chickenDiffuse = Content.Load<Texture2D>(@"Texture\chicken_diffuse");

            bounce = Content.Load<SoundEffect>(@"Audio\bounce").CreateInstance();
            walk = Content.Load<SoundEffect>(@"Audio\walk").CreateInstance();
            camera.Walk = walk;
            daySong = Content.Load<Song>(@"Audio\day");
            nightSong = Content.Load<Song>(@"Audio\night");

            effect.AmbientLightColor = new Vector3(1f, 1f, 1f);
            effect.DiffuseColor = new Vector3(0.1f, 0.1f, 0.1f);
            effect.Projection = camera.ProjectionMatrix;
            effect.View = camera.ViewMatrix;
            effect.LightingEnabled = true;

            customeffect.Parameters["FogColor"].SetValue(Color.White.ToVector4());
            camera.setClippingFar(1000.0f);
            customeffect.Parameters["FarPlane"].SetValue(camera.getClippingFar());
            customeffect.Parameters["DiffuseLightRadius"].SetValue(0.8f);
            customeffect.Parameters["DiffuseLightAngleCosine"].SetValue(0.6f);
            customeffect.Parameters["DiffuseLightDecayExponent"].SetValue(20);
            customeffect.Parameters["DiffuseIntensity"].SetValue(1.0f);
            customeffect.Parameters["DaylightIntensity"].SetValue(1.5f);

            customEffectAnimation.Parameters["FogColor"].SetValue(Color.White.ToVector4());
            customEffectAnimation.Parameters["FarPlane"].SetValue(camera.getClippingFar());
            customEffectAnimation.Parameters["DiffuseLightRadius"].SetValue(0.8f);
            customEffectAnimation.Parameters["DiffuseLightAngleCosine"].SetValue(0.6f);
            customEffectAnimation.Parameters["DiffuseLightDecayExponent"].SetValue(20);
            customEffectAnimation.Parameters["DiffuseIntensity"].SetValue(1.0f);
            customEffectAnimation.Parameters["DaylightIntensity"].SetValue(1.5f);

            BuildMaze(wall, wallDiffuse, mazeLayout);

            viewVector = Vector3.Transform(camera.ViewDirection - camera.Position, Matrix.CreateRotationY(0));
            viewVector.Normalize();

            SkinningData skinningData = chickenModel.Tag as SkinningData;
            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            chickenAnimationPlayer = new AnimationPlayer(skinningData);
            AnimationClip clip = skinningData.AnimationClips["Take 001"];
            chickenAnimationPlayer.StartClip(clip);

            MediaPlayer.Play(daySong);
            MediaPlayer.IsRepeating = true;
            currentSong = daySong;


            chickenPosition = camera.Position;
            chickenPosition.Y = 0;
            target = chickenPosition;
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
            int chickX = (int)Math.Round(Math.Abs(-((chickenPosition.X - maze_min_x + WALL_MIN_X) / WALL_WIDTH)));
            int chickZ = (int)Math.Round(Math.Abs((chickenPosition.Z - maze_max_z + WALL_MAX_Z) / WALL_WIDTH));
            int cycleDir = 0;
            prevDistBetween = distBetween;

            previousGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            customeffect.Parameters["DiffuseLightDirection"].SetValue(camera.ViewDirection);
            customeffect.Parameters["DiffusePosition"].SetValue(camera.Position);

            customEffectAnimation.Parameters["DiffuseLightDirection"].SetValue(camera.ViewDirection);
            customEffectAnimation.Parameters["DiffusePosition"].SetValue(camera.Position);

            collided = false;

            float distance;
            Vector3 c = camera.Position;
            Vector3.Distance(ref chickenPosition, ref c, out distance);
            volume = (1.0f / distance) * 100;

            if (fogOn)
                volume /= 2;

            volume = MathHelper.Clamp(volume, 0.0f, 1.0f);
            MediaPlayer.Volume = volume;

            // Allows the game to exit
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed
                || currentKeyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            if (currentGamePadState.Buttons.Start == ButtonState.Pressed
                || currentKeyboardState.IsKeyDown(Keys.Home))
            {
                camera.Position = startingPosition;
                fogOn = false;
                collisionOn = true;
                camera.resetZoom();
            }

            if (((previousGamePadState.Buttons.Y == ButtonState.Released) && (currentGamePadState.Buttons.Y == ButtonState.Pressed))
                || ((currentKeyboardState.IsKeyDown(Keys.W) && previousKeyboardState.IsKeyUp(Keys.W))))
            {
                if ((camera.Position.X < (MAZE_X * WALL_WIDTH / 2)) && (camera.Position.Z < (MAZE_Y * WALL_WIDTH / 2))
                    && (camera.Position.X > -(MAZE_X * WALL_WIDTH / 2)) && (camera.Position.Z > -(MAZE_X * WALL_WIDTH / 2)))
                {
                    collisionOn = !collisionOn;
                }
            }

            if (((previousGamePadState.Buttons.X == ButtonState.Released) && (currentGamePadState.Buttons.X == ButtonState.Pressed))
                || ((currentKeyboardState.IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F))))
            {
                fogOn = !fogOn;
            }

            if (((previousGamePadState.Buttons.RightShoulder == ButtonState.Released) && (currentGamePadState.Buttons.RightShoulder == ButtonState.Pressed))
                || ((currentKeyboardState.IsKeyDown(Keys.D) && previousKeyboardState.IsKeyUp(Keys.D))))
            {
                day = !day;

                if (day)
                {
                    bool pause = false; ;
                    if (MediaPlayer.State == MediaState.Paused)
                        pause = true;

                    MediaPlayer.Play(daySong);
                    currentSong = daySong;
                    
                    if(pause)
                        MediaPlayer.Pause();
                }
                else
                {
                    bool pause = false; ;
                    if (MediaPlayer.State == MediaState.Paused)
                        pause = true;

                    MediaPlayer.Play(nightSong);
                    currentSong = nightSong;
                   
                    if (pause)
                        MediaPlayer.Pause();
                }
            }

            if (((previousGamePadState.Buttons.RightStick == ButtonState.Released) && (currentGamePadState.Buttons.RightStick == ButtonState.Pressed))
                || ((currentKeyboardState.IsKeyDown(Keys.M) && previousKeyboardState.IsKeyUp(Keys.M))))
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Pause();
                }
                else
                {
                    MediaPlayer.Resume();
                }
            }

            if (day)
            {
                customeffect.Parameters["DayEnabled"].SetValue(true);
                customeffect.Parameters["DiffuseLightDecayExponent"].SetValue(0.5f);
                customEffectAnimation.Parameters["DayEnabled"].SetValue(true);
                customEffectAnimation.Parameters["DiffuseLightDecayExponent"].SetValue(0.5f);
            }
            else
            {
                customeffect.Parameters["DayEnabled"].SetValue(false);
                customeffect.Parameters["DiffuseLightDecayExponent"].SetValue(20);
                customEffectAnimation.Parameters["DayEnabled"].SetValue(false);
                customEffectAnimation.Parameters["DiffuseLightDecayExponent"].SetValue(20);
            }

            if (fogOn)
            {
                camera.setClippingFar(300.0f /*MAZE_X * WALL_WIDTH*/);
                customeffect.Parameters["FarPlane"].SetValue(camera.getClippingFar());
                customEffectAnimation.Parameters["FarPlane"].SetValue(camera.getClippingFar());
            }
            else
            {
                camera.setClippingFar(1000.0f);
                customeffect.Parameters["FarPlane"].SetValue(camera.getClippingFar());
                customEffectAnimation.Parameters["FarPlane"].SetValue(camera.getClippingFar());
            }

            if (collisionOn)
            {
                for (int i = 0; i < MAZE_X; i++)
                {
                    for (int j = 0; j < MAZE_Y; j++)
                    {
                        CollisionCheck(UpdateBox(camBox), mazeBoxes[i, j], i, j);
                    }
                }
            }

            if (!collided)
            {
                prevCamPosition = camera.Position;
            }

            chickenAnimationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.CreateTranslation(chickenPosition));

            if (chickenPosition == target)
            {
                Random rand = new Random();
                cycleDir = rand.Next();
                if ((cycleDir % 2 == 0) && (prevCycleDir % 2 == 0))
                {
                    if ((mazeLayout[chickX, chickZ + 1] == 0) && (chickZ + 1 < MAZE_Y))
                    {
                        openPaths++;
                        target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * chickX), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * (chickZ + 1)));
                    }
                    else if ((mazeLayout[chickX + 1, chickZ] == 0) && (chickX + 1 < MAZE_X))
                    {
                        openPaths++;
                        target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * (chickX + 1)), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * chickZ));
                    }
                    else if ((mazeLayout[chickX - 1, chickZ] == 0) && (chickX - 1 > 0))
                    {
                        openPaths++;
                        target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * (chickX - 1)), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * chickZ));
                    }
                    else if ((mazeLayout[chickX, chickZ - 1] == 0) && (chickZ - 1 > 0))
                    {
                        openPaths++;
                        target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * chickX), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * (chickZ - 1)));
                    }
                }
                else
                {
                    if ((mazeLayout[chickX - 1, chickZ] == 0) && (chickX - 1 > 0))
                    {
                        target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * (chickX - 1)), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * chickZ));
                    }
                    else if ((mazeLayout[chickX, chickZ - 1] == 0) && (chickZ - 1 > 0))
                    {
                        target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * chickX), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * (chickZ - 1)));
                    }
                    else if ((mazeLayout[chickX, chickZ + 1] == 0) && (chickZ + 1 < MAZE_Y))
                    {
                        target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * chickX), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * (chickZ + 1)));
                    }
                    else if ((mazeLayout[chickX + 1, chickZ] == 0) && (chickX + 1 < MAZE_X))
                    {
                        target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * (chickX + 1)), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * chickZ));
                    }
                }
                if (prevDistBetween == -(distBetween) && openPaths > 1)
                {
                    target = chickenPosition;
                    distBetween.Y = 1;
                }
                else
                {
                    distBetween = new Vector3(chickenPosition.X - target.X, chickenPosition.Y - target.Y, chickenPosition.Z - target.Z);
                }
            }

            if (distBetween.X > 0)
            {
                chickenPosition.X -= 1;
                distBetween.X -= 1;
            }
            if (distBetween.X < 0)
            {
                chickenPosition.X += 1;
                distBetween.X += 1;
            }
            if (distBetween.Z < 0)
            {
                chickenPosition.Z += 1;
                distBetween.Z += 1;
            }
            if (distBetween.Z > 0)
            {
                chickenPosition.Z -= 1;
                distBetween.Z -= 1;
            }

            //if (directionNorth == prevDirectionNorth)
            //{
            //    target = new Vector3();
            //}

            prevCycleDir = cycleDir;
            prevTarget = target;
            prevDirectionNorth = directionNorth;


            Console.WriteLine("X: " + chickenPosition.X + ", Y: " + chickenPosition.Y + ", Z: " + chickenPosition.Z);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.White);

            DrawChicken(chickenModel, chickenDiffuse, chickenPosition);
     
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
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = customeffect;
                    customeffect.Parameters["World"].SetValue(gameWorldRotation * transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(pos));
                    customeffect.Parameters["View"].SetValue(camera.ViewMatrix);
                    customeffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(transforms[mesh.ParentBone.Index] * gameWorldRotation));
                    customeffect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);
                    customeffect.Parameters["ViewVector"].SetValue(viewVector);
                    customeffect.Parameters["ModelTexture"].SetValue(t);
                    customeffect.Parameters["FogEnabled"].SetValue(fogOn);
                }
                mesh.Draw();
            }
        }

        private void DrawChicken(Model m, Texture2D t, Vector3 pos)
        {
            Matrix[] groundMatrix = new Matrix[chickenModel.Bones.Count];
            chickenModel.CopyAbsoluteBoneTransformsTo(groundMatrix);

            Matrix[] bones = chickenAnimationPlayer.GetSkinTransforms();

            foreach (ModelMesh mm in chickenModel.Meshes)
            {
                foreach (ModelMeshPart mmp in mm.MeshParts)
                {
                    mmp.Effect = customEffectAnimation;
                    customEffectAnimation.Parameters["World"].SetValue(groundMatrix[mm.ParentBone.Index] * Matrix.Identity * Matrix.CreateTranslation(pos));
                    customEffectAnimation.Parameters["View"].SetValue(camera.ViewMatrix);
                    customEffectAnimation.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(groundMatrix[mm.ParentBone.Index] * Matrix.Identity * Matrix.CreateTranslation(pos)));
                    customEffectAnimation.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);
                    customEffectAnimation.Parameters["ViewVector"].SetValue(viewVector);
                    customEffectAnimation.Parameters["ModelTexture"].SetValue(t);
                    customEffectAnimation.Parameters["FogEnabled"].SetValue(fogOn);
                    customEffectAnimation.Parameters["Bones"].SetValue(bones);
                }
                mm.Draw();
            }
            chickenSphere.Center = pos;
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
            return box;
        }

        private void CollisionCheck(BoundingSphere cam, BoundingBox wall, int x, int z)
        {
            if (cam.Intersects(wall))
            {
                if (bounce.State != SoundState.Playing)
                    bounce.Play();

                collided = true;
                Vector3 pos = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * x), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * z));
                int camX = (int)Math.Round(Math.Abs(-((camera.Position.X - maze_min_x + WALL_MIN_X) / WALL_WIDTH)));
                int camZ = (int)Math.Round(Math.Abs((camera.Position.Z - maze_max_z + WALL_MAX_Z) / WALL_WIDTH));

                Vector3 newVector = camera.Position;

                if (x == camX)
                {
                    if (camZ > z)
                    {
                        newVector.Z = pos.Z - 40 - 4;
                    }
                    else if (camZ < z)
                    {
                        newVector.Z = pos.Z + 40 + 4;
                    }
                }

                if (z == camZ)
                {
                    if (camX < x)
                    {
                        newVector.X = pos.X - 40 - 4;
                    }
                    else if (camX > x)
                    {
                        newVector.X = pos.X + 40 + 4;
                    }
                }
                camera.Position = newVector;
            }
        }

        private Vector3 CollisionChickenCheck(BoundingSphere chick, BoundingBox wall, int x, int z)
        {
            Vector3 dir = new Vector3(0, 0, 0);
            Vector3 pos = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * x), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * z));
            int chickX = (int)Math.Round(Math.Abs(-((chickenPosition.X - maze_min_x + WALL_MIN_X) / WALL_WIDTH)));
            int chickZ = (int)Math.Round(Math.Abs((chickenPosition.Z - maze_max_z + WALL_MAX_Z) / WALL_WIDTH));

            if (chick.Intersects(wall))
            {
                chickCollided = true;
                extraMoveTime = new TimeSpan(0, 0, 1);

                if (x == chickX)
                {
                    if (chickZ > z)
                    {
                        dir += new Vector3(1, 0, 0);//1x
                    }
                    else if (chickZ < z)
                    {
                        dir += new Vector3(-1, 0, 0);//-1x
                    }
                }
                else if (z == chickZ)
                {
                    if (chickX < x)
                    {
                        dir += new Vector3(0, 0, -1);//-1z
                    }
                    else if (chickX > x)
                    {
                        dir += new Vector3(0, 0, 1);//1z
                    }
                }
            }
            else
            {
                //extraMoveTime -= gameTime.ElapsedGameTime;
                //if (extraMoveTime > TimeSpan.Zero)
                //{
                //    dir = prevDir;
                //}
                //else
                //{
                //    dir = new Vector3();
                //}
                //if ((0 < chickX) && (chickX < MAZE_X - 1) && (0 < chickZ) && (chickZ < MAZE_Y - 1))
                //{
                //    if (mazeLayout[chickX, chickZ + 1] == 0)
                //    {
                //        dir += new Vector3(0, 0, 2);
                //    }
                //    else if (mazeLayout[chickX + 1, chickZ] == 0)
                //    {
                //        dir += new Vector3(-2, 0, 0);
                //    }
                //    else if (mazeLayout[chickX, chickZ - 1] == 0)
                //    {
                //        dir += new Vector3(0, 0, -2);
                //    }
                //    else if (mazeLayout[chickX - 1, chickZ] == 0)
                //    {
                //        dir += new Vector3(2, 0, 0);
                //    }
                //}
            }
            return dir;
        }

        private void ChickenNavigate()
        {
            chickenPosition = WallCheck(chickenPosition);
        }

        private Vector3 WallCheck(Vector3 chPos)
        {
            int cellX = (int)Math.Round(Math.Abs(-((chPos.X - maze_min_x + WALL_MIN_X) / WALL_WIDTH)));
            int cellZ = (int)Math.Round(Math.Abs((chPos.Z - maze_max_z + WALL_MAX_Z) / WALL_WIDTH));
            Vector3 direction = new Vector3(0, 0, 0);

            if (chickenSphere.Intersects(mazeBoxes[cellX, cellZ + 1]))
            {
                direction += new Vector3(0, 0, -1);
            }
            if (chickenSphere.Intersects(mazeBoxes[cellX + 1, cellZ]))
            {
                direction += new Vector3(-1, 0, 0);
            }
            if (chickenSphere.Intersects(mazeBoxes[cellX, cellZ - 1]))
            {
                direction += new Vector3(0, 0, 1);
            }
            if (chickenSphere.Intersects(mazeBoxes[cellX - 1, cellZ]))
            {
                direction += new Vector3(1, 0, 0);
            }

            //if (cellZ + 1 < MAZE_Y && mazeLayout[cellX, cellZ + 1] == 1)
            //{
            //    direction = new Vector3(1, 0, 0);
            //}
            //if (cellX + 1 < MAZE_X && mazeLayout[cellX + 1, cellZ] == 1)
            //{
            //    direction = new Vector3(0, 0, -1);
            //}
            //if (cellZ - 1 > 0 && mazeLayout[cellX, cellZ - 1] == 1)
            //{
            //    direction = new Vector3(-1, 0, 0);
            //}
            //if (cellX - 1 > 0 && mazeLayout[cellX - 1, cellZ] == 1)
            //{
            //    direction = new Vector3(0, 0, 1);
            //}
            return direction;
        }
    }
}
