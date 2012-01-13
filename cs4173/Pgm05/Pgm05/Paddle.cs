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
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class Paddle : Microsoft.Xna.Framework.DrawableGameComponent
	{
		public Rectangle CollisionRectangle {
			get {
				return new Rectangle((int) location.X, (int) location.Y, texture.Width, texture.Height);
			}
		}

		public Vector2 Location {
			get {
				return location;
			}
		}

		Vector2 location;
		SpriteBatch spriteBatch;
		Texture2D texture;
		Controls controls;

		const int Speed = 10;

		public Paddle(Game game)
			: base(game) {
			UpdateOrder = BrickInvaders.PaddlePriority;
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize() {
			spriteBatch = new SpriteBatch(Game.GraphicsDevice);
			texture = Game.Content.Load<Texture2D>(@"Images\Paddle");
			controls = (Controls) Game.Services.GetService(typeof(Controls));
			location.X = Game.Window.ClientBounds.Width / 2 - texture.Width / 2;
			location.Y = Game.Window.ClientBounds.Height - texture.Height * 3;

			base.Initialize();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime) {
			DoBrickCollision();
			if (controls.IsLeftPressed) {
				location.X -= Speed;
				if (location.X < 0) {
					location.X = 0;
				}
			} else if (controls.IsRightPressed) {
				location.X += Speed;
				if (location.X + texture.Width > Game.Window.ClientBounds.Width) {
					location.X = Game.Window.ClientBounds.Width - texture.Width;
				}
			}
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime) {
			spriteBatch.Begin();
			spriteBatch.Draw(texture, location, Color.White);
			spriteBatch.End();
			base.Draw(gameTime);
		}

		void DoBrickCollision() {
			TileMatrix tileMatrix = (TileMatrix) Game.Services.GetService(typeof(TileMatrix));
			if (CollisionRectangle.Intersects(tileMatrix.CollisionRectangle)) {
				for (int row = 0; row < TileMatrix.NumRows; row++) {
					if (CollisionRectangle.Intersects(tileMatrix[row].CollisionRectangle)) {
						for (int tile = 0; tile < TileRow.TilesPerRow; tile++) {
							if (!tileMatrix[row][tile].Cleared && CollisionRectangle.Intersects(tileMatrix[row][tile].CollisionRectangle)) {
								((BrickInvaders) Game).State = GameState.GameOver;
								break; // only need to hit one tile to die
							}
						}
					}
				}
			}
		}
	}
}