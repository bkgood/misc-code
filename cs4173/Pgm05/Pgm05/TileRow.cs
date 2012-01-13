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
	public class TileRow : Microsoft.Xna.Framework.GameComponent
	{
		public Rectangle CollisionRectangle {
			get {
				return new Rectangle((int) location.X, (int) location.Y, Tile.TileDimension * TilesPerRow + Tile.TileDimension * (TilesPerRow - 1), Tile.TileDimension);
			}
		}

		public Vector2 Location {
			get {
				return location;
			}
			set {
				location = value;
				for (int i = 0; i < tiles.Length; i++) {
					if (tiles[i] == null) {
						continue;
					}
					Vector2 tileLocation;
					tileLocation.Y = value.Y;
					tileLocation.X = value.X + i * (Tile.TileDimension + DistanceBetweenTiles);
					tiles[i].Location = tileLocation;
				}
			}
		}

		public bool MovingRight {
			get;
			set;
		}

		public Tile this[int tileNum] {
			get {
				return tiles[tileNum];
			}
		}

		Tile[] tiles;
		TileColor rowColor;
		Vector2 location;

		public const int TilesPerRow = 26;
		public const int DistanceBetweenTiles = 6;

		public TileRow(Game game, TileColor rowColor)
			: base(game) {
			UpdateOrder = BrickInvaders.TileRowPriority;
			Game.Components.Add(this);
			tiles = new Tile[TilesPerRow];
			this.rowColor = rowColor;
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize() {
			for (int i = 0; i < tiles.Length; i++) {
				Vector2 tileLocation;
				tileLocation.Y = location.Y;
				tileLocation.X = location.X + i * (Tile.TileDimension + DistanceBetweenTiles);
				tiles[i] = new Tile(Game, this, rowColor);
				tiles[i].Location = tileLocation;
			}
			base.Initialize();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
		}

		public void Draw(SpriteBatch sb, Dictionary<TileColor, Texture2D> tileTextures) {
			foreach (Tile tile in tiles) {
				tile.Draw(sb, tileTextures[rowColor]);
			}
		}

		public void Move() {
			if (MovingRight) {
				Location = new Vector2(Location.X + DistanceBetweenTiles, Location.Y);
			} else {
				Location = new Vector2(Location.X - DistanceBetweenTiles, Location.Y);
			}
		}
	}
}