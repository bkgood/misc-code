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
	public enum TileColor
	{
		Red,
		Orange,
		Yellow,
		Green,
	}

	public enum TileSide
	{
		Top,
		Right,
		Bottom,
		Left,
	}

	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class Tile : Microsoft.Xna.Framework.GameComponent
	{
		public Rectangle CollisionRectangle {
			get {
				return new Rectangle((int) location.X, (int) location.Y, TileDimension, TileDimension);
			}
		}

		public bool Cleared {
			get;
			set;
		}

		public bool CheckCollision {
			get;
			set;
		}

		public Vector2 Location {
			get {
				return location;
			}
			set {
				location = value;
			}
		}

		public TileColor Color_ {
			get {
				return color;
			}
		}

		TileRow containingRow;
		TileColor color;
		Vector2 location;

		public const int TileDimension = 20;
		public const int DistanceBetweenTiles = 6;

		public Tile(Game game, TileRow row, TileColor color)
			: base(game) {
			UpdateOrder = BrickInvaders.TilePriority;
			Game.Components.Add(this);
			Cleared = false;
			this.containingRow = row;
			this.color = color;
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize() {
			base.Initialize();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime) {
			if (!Cleared && CollisionRectangle.Bottom >= Game.Window.ClientBounds.Height) {
				((BrickInvaders) Game).State = GameState.GameOver;
			}
			base.Update(gameTime);
		}

		public void Draw(SpriteBatch sb, Texture2D texture) {
			if (!Cleared) {
				sb.Draw(texture, new Rectangle((int) location.X, (int) location.Y, TileDimension, TileDimension), Color.White);
			}
		}
	}
}