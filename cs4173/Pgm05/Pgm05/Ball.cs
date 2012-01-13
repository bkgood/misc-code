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
	public class Ball : Microsoft.Xna.Framework.DrawableGameComponent
	{
		public Rectangle CollisionRectangle {
			get {
				return new Rectangle((int) location.X, (int) location.Y, texture.Width, texture.Height);
			}
		}
		Vector2 location;
		Vector2 movement;
		Texture2D texture;
		SpriteBatch spriteBatch;
		Paddle paddle;
		bool isColliding;
		TileMatrix tileMatrix;


		const float MaxSpeed = 8f;
		const float MaxDeflection = Microsoft.Xna.Framework.MathHelper.PiOver4;

		public Ball(Game game)
			: base(game) {
			UpdateOrder = BrickInvaders.BallPriority;
			paddle = (Paddle) game.Services.GetService(typeof(Paddle));
			tileMatrix = (TileMatrix) game.Services.GetService(typeof(TileMatrix));
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize() {
			spriteBatch = new SpriteBatch(Game.GraphicsDevice);
			texture = Game.Content.Load<Texture2D>(@"Images\Ball");
			Reset();
			base.Initialize();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime) {
			location += movement;
			DoWallCollision();
			DoPaddleCollision();
			DoBrickCollision();
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime) {
			spriteBatch.Begin();
			spriteBatch.Draw(texture, location, Color.White);
			spriteBatch.End();
			base.Draw(gameTime);
		}

		void Reset() {
			location = new Vector2(0, 300);
			movement = new Vector2(1, 1);
		}

		void DoWallCollision() {
			if (location.X <= 0) {
				location.X = 0;
				movement.X *= -1;
			} else if (location.X >= Game.Window.ClientBounds.Width - texture.Width) {
				location.X = Game.Window.ClientBounds.Width - texture.Width;
				movement.X *= -1;
			}
			if (location.Y <= 0) {
				location.Y = 0;
				movement.Y *= -1;
			} else if (location.Y >= Game.Window.ClientBounds.Height) {
			//} else if (location.Y >= Game.Window.ClientBounds.Height  - texture.Height) {
				location.Y = Game.Window.ClientBounds.Height - texture.Height;
				movement.Y *= -1;
				Reset();
			}
		}

		void DoPaddleCollision() {
			if (movement.Y < 0) {
				return;
			}
			if (CollisionRectangle.Intersects(paddle.CollisionRectangle)) {
				if (isColliding) {
					return;
				}
				isColliding = true;
				Vector2 ballCenter = new Vector2(location.X + texture.Width / 2, location.Y + texture.Height / 2);
				float paddleCenter = paddle.Location.X + paddle.CollisionRectangle.Width / 2f;
				if (paddle.CollisionRectangle.Left > ballCenter.X && paddle.CollisionRectangle.Top < ballCenter.Y) {
					movement.Y = -Math.Abs(movement.Y);
					movement.X *= -1;
				} else if (paddle.CollisionRectangle.Right < ballCenter.X && paddle.CollisionRectangle.Top < ballCenter.Y) {
					movement.Y = -Math.Abs(movement.Y);
					movement.X *= -1;
				} else if (paddle.CollisionRectangle.Top > CollisionRectangle.Top) {
					movement.Y *= -1;
					float paddleWidth = paddle.CollisionRectangle.Width;
					float paddleX = paddle.CollisionRectangle.X;
					float paddleCenterX = paddleX + paddleWidth / 2;
					float percentChange = Math.Abs(paddleCenterX - ballCenter.X) / paddleWidth;
					percentChange += .05f;
					movement.X = movement.Y * percentChange; // lovely formula acquired with a.b=|a||b|cos(t)
					if (ballCenter.X < paddleCenterX) {
						movement.X = -Math.Abs(movement.X);
					} else if (ballCenter.X > paddleCenterX) {
						movement.X = Math.Abs(movement.X);
					}
					location.Y = paddle.Location.Y - texture.Height;
				} else if (paddle.CollisionRectangle.Bottom < ballCenter.Y) {
					movement.Y *= -1;
					location.Y = paddle.CollisionRectangle.Bottom;
				} else {
					location -= movement;
					movement -= new Vector2(1f, 1f);
				}
				if (movement.Length() < MaxSpeed) {
					movement *= 2;
				}
			} else {
				isColliding = false;
			}
		}

		void DoBrickCollision() {
			bool collision = false;
			Rectangle tileRectangle = new Rectangle();
			if (CollisionRectangle.Intersects(tileMatrix.CollisionRectangle)) {
				for (int row = 0; row < TileMatrix.NumRows; row++) {
					if (CollisionRectangle.Intersects(tileMatrix[row].CollisionRectangle)) {
						for (int tile = 0; tile < TileRow.TilesPerRow; tile++) {
							if (!tileMatrix[row][tile].Cleared && CollisionRectangle.Intersects(tileMatrix[row][tile].CollisionRectangle)) {
								tileMatrix[row][tile].Cleared = true;
								collision = true;
								tileRectangle = tileMatrix[row][tile].CollisionRectangle;
								// don't break, ball could be hitting two tiles
							}
						}
					}
				}
			}
			if (collision) {
				Vector2 ballCenter = new Vector2(location.X + texture.Width / 2, location.Y + texture.Height / 2);
				if (tileRectangle.Left > ballCenter.X && tileRectangle.Top < ballCenter.Y) {
					movement.Y = -Math.Abs(movement.Y);
					movement.X *= -1;
				} else if (tileRectangle.Right < ballCenter.X && tileRectangle.Top < ballCenter.Y) {
					movement.Y = -Math.Abs(movement.Y);
					movement.X *= -1;
				} else if (tileRectangle.Top > CollisionRectangle.Top) {
					movement.Y *= -1;
					location.Y = tileRectangle.Y - texture.Height;
				} else if (tileRectangle.Bottom < ballCenter.Y) {
					movement.Y *= -1;
					location.Y = tileRectangle.Bottom;
				}
			}
		}
	}
}