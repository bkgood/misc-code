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


namespace Pgm04
{
	/// <summary>
	/// A player-controlled actor in our game.
	/// </summary>
	public class Actor : Microsoft.Xna.Framework.DrawableGameComponent, ICameraMonitorable
	{
		Point AbsolutePosition {
			get {
				if (!isMoving) {
					return new Point(position.X * map.TileDimensions.X, position.Y * map.TileDimensions.Y);
				} else {
					return manualDestination;
				}
			}
		}

		Texture2D sprites;
		private const int NumWalkingFrames = 8;
		private const int MillisecondsBetweenFrames = 50;
		int pixelsPerFrame;
		SpriteBatch spriteBatch;
		TileMap map;
		Point texture = new Point(0, 0);
		Point position = new Point(17, 6);
		Point manualDestination = Point.Zero;
		Direction facingDirection;
		int frameNum;
		readonly Point spriteSize = new Point(72, 72);
		GamePadStates gp;
		bool isMoving = false;
		TimeSpan timer;

		Point[] standingTextures = new Point[] { new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0), };
		Point[] walkingTextures = new Point[] { new Point(0, 1), new Point(0, 2), new Point(0, 3), new Point(0, 4), };

		enum Direction
		{
			North,
			South,
			East,
			West,
		}

		/// <summary>
		/// Construct the Actor instance
		/// </summary>
		/// <param name="game">Game this actor is to associate with.</param>
		public Actor(Game game)
			: base(game) {
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize() {
			sprites = Game.Content.Load<Texture2D>(@"Images\spritesheet");
			spriteBatch = new SpriteBatch(Game.GraphicsDevice);
			map = Game.Services.GetService(typeof(TileMap)) as TileMap;
			gp = Game.Services.GetService(typeof(GamePadStates)) as GamePadStates;
			pixelsPerFrame = map.TileDimensions.X / NumWalkingFrames;
			base.Initialize();
		}

		/// <summary>
		/// Allows the Actor to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime) {
			if (isMoving) {
				Animate(gameTime);
			}
			if (!isMoving) {
				Vector2 ts = gp.CurrFrame.ThumbSticks.Left;
				if (ts.Y > .5f) {
					Face(Direction.North);
					Move(Direction.North);
				} else if (ts.Y < -.5f) {
					Face(Direction.South);
					Move(Direction.South);
				} else if (ts.X < -.5f) {
					Face(Direction.West);
					Move(Direction.West);
				} else if (ts.X > .5f) {
					Face(Direction.East);
					Move(Direction.East);
				} else {
					texture = standingTextures[(int) facingDirection];
				}
				if (isMoving) {
					Animate(gameTime);
				}
			}
			base.Update(gameTime);
		}

		/// <summary>
		/// Draws the actor.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Draw(GameTime gameTime) {
			Vector2 offset = (Game.Services.GetService(typeof(Camera)) as Camera).Offset;
			spriteBatch.Begin();
			if (!isMoving) {
				spriteBatch.Draw(sprites, new Rectangle(position.X * map.TileDimensions.X - (spriteSize.X / pixelsPerFrame), position.Y * map.TileDimensions.Y - (spriteSize.Y / 2), spriteSize.X, spriteSize.Y), new Rectangle(texture.X * spriteSize.X, texture.Y * spriteSize.Y, spriteSize.X, spriteSize.Y), Color.White, 0f, offset, SpriteEffects.None, 0f);
			} else {
				spriteBatch.Draw(sprites, new Rectangle(manualDestination.X - (spriteSize.X / pixelsPerFrame), manualDestination.Y - (spriteSize.Y / 2), spriteSize.X, spriteSize.Y), new Rectangle(texture.X * spriteSize.X, texture.Y * spriteSize.Y, spriteSize.X, spriteSize.Y), Color.White, 0f, offset, SpriteEffects.None, 0f);
			}
			spriteBatch.End();
			base.Draw(gameTime);
		}

		/// <summary>
		/// Sets the actor to face a direction.
		/// </summary>
		/// <param name="dir">The direction to face</param>
		void Face(Direction dir) {
			texture = standingTextures[(int) dir];
			facingDirection = dir;
		}

		/// <summary>
		/// Move the actor one tile in a given direction if possible, and setup the move animation.
		/// </summary>
		/// <param name="dir">The direction to move</param>
		void Move(Direction dir) {
			Point newPosition = position;
			switch (dir) {
			case Direction.North:
				newPosition.Y--;
				break;
			case Direction.South:
				newPosition.Y++;
				break;
			case Direction.East:
				newPosition.X++;
				break;
			case Direction.West:
				newPosition.X--;
				break;
			}
			if (map.IsWalkable(newPosition)) {
				position = newPosition;
				texture = walkingTextures[(int) dir];
				isMoving = true;
				facingDirection = dir;
				frameNum = NumWalkingFrames;
				timer = new TimeSpan();
			} else {
				isMoving = false;
			}
		}

		/// <summary>
		/// Setup the next frame of the move animation if it is time to, or halt it if the last frame's been used.
		/// </summary>
		/// <param name="gameTime">A snapshot of timing values.</param>
		void Animate(GameTime gameTime) {
			bool chFrame = true;
			timer += gameTime.ElapsedGameTime;
			if (timer.Milliseconds < MillisecondsBetweenFrames) {
			    chFrame = false;
			}
			texture.X = frameNum - 1;
			switch (facingDirection) {
			case Direction.North:
				manualDestination = new Point(position.X * map.TileDimensions.X, position.Y * map.TileDimensions.Y + frameNum * pixelsPerFrame);
				break;
			case Direction.South:
				manualDestination = new Point(position.X * map.TileDimensions.X, position.Y * map.TileDimensions.Y - frameNum * pixelsPerFrame);
				break;
			case Direction.East:
				manualDestination = new Point(position.X * map.TileDimensions.X - frameNum * pixelsPerFrame, position.Y * map.TileDimensions.Y);
				break;
			case Direction.West:
				manualDestination = new Point(position.X * map.TileDimensions.X + frameNum * pixelsPerFrame, position.Y * map.TileDimensions.Y);
				break;
			}
			if (chFrame) {
				frameNum--;
				timer = new TimeSpan();
			}
			if (frameNum == 0) {
				isMoving = false;
			}
		}

		/// <summary>
		/// Implementation requred for ICameraMonitorable.
		/// </summary>
		/// <returns>MonitorInfo for use by camera.</returns>
		public MonitorInfo GetMonitorInfo() {
			return new MonitorInfo { PixelDimensions = this.spriteSize, AbsolutePosition = this.AbsolutePosition, };
		}
	}
}