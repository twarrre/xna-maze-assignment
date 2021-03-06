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
        Vector3 distBetween;
        const int WALL_MIN_X = -40;
        const int WALL_MAX_Z = 40;
        const int WALL_WIDTH = 80;
        BoundingBox[,] mazeBoxes;
        int[,] mazeLayout;
        BoundingSphere camBox;
        BoundingSphere chickenSphere;
        Boolean collided;

        FirstPersonCamera camera;
        Vector3 startingPosition;
        Vector3 chickenPosition;
        Vector3 prevCamPosition;
        Vector3 heading;
        Vector3 prevHeading;
        Vector3 target;
        Vector3 viewVector;

        Boolean collisionOn;
        Boolean fogOn;
        Boolean day;
        float chickenRot;

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

            collided = false;
            collisionOn = true;
            fogOn = false;
            day = true;
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
            chickenRot = 0.0f;

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
                collisionOn = !collisionOn;
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
                camera.setClippingFar(300.0f);
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

            if (chickenPosition.X == target.X && chickenPosition.Z == target.Z)
            {
                Random r = new Random();
                bool newDirection = false;
                while (!newDirection)
                {
                    int dir = r.Next(4);

                    switch (dir)
                    {
                        case 0:
                            if (chickX + 1 < MAZE_X && mazeLayout[chickX + 1, chickZ] == 0)
                            {
                                newDirection = true;
                                heading = new Vector3(1, 0, 0);
                                target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * (chickX + 1)), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * chickZ));
                                chickenRot = 90;
                            }
                            break;
                        case 1:
                            if (chickZ + 1 < MAZE_Y && mazeLayout[chickX, chickZ + 1] == 0)
                            {
                                newDirection = true;
                                heading = new Vector3(0, 0, -1);
                                target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * chickX), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * (chickZ + 1)));
                                chickenRot = 180;
                            }
                            break;
                        case 2:
                            if (chickX - 1 >= 0 && mazeLayout[chickX - 1, chickZ] == 0)
                            {
                                newDirection = true;
                                heading = new Vector3(-1, 0, 0);
                                target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * (chickX - 1)), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * chickZ));
                                chickenRot = 270;
                            }
                            break;
                        case 3:
                            if (chickZ - 1 >= 0 && mazeLayout[chickX, chickZ - 1] == 0)
                            {
                                newDirection = true;
                                heading = new Vector3(0, 0, 1);
                                target = new Vector3(maze_min_x - WALL_MIN_X - (-WALL_WIDTH * chickX), 0, maze_max_z - WALL_MAX_Z - (WALL_WIDTH * (chickZ - 1)));
                                chickenRot = 0;
                            }
                            break;
                    }
                }
            }
            chickenPosition += heading;

            chickenAnimationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.CreateRotationY(MathHelper.ToRadians(chickenRot)) * Matrix.CreateTranslation(chickenPosition));

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
                    customEffectAnimation.Parameters["World"].SetValue(groundMatrix[mm.ParentBone.Index]);
                    customEffectAnimation.Parameters["View"].SetValue(camera.ViewMatrix);
                    customEffectAnimation.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(groundMatrix[mm.ParentBone.Index]));
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
    }
}
