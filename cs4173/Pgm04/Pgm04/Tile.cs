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
	/// Represents a simple tile
	/// </summary>
	abstract class Tile { }

	/// <summary>
	/// Represents a tile containing information.
	/// </summary>
	class InfoTile : Tile
	{
		/// <summary>
		/// True when tile is traversable.
		/// </summary>
		public bool IsWalkable {
			get;
			protected set;
		}

		public InfoTile(bool isWalkable) {
			IsWalkable = isWalkable;
		}
	}

	/// <summary>
	/// Represents a tile to be drawn.
	/// </summary>
	class DrawableTile : Tile
	{
		TileSet tileset;
		int tilesetIndex;

		public DrawableTile(TileSet ts, int tilesetIndex) {
			tileset = ts;
			this.tilesetIndex = tilesetIndex;
		}

		/// <summary>
		/// Draw the tile.
		/// </summary>
		/// <param name="sb">SpriteBatch on which to draw the tile</param>
		/// <param name="at">Where to draw the tile</param>
		/// <param name="offset">Offset with which to draw the tile (hint: used for map scrolling with camera)</param>
		public void Draw(SpriteBatch sb, Vector2 at, Vector2 offset) {
			if (tilesetIndex != 0) {
				sb.Draw(tileset.Texture, at, tileset[tilesetIndex], Color.White, 0f, offset, 1f, SpriteEffects.None, 0f);
			}
		}
	}
}
