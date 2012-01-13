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
	/* IMPORTANT: COPYRIGHTS AND ACKNOWLEDGEMENTS
	 * beach tileset.png: Obtained from http://reinerstileset.4players.de/environmentE.html, minorly modified by myself.
	 * walkable.png: Self-created in GIMP
	 * Diag.png: Self-created in GIMP (uses stock tile image in GIMP, and uses font "Berlin Sans FB Demi Bold" (neither my creations))
	 * spritesheet.png: Obtained from http://reinerstileset.4players.de/humansE.html, minor modifications of my own.
	 * map.tmx: Created using Tiled-QT @ http://mapeditor.org/. Tiled-QT generated an XML map file (map.tmx) from tilesets
	 *	"beach tileset.png" and "walkable.png", using a simple drag-and-drop process. Permission obtained 2-9-10 in class for use;
	 *	program's largest role was helping me to lay out my map and its layers, far easier than trying to visualize multiple layers
	 *	using text files or my own custom XML. It however does not modify the tile sheets themselves, it only creates an XML file
	 *	describing their use. Tiled-QT is made available (along with its source) under the GNU GPL v2. Documentation of the tmx XML
	 *	format available at http://sourceforge.net/apps/mediawiki/tiled/index.php?title=Examining_the_map_format
	 * Whitney.mp3: Obtained from http://www.queststudios.com/quest/collection.html.
	 */
	public class GamePadStates : GameComponent
	{
		public GamePadState PrevFrame {
			get;
			private set;
		}
		public GamePadState CurrFrame {
			get;
			private set;
		} 

		public readonly PlayerIndex PIndex;

		public GamePadStates(Game game, PlayerIndex pIndex) : base(game) {
			PIndex = pIndex;
		}

		public override void Initialize() {
			PrevFrame = CurrFrame = GamePad.GetState(PIndex);
			base.Initialize();
		}

		public override void Update(GameTime gameTime) {
			PrevFrame = CurrFrame;
			CurrFrame = GamePad.GetState(PlayerIndex.One);
			base.Update(gameTime);
		}
	}
	
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class MapGame : Microsoft.Xna.Framework.Game
	{
		TileMap map;
		Camera camera;
		Actor myActor;
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		GamePadStates gp;
		GameState state = GameState.Start;
		Song song;
		Texture2D diag;

		readonly Color BGColor = Color.CornflowerBlue;

		enum GameState
		{
			Start,
			Play,
		}

		public MapGame() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize() {
			graphics.PreferredBackBufferHeight = graphics.PreferredBackBufferWidth = 500;
			graphics.ApplyChanges();
			Window.Title = "CS 4173, Assignment 4 – Bill Good";
			map = new TileMap(this, @"Content\Maps\map.tmx");
			map.DrawOrder = 1;
			camera = new Camera(this);
			myActor = new Actor(this);
			myActor.DrawOrder = 2;
			gp = new GamePadStates(this, PlayerIndex.One);
			gp.UpdateOrder = int.MinValue; // please make this update first!
			Services.AddService(typeof(TileMap), map);
			Services.AddService(typeof(Camera), camera);
			Services.AddService(typeof(GamePadStates), gp);
			Components.Add(map);
			Components.Add(camera);
			Components.Add(myActor);
			Components.Add(gp);
			camera.Monitor(map);
			camera.Monitor(myActor, true);
			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
			spriteBatch = new SpriteBatch(GraphicsDevice);
			song = Content.Load<Song>(@"Sounds\Whitney");
			diag = Content.Load<Texture2D>(@"Images\Diag");
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent() {
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime) {
			if (gp.CurrFrame.Buttons.Back == ButtonState.Pressed) {
				this.Exit();
			}
			switch (state) {
			case GameState.Start:
				myActor.Enabled = false;
				if (gp.CurrFrame.Buttons.Start == ButtonState.Pressed) {
					state = GameState.Play;
					MediaPlayer.Play(song);
					MediaPlayer.IsRepeating = true;
					myActor.Enabled = true;
				}
				break;
			case GameState.Play:
				break;
			}
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(BGColor);
			base.Draw(gameTime);
			switch (state) {
			case GameState.Start:
				spriteBatch.Begin();
				spriteBatch.Draw(diag, Vector2.Zero, Color.White);
				spriteBatch.End();
				break;
			case GameState.Play:
				break;
			}
		}
	}
}
