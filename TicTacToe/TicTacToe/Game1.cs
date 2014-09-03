using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace TicTacToe
{
    public enum TicTacToePlayer { None, PlayerO, PlayerX }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        bool touching;
        bool winnable;

        TicTacToePlayer winner;
        TicTacToePlayer current;
        TicTacToePlayer[,] grid;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D gridTexture;
        Rectangle gridRectangle;

        Texture2D resetButton;
        Rectangle resetButtonPosition;

        Texture2D oPiece;
        Texture2D xPiece;

        Texture2D oWinner;
        Texture2D xWinner;
        Texture2D noWinner;

        Texture2D oTurn;
        Texture2D xTurn;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            TargetElapsedTime = TimeSpan.FromTicks(333333);
        }

        protected override void Initialize()
        {
            Reset();
            base.Initialize();
        }

        private void Reset()
        {
            winnable = true;
            winner = TicTacToePlayer.None;
            grid = new TicTacToePlayer[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    grid[i, j] = TicTacToePlayer.None;
                }
            }
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            gridTexture = Content.Load<Texture2D>("TicTacToe_Grid");
            gridRectangle = new Rectangle(0, 0, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);

            oPiece = Content.Load<Texture2D>("TicTacToe_O");
            xPiece = Content.Load<Texture2D>("TicTacToe_X");

            resetButton = Content.Load<Texture2D>("TicTacToe_Reset");
            resetButtonPosition = new Rectangle(spriteBatch.GraphicsDevice.Viewport.Width / 2 - (resetButton.Width / 2), spriteBatch.GraphicsDevice.Viewport.Height - 95, resetButton.Width, resetButton.Height);

            oWinner = Content.Load<Texture2D>("TicTacToe_O_Winner");
            xWinner = Content.Load<Texture2D>("TicTacToe_X_Winner");
            noWinner = Content.Load<Texture2D>("TicTacToe_Draw");

            oTurn = Content.Load<Texture2D>("TicTacToe_O_Turn");
            xTurn = Content.Load<Texture2D>("TicTacToe_X_Turn");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            TouchCollection touches = TouchPanel.GetState();

            if (!touching && touches.Count > 0)
            {
                touching = true;
                TouchLocation touch = touches.First();

                HandleBoardTouch(touch);
                HandleResetTouch(touch);
            }
            else if (touches.Count == 0)
            {
                touching = false;
            }

            base.Update(gameTime);
        }

        private void HandleBoardTouch(TouchLocation touch)
        {
            if (winner == TicTacToePlayer.None)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Rectangle box = GetGridSpace(i, j, 150, 150);
                        if (grid[i, j] == TicTacToePlayer.None && box.Contains((int)touch.Position.X, (int)touch.Position.Y))
                        {

                            grid[i, j] = current;

                            CheckForWin(current);
                            CheckForWinnable();

                            current = current == TicTacToePlayer.PlayerO ? TicTacToePlayer.PlayerX : TicTacToePlayer.PlayerO;
                        }
                    }
                }
            }
        }

        private void CheckForWin(TicTacToePlayer player)
        {
            Func<TicTacToePlayer, bool> checkWinner = b => b == player;
            if (
                grid.Row(0).All(checkWinner) || grid.Row(1).All(checkWinner) || grid.Row(2).All(checkWinner) ||
                grid.Column(0).All(checkWinner) || grid.Column(1).All(checkWinner) || grid.Column(2).All(checkWinner) ||
                grid.Diagonal(MultiDimensionalArrayExtensions.DiagonalDirection.DownRight).All(checkWinner) || grid.Diagonal(MultiDimensionalArrayExtensions.DiagonalDirection.DownLeft).All(checkWinner)
            )
            {
                winner = player;
            }
        }
        
        private void CheckForWinnable()
        {
            if (winner == TicTacToePlayer.None)
            {
                Func<TicTacToePlayer, bool> checkNone = b => b == TicTacToePlayer.None;
                if (!grid.All().Any(checkNone))
                {
                    winnable = false;
                }
            }
        }

        private void HandleResetTouch(TouchLocation touch)
        {
            if (resetButtonPosition.Contains((int)touch.Position.X, (int)touch.Position.Y))
            {
                Reset();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            DrawGrid();
            DrawPieces();
            DrawStatus();
            DrawResetButton();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawGrid()
        {
            spriteBatch.Draw(gridTexture, gridRectangle, Color.White);
        }

        private void DrawPieces()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (grid[i, j] != TicTacToePlayer.None)
                    {
                        Texture2D texture = grid[i, j] == TicTacToePlayer.PlayerO ? oPiece : xPiece;
                        Rectangle position = GetGridSpace(i, j, texture.Width, texture.Height);
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        private void DrawStatus()
        {
            Texture2D texture;
            if (winner != TicTacToePlayer.None)
            {
                texture = winner == TicTacToePlayer.PlayerO ? oWinner : xWinner;
            }
            else if (!winnable)
            {
                texture = noWinner;
            }
            else
            {
                texture = current == TicTacToePlayer.PlayerO ? oTurn : xTurn;
            }

            Rectangle position = new Rectangle(spriteBatch.GraphicsDevice.Viewport.Width / 2 - (texture.Width / 2), 15, texture.Width, texture.Height);
            spriteBatch.Draw(texture, position, Color.White);
        }

        private void DrawResetButton()
        {
            if (winner != TicTacToePlayer.None || !winnable)
            {
                spriteBatch.Draw(resetButton, resetButtonPosition, Color.White);
            }
        }
        
        private Rectangle GetGridSpace(int column, int row, int width, int height)
        {
            int centerX = spriteBatch.GraphicsDevice.Viewport.Width / 2;
            int centerY = spriteBatch.GraphicsDevice.Viewport.Height / 2;

            int x = centerX + ((column - 1) * 150) - (width / 2);
            int y = centerY + ((row - 1) * 150) - (height / 2);

            return new Rectangle(x, y, width, height);
        }
    }
}
