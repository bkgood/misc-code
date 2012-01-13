/***
 * Sound obtained from http://www.djgallagher.com/games/classics/spaceinvaders/sounds.php
 * Program mostly complete but I've been sick to the point of not being able to concentrate (fever, muscle aches, cough, headaches
 * and other flu symptoms but no flu according to the dr) since last Thursday and as such haven't been able to complete it.
 * Hopefully this is worth some points.
 */

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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Pgm05
{
	public enum GameState
	{
		Playing, NewGame, NewBall, GameOver
	}

	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class BrickInvaders : Microsoft.Xna.Framework.Game
	{
		public GameState State;
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Controls controls;
		Ball ball;
		Paddle paddle;
		TileMatrix tileMatrix;
		Texture2D bg;
		Dictionary<TileColor, Texture2D> tileTextures;

		// priorities of Update calls, lower comes first
		public const int ControlPriority = 1;
		public const int BallPriority = 2;
		public const int PaddlePriority = 2;
		public const int TileMatrixPriority = 3;
		public const int TileRowPriority = 4;
		public const int TilePriority = 5;

		public BrickInvaders() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			tileTextures = new Dictionary<TileColor, Texture2D>();
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize() {
			graphics.PreferredBackBufferWidth = 800;
			graphics.PreferredBackBufferHeight = 600;
			graphics.ApplyChanges();
			Window.Title = "Brick Invaders – CS 4173, Assignment 6 – Bill Good";

			if (GamePad.GetCapabilities(PlayerIndex.One).IsConnected) {
				Components.Add(controls = new GamePadControls(this, PlayerIndex.One));
			} else {
				Components.Add(controls = new KeyboardControls(this));
			}
			Services.AddService(typeof(Controls), controls);

			Components.Add(tileMatrix = new TileMatrix(this, tileTextures));
			Services.AddService(typeof(TileMatrix), tileMatrix);

			Components.Add(paddle = new Paddle(this));
			Services.AddService(typeof(Paddle), paddle);
			Components.Add(ball = new Ball(this));
			Services.AddService(typeof(Ball), ball);
			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
			spriteBatch = new SpriteBatch(GraphicsDevice);
			bg = Content.Load<Texture2D>(@"Images\PolarisStars");
			tileTextures[TileColor.Green] = Content.Load<Texture2D>(@"Images\BrickGreen");
			tileTextures[TileColor.Orange] = Content.Load<Texture2D>(@"Images\BrickOrange");
			tileTextures[TileColor.Red] = Content.Load<Texture2D>(@"Images\BrickRed");
			tileTextures[TileColor.Yellow] = Content.Load<Texture2D>(@"Images\BrickYellow");
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent() {
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime) {
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) {
				this.Exit();
			}
			if (gameTime.IsRunningSlowly) {
				//Console.WriteLine("is running slowly " + new Random().Next());
			}
			switch (State) {
			case GameState.Playing:
				break;
			case GameState.NewGame:
				break;
			case GameState.NewBall:
				break;
			case GameState.GameOver:
				break;
			}
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(Color.Black);

			spriteBatch.Begin();
			spriteBatch.Draw(bg, Vector2.Zero, Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
