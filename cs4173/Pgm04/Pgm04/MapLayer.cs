using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
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
	/// Represents a layer of a map.
	/// </summary>
	class MapLayer
	{
		public bool IsInfo {
			get;
			private set;
		}

		public Point Dimensions {
			get;
			private set;
		}

		public string Name {
			get;
			private set;
		}

		List<Tile> map;
		Game game;

		/// <summary>
		/// Constructs new layer given an XmlReader sitting at a new layer and all known tilesets.
		/// </summary>
		/// <param name="game">Game to associate with</param>
		/// <param name="xml">XmlReader at a new layer</param>
		/// <param name="tilesets">All known tilesets</param>
		public MapLayer(Game game, XmlReader xml, List<TileSet> tilesets) {
			this.game = game;
			map = new List<Tile>();
			Name = xml["name"];
			IsInfo = (Name == "walkable" ? true : false);
			Dimensions = new Point(int.Parse(xml["width"]), int.Parse(xml["height"]));
			while (xml.Read()) {
				if (xml.NodeType == XmlNodeType.EndElement && xml.Name.ToLower() == "data") {
					break;
				} else if (xml.NodeType != XmlNodeType.Element) {
					continue;
				} else if (xml.Name.ToLower() == "tile") {
					TileSet tileSet = tilesets[tilesets.Count - 1];
					int gid = int.Parse(xml["gid"]);
					foreach (TileSet ts in tilesets.Reverse<TileSet>()) {
						if (gid >= ts.FirstGID) {
							tileSet = ts;
							break;
						}
					}
					if (IsInfo) {
						map.Add(new InfoTile(gid == tileSet.FirstGID ? true : false));
					} else {
						map.Add(new DrawableTile(tileSet, gid));
					}
				}
			}
		}

		/// <summary>
		/// Draws the layer.
		/// </summary>
		/// <param name="sb">SpriteBatch to draw on.</param>
		public void Draw(SpriteBatch sb) {
			if (IsInfo) return;
			int i = 0;
			Vector2 offset = ((Camera) game.Services.GetService(typeof(Camera))).Offset;
			foreach (DrawableTile tile in map) {
				tile.Draw(sb, new Vector2((i % Dimensions.X) * 32, (i / Dimensions.X) * 32), offset);
				i++;
			}
		}

		/// <summary>
		/// Determines if point on coordinate plane is traversable.
		/// </summary>
		/// <param name="point">point</param>
		/// <returns>True if traversable.</returns>
		public bool IsWalkable(Point point) {
			if (!IsInfo) {
				return true;
			}
			int tileIndex = 0;
			tileIndex += point.X;
			tileIndex += point.Y * Dimensions.X;
			return (map[tileIndex] as InfoTile).IsWalkable;
		}
	}
}
