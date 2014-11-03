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
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class InputManager
    {
        private MouseState currentMouseState;
        private MouseState previousMouseState;
        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;
        private GamePadState previousGamePadState;
        private GamePadState currentGamePadState;
        private float sensitivity = 10.0f;

        public InputManager()
        {
            currentMouseState = Mouse.GetState();
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public Input Update(Rectangle client)
        {
            Input i = new Input();

            ProcessMouse(ref i, client);
            ProcessController(ref i);
            ProcessKeyboard(ref i);

            return i;
        }

        private void ProcessController(ref Input i)
        {
            previousGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            float x = GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X;
            float y = GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y;

            if (x > 0)
            {
                i.SetRight(x);
            }
            else if (x < 0)
            {
                i.SetLeft(x);
            }

            if (y > 0)
            {
                i.SetForward(y);
            }
            else if (y < 0)
            {
                i.SetBackward(y);
            }

            if (previousGamePadState.IsButtonUp(Buttons.A) && currentGamePadState.IsButtonDown(Buttons.A))
            {
                i.SetJumping(true);
            }

            if (currentGamePadState.IsButtonDown(Buttons.LeftShoulder))
            {
                i.SetCrouching(true);
            }

            if (currentGamePadState.IsButtonDown(Buttons.LeftStick))
            {
                i.SetSprinting(true);
            }

            if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X != 0)
            {
                i.SetViewX(-GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X * sensitivity);
            }

            if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y != 0)
            {
                i.SetViewY(GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y * sensitivity);
            }

            if (currentGamePadState.IsButtonDown(Buttons.LeftStick) && currentGamePadState.IsButtonDown(Buttons.LeftShoulder) && previousGamePadState.IsButtonUp(Buttons.LeftShoulder))
            {
                i.SetSliding(true);
                i.SetCrouching(false);
                i.SetSprinting(false);
            }
        }

        private void ProcessKeyboard(ref Input i)
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.W))
            {
                i.SetForward(1.0f);
            }

            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                i.SetBackward(-1.0f);
            }

            if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                i.SetLeft(-1.0f);
            }

            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                i.SetRight(1.0f);
            }

            if (previousKeyboardState.IsKeyUp(Keys.Space) && currentKeyboardState.IsKeyDown(Keys.Space))
            {
                i.SetJumping(true);
            }

            if (currentKeyboardState.IsKeyDown(Keys.C))
            {
                i.SetCrouching(true);
            }

            if (currentKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                i.SetSprinting(true);
            }

            if (currentKeyboardState.IsKeyDown(Keys.LeftShift) && currentKeyboardState.IsKeyDown(Keys.C) && previousKeyboardState.IsKeyUp(Keys.C))
            {
                i.SetSliding(true);
                i.SetSprinting(false);
                i.SetCrouching(false);
            }
        }

        private void ProcessMouse(ref Input i, Rectangle client)
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            int centerX = client.Width / 2;
            int centerY = client.Height / 2;
            int deltaX = centerX - currentMouseState.X;
            int deltaY = centerY - currentMouseState.Y;

            Mouse.SetPosition(centerX, centerY);

            i.AddViewX((float)deltaX);
            i.AddViewY((float)deltaY);
        }
    }
}
