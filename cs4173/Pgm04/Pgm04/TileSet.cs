using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	/// Represents a set of tiles, index accessible with a single texture.
	/// </summary>
	class TileSet : GameComponent
	{
		public string Name {
			get;
			protected set;
		}

		public Texture2D Texture {
			get;
			protected set;
		}

		public Point TextureDimensions {
			get {
				return new Point(Texture.Width / Dimensions.X, Texture.Height / Dimensions.Y);
			}
		}

		public Point Dimensions {
			get;
			protected set;
		}

		/// <summary>
		/// Get a rectangle describing a tile with offset index on the texture.
		/// </summary>
		/// <param name="index">Offset</param>
		/// <returns>Tile-bounding rectangle.</returns>
		public Rectangle this[int index] {
			get {
				index -= FirstGID;
				return new Rectangle(index % TextureDimensions.X * Dimensions.X, index / TextureDimensions.X * Dimensions.Y, Dimensions.X, Dimensions.Y);
			}
		}

		public int FirstGID {
			get;
			private set;
		}

		/// <summary>
		/// Constructs a TileSet from XML.
		/// </summary>
		/// <param name="game">Game tileset is to associate with.</param>
		/// <param name="xml">XmlReader to initialize from.</param>
		public TileSet(Game game, XmlReader xml) : base(game) {
			FirstGID = int.Parse(xml.GetAttribute("firstgid"));
			Name = xml["name"];
			Dimensions = new Point(int.Parse(xml["tilewidth"]), int.Parse(xml["tileheight"]));
			while (xml.Read() && xml.NodeType != XmlNodeType.Element && xml.Name != "image");
			string textureName = xml["source"];
			textureName = textureName.Substring(0, textureName.LastIndexOf('.'));
			
			Texture = game.Content.Load<Texture2D>(String.Format(@"Images\Tile Sets\{0}", textureName));
		}
	}
}
