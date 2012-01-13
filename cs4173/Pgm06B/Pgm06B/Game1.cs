// Bill Good

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

namespace Pgm06B
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		Matrix cameraView;
		Matrix cameraProjection;
		Matrix world;

		BasicEffect effect;

		Texture2D osuLogo;
		Texture2D pistolPete;
		Texture2D red;
		Texture2D blue;
		Texture2D orange;
		Texture2D black;

		VertexPositionNormalTexture[] frontVertices;
		VertexPositionNormalTexture[] backVertices;
		VertexPositionNormalTexture[] leftVertices;
		VertexPositionNormalTexture[] rightVertices;
		VertexPositionNormalTexture[] topVertices;
		VertexPositionNormalTexture[] bottomVertices;

		readonly Color bgColor = Color.White;

		public Game1() {
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
			cameraView = Matrix.CreateLookAt(
				new Vector3(0f, 0f, 10f),
				Vector3.Zero,
				Vector3.UnitY);
			cameraProjection = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.PiOver4,
				(float) Window.ClientBounds.Width / Window.ClientBounds.Height,
				1f,
				100f);
			world = Matrix.Identity;
			SetupVertices();

			base.Initialize();
		}

		void SetupVertices() {
			Vector3[] points;
			Vector2[] textPoints;

			frontVertices = new VertexPositionNormalTexture[4];
			points = new Vector3[] {
			    new Vector3(-1, -1, 1),
			    new Vector3(-1, 1, 1),
			    new Vector3(1, -1, 1),
			    new Vector3(1, 1, 1)
			};
			textPoints = new Vector2[] {
			    Vector2.UnitY,
			    Vector2.Zero,
			    Vector2.One,
			    Vector2.UnitX
			};
			for (int i = 0; i < points.Length; i++) {
				frontVertices[i] = new VertexPositionNormalTexture(points[i], Vector3.UnitZ, textPoints[i]);
			}

			backVertices = new VertexPositionNormalTexture[4];
			points = new Vector3[] {
			    new Vector3(-1, -1, -1),
			    new Vector3(-1, 1, -1),
			    new Vector3(1, -1, -1),
			    new Vector3(1, 1, -1)
			};
			textPoints = new Vector2[] {
			    Vector2.One,
			    Vector2.UnitX,
			    Vector2.UnitY,
			    Vector2.Zero,
			};
			for (int i = 0; i < points.Length; i++) {
				backVertices[i] = new VertexPositionNormalTexture(points[i], -Vector3.UnitZ, textPoints[i]);
			}

			leftVertices = new VertexPositionNormalTexture[4];
			points = new Vector3[] {
			    new Vector3(-1, -1, -1),
			    new Vector3(-1, 1, -1),
			    new Vector3(-1, -1, 1),
			    new Vector3(-1, 1, 1)
			};
			textPoints = new Vector2[] {
			    Vector2.UnitY,
			    Vector2.Zero,
			    Vector2.One,
			    Vector2.UnitX
			};
			for (int i = 0; i < points.Length; i++) {
				leftVertices[i] = new VertexPositionNormalTexture(points[i], -Vector3.UnitX, textPoints[i]);
			}

			rightVertices = new VertexPositionNormalTexture[4];
			points = new Vector3[] {
			    new Vector3(1, -1, -1),
			    new Vector3(1, 1, -1),
			    new Vector3(1, -1, 1),
			    new Vector3(1, 1, 1)
			};
			textPoints = new Vector2[] {
			    Vector2.UnitY,
			    Vector2.Zero,
			    Vector2.One,
			    Vector2.UnitX
			};
			for (int i = 0; i < points.Length; i++) {
				rightVertices[i] = new VertexPositionNormalTexture(points[i], Vector3.UnitX, textPoints[i]);
			}

			topVertices = new VertexPositionNormalTexture[4];
			points = new Vector3[] {
			    new Vector3(-1, 1, 1),
			    new Vector3(-1, 1, -1),
			    new Vector3(1, 1, 1),
			    new Vector3(1, 1, -1)
			};
			textPoints = new Vector2[] {
			    Vector2.UnitY,
			    Vector2.Zero,
			    Vector2.One,
			    Vector2.UnitX
			};
			for (int i = 0; i < points.Length; i++) {
				topVertices[i] = new VertexPositionNormalTexture(points[i], Vector3.UnitY, textPoints[i]);
			}

			bottomVertices = new VertexPositionNormalTexture[4];
			points = new Vector3[] {
			    new Vector3(-1, -1, 1),
			    new Vector3(-1, -1, -1),
			    new Vector3(1, -1, 1),
			    new Vector3(1, -1, -1)
			};
			textPoints = new Vector2[] {
			    Vector2.UnitY,
			    Vector2.Zero,
			    Vector2.One,
			    Vector2.UnitX
			};
			for (int i = 0; i < points.Length; i++) {
				bottomVertices[i] = new VertexPositionNormalTexture(points[i], -Vector3.UnitY, textPoints[i]);
			}
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			pistolPete = Content.Load<Texture2D>(@"Pistol Pete");
			osuLogo = Content.Load<Texture2D>(@"OSU Logo");

			red = new Texture2D(GraphicsDevice, 1, 1);
			red.SetData<Color>(new Color[] { Color.Red });
			blue = new Texture2D(GraphicsDevice, 1, 1);
			blue.SetData<Color>(new Color[] { Color.Blue });
			orange = new Texture2D(GraphicsDevice, 1, 1);
			orange.SetData<Color>(new Color[] { Color.Orange });
			black = new Texture2D(GraphicsDevice, 1, 1);
			black.SetData<Color>(new Color[] { Color.Black });

			effect = new BasicEffect(GraphicsDevice, null);
			effect.EnableDefaultLighting();
			effect.PreferPerPixelLighting = true;
			effect.View = cameraView;
			effect.Projection = cameraProjection;
			effect.Texture = black;
			// Turn off coloring and turn on texturing.
			effect.VertexColorEnabled = false;
			effect.TextureEnabled = true;
			effect.LightingEnabled = true;
			effect.World = Matrix.Identity;
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent() {
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime) {
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			KeyboardState kbState = Keyboard.GetState();
			if (kbState.IsKeyDown(Keys.X)) {
				effect.World *= Matrix.CreateRotationX(.02f);
			}
			if (kbState.IsKeyDown(Keys.Y)) {
				effect.World *= Matrix.CreateRotationY(.02f);
			}
			if (kbState.IsKeyDown(Keys.Z)) {
				effect.World *= Matrix.CreateRotationZ(.02f);
			}


			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(bgColor);

			GraphicsDevice.RenderState.CullMode = CullMode.None;
			
			GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);

			effect.Texture = pistolPete;
			effect.Begin();
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Begin();
				GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, frontVertices, 0, 2);
				pass.End();
			}
			effect.End();

			effect.Texture = osuLogo;
			effect.Begin();
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Begin();
				GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, backVertices, 0, 2);
				pass.End();
			}
			effect.End();

			effect.Texture = red;
			effect.Begin();
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Begin();
				GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, leftVertices, 0, 2);
				pass.End();
			}
			effect.End();

			effect.Texture = blue;
			effect.Begin();
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Begin();
				GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, rightVertices, 0, 2);
				pass.End();
			}
			effect.End();

			effect.Texture = orange;
			effect.Begin();
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Begin();
				GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, topVertices, 0, 2);
				pass.End();
			}
			effect.End();

			effect.Texture = black;
			effect.Begin();
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Begin();
				GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, bottomVertices, 0, 2);
				pass.End();
			}
			effect.End();

			base.Draw(gameTime);
		}
	}
}
