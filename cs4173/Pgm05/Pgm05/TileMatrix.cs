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
	public class TileMatrix : Microsoft.Xna.Framework.DrawableGameComponent
	{
		public Rectangle CollisionRectangle {
			get {
				return new Rectangle((int) location.X, (int) location.Y,
					(TileRow.TilesPerRow + 3) * Tile.TileDimension + TileRow.DistanceBetweenTiles * (TileRow.TilesPerRow + 2),
					NumRows * Tile.TileDimension + (NumRows - 1) * DistanceBetweenRows);
			}
		}

		public Vector2 Location {
			get {
				return location;
			}
			set {
				location = value;
				for (int i = 0; i < rows.Length; i++) {
					if (rows[i] == null) {
						continue;
					}
					Vector2 rowLocation;
					rowLocation.Y = value.Y + i * (Tile.TileDimension + DistanceBetweenRows);
					rowLocation.X = rows[i].Location.X;
					rows[i].Location = rowLocation;
				}
			}
		}

		public TileRow this[int rowNum] {
			get {
				return rows[rowNum];
			}
		}

		TileRow[] rows;
		SpriteBatch spriteBatch;
		Dictionary<TileColor, Texture2D> textures;
		Vector2 location;
		int timer = 0;
		SoundEffect moveSound;

		const int MoveInterval = 1000;
		public const int NumRows = 8;
		readonly Vector2 StartLocation = new Vector2(20, 20);
		const int OddRowOffset = 98;
		const int DistanceBetweenRows = 6;

		public TileMatrix(Game game, Dictionary<TileColor, Texture2D> textureMap)
			: base(game) {
			rows = new TileRow[NumRows];
			textures = textureMap;
			UpdateOrder = BrickInvaders.TileMatrixPriority;
			location = StartLocation;
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize() {
			spriteBatch = new SpriteBatch(Game.GraphicsDevice);
			for (int i = 0; i < rows.Length; i++) {
				TileColor rowColor;
				Vector2 rowLocation = new Vector2();
				rowLocation.Y = location.Y + i * DistanceBetweenRows + i * Tile.TileDimension;
				rowLocation.X = (i % 2 == 0 ? location.X : OddRowOffset);
				if (i < 2) {
					rowColor = TileColor.Red;
				} else if (i < 4) {
					rowColor = TileColor.Orange;
				} else if (i < 6) {
					rowColor = TileColor.Yellow;
				} else {
					rowColor = TileColor.Green;
				}
				rows[i] = new TileRow(Game, rowColor);
				rows[i].Location = rowLocation;
				rows[i].MovingRight = (i % 2 == 0 ? true : false);
				moveSound = Game.Content.Load<SoundEffect>(@"Effects\movement");
			}
			base.Initialize();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime) {
			// all I should do is move stuff
			timer += gameTime.ElapsedGameTime.Milliseconds;
			if (timer > MoveInterval) {
				bool hasShifted = false;
				moveSound.Play();
				timer -= MoveInterval;
				for (int row = 0; row < rows.Length; row++) {
					rows[row].Move();
				}
				if (!rows[0].MovingRight && rows[0].Location.X < 20) {
					Location = new Vector2(location.X, location.Y + Tile.TileDimension);
					hasShifted = true;
				} else if (!rows[1].MovingRight && rows[1].Location.X < 20) {
					Location = new Vector2(location.X, location.Y + Tile.TileDimension);
					hasShifted = true;
				}
				if (hasShifted) {
					for (int row = 0; row < rows.Length; row++) {
						rows[row].MovingRight = !rows[row].MovingRight;
					}
				}
			}
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime) {
			spriteBatch.Begin();
			foreach (TileRow row in rows) {
				row.Draw(spriteBatch, textures);
			}
			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}