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

namespace Pgm06A
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		const int NumSides = 60;

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Color coneColor = Color.Blue;
		BasicEffect effect;
		const float coneHeight = 5f;
		const float radius = coneHeight / 4f;

		// Camera matrices
		Matrix viewMatrix = Matrix.Identity;
		Matrix projMatrix = Matrix.Identity;

		//
		VertexPositionNormalTexture[] baseVerts = new VertexPositionNormalTexture[NumSides + 2];
		VertexPositionNormalTexture[] sideVerts = new VertexPositionNormalTexture[NumSides + 2];
		Matrix world = Matrix.Identity * Matrix.CreateFromYawPitchRoll(0, MathHelper.Pi, 0);

		Texture2D blue;

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
			viewMatrix = Matrix.CreateLookAt(
				new Vector3(0f, 0f, 11f),
				Vector3.Zero,
				Vector3.UnitY);
			projMatrix = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.PiOver4,
				(float) Window.ClientBounds.Width / Window.ClientBounds.Height,
				1f,
				100f);

			blue = new Texture2D(GraphicsDevice, 1, 1);
			blue.SetData<Color>(new Color[] { Color.Blue });

			baseVerts[0] = new VertexPositionNormalTexture(Vector3.Zero, Vector3.Zero, Vector2.Zero);
			for (int i = 1; i < baseVerts.Length; i++) {
				float theta = (i - 1) * (MathHelper.TwoPi / NumSides);
				Vector3 position = new Vector3((float) Math.Cos(theta) * radius, (float) Math.Sin(theta) * radius, 0);
				baseVerts[i] = new VertexPositionNormalTexture(position, -Vector3.UnitZ, Vector2.Zero);
			}

			sideVerts[0] = new VertexPositionNormalTexture(new Vector3(0, 0, -coneHeight), Vector3.Zero, Vector2.Zero);
			for (int i = 1; i < sideVerts.Length; i++) {
				float theta = (i - 1) * (MathHelper.TwoPi / NumSides);
				Vector3 position = new Vector3((float) Math.Cos(theta) * radius, (float) Math.Sin(theta) * radius, 0);
				sideVerts[i] = new VertexPositionNormalTexture(position, Vector3.Zero, Vector2.Zero);

			}
			SetNormals(sideVerts);

			base.Initialize();
		}

		static void SetNormals(VertexPositionNormalTexture[] verts) {
			for (int i = 2; i < verts.Length; i++) {
				Vector3 prev = verts[i - 1].Position - verts[0].Position;
				Vector3 curr = verts[0].Position - verts[i].Position;
				Vector3 normal = Vector3.Cross(prev, curr);
				normal.Normalize();
				verts[0].Normal += normal;
				verts[i - 1].Normal += normal;
				verts[i].Normal += normal;
			}
			for (int i = 0; i < verts.Length; i++) {
			    verts[i].Normal.Normalize();
			}
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
			spriteBatch = new SpriteBatch(GraphicsDevice);

			effect = new BasicEffect(GraphicsDevice, null);
			effect.View = viewMatrix;
			effect.Projection = projMatrix;
			effect.EnableDefaultLighting();
			effect.PreferPerPixelLighting = true;
			effect.Texture = blue;
			effect.VertexColorEnabled = false;
			effect.TextureEnabled = true;
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

			world *= Matrix.CreateRotationY(MathHelper.PiOver4 / 20f);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(Color.Black);

			GraphicsDevice.RenderState.CullMode = CullMode.None;
			GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);

			effect.Begin();
			effect.World = world;
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Begin();
				// I wasn't sure if the assignment wanted to draw a base or not so I went ahead and drew one
				GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleFan, baseVerts, 0, NumSides);
				GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleFan, sideVerts, 0, NumSides);
				pass.End();
			}
			effect.End();

			base.Draw(gameTime);
		}
	}
}
