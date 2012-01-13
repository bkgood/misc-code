using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
	/// Represents a map of layers of tiles.
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class TileMap : Microsoft.Xna.Framework.DrawableGameComponent, ICameraMonitorable
	{
		public Point Dimensions {
			get;
			protected set;
		}

		public Point TileDimensions {
			get;
			protected set;
		}

		public Point PixelDimensions {
			get {
				return new Point(Dimensions.X * TileDimensions.X, Dimensions.Y * TileDimensions.Y);
			}
		}

		SpriteBatch spriteBatch;
		SpriteFont font;
		List<MapLayer> layers;
		List<TileSet> tilesets;
		string mapFile;
		

		public TileMap(Game game, string mapFile) : base(game) {
			layers = new List<MapLayer>();
			tilesets = new List<TileSet>();
			this.mapFile = mapFile;
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// Sets up the map from xml (path provided in constructor), and makes new TileSets and 
		/// MapLayers as it comes across the related XML bits.
		/// </summary>
		public override void Initialize() {
			XmlReaderSettings settings = new XmlReaderSettings { 
				IgnoreComments = true,
				IgnoreProcessingInstructions = true,
				IgnoreWhitespace = true
			};
			XmlReader xml = XmlReader.Create(mapFile, settings);
			while (xml.Read()) {
				if (xml.NodeType != XmlNodeType.Element) {
					continue;
				}
				switch (xml.Name.ToLower()) {
				case "map":
					TileDimensions = new Point(int.Parse(xml.GetAttribute("tilewidth")), int.Parse(xml.GetAttribute("tileheight")));
					Dimensions = new Point(int.Parse(xml.GetAttribute("width")), int.Parse(xml.GetAttribute("height")));
					//Console.WriteLine("Making map with dimensions {0}", Dimensions);
					break;
				case "tileset":
					tilesets.Add(new TileSet(Game, xml));
					break;
				case "layer":
					layers.Add(new MapLayer(Game, xml, tilesets));
					break;
				}
			}
			
			base.Initialize();
		}

		protected override void LoadContent() {
			spriteBatch = new SpriteBatch(Game.GraphicsDevice);
			base.LoadContent();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime) {

			base.Update(gameTime);
		}

		/// <summary>
		/// Draws the map.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Draw(GameTime gameTime) {
			spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
			foreach (MapLayer layer in layers) {
				layer.Draw(spriteBatch);
			}
			spriteBatch.End();
			base.Draw(gameTime);
		}

		/// <summary>
		/// Determine if a point on the coordinate plane is traversable.
		/// </summary>
		/// <param name="point">Coordinate</param>
		/// <returns>True if traversable</returns>
		public bool IsWalkable(Point point) {
			foreach (MapLayer layer in layers) {
				if (!layer.IsWalkable(point)) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Implementation requred for ICameraMonitorable.
		/// </summary>
		/// <returns>MonitorInfo for use by camera.</returns>
		public MonitorInfo GetMonitorInfo() {
			return new MonitorInfo { PixelDimensions = this.PixelDimensions, AbsolutePosition = Point.Zero, };
		}
	}
}