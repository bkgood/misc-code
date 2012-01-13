// Bill Good
// Program 1

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

namespace Pgm01
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D cards; // given sprite sheet of cards
		Texture2D logo;
		Random rng; // random number generator for choosing cards at random
		HashSet<Point> usedCards;
		List<DrawInfo> toDraw;
		Color bgColor = new Color(0, 128, 0); // same green as in foundation given in Cards.png
		Vector2 logoPosition;

		struct DrawInfo // used for passing around info to draw cards to the screen
		{
			public Point card;
			public Vector2 position;
			public float zIndex; // 0 is front, 1 is back

			public DrawInfo(Point card, Vector2 position, float zIndex)
			{
				this.card = card;
				this.position = position;
				this.zIndex = zIndex;
			}
		}

		// Various constants from Cards.png
		const int CARD_WIDTH = 92;
		const int CARD_HEIGHT = 138;
		const int GRID_WIDTH = 10;
		const int GRID_HEIGHT = 4;

		// Some special cards:
		readonly Point CARD_BACKING = new Point(8, 4);
		readonly Point CARD_FOUNDATION = new Point(9, 4);
		readonly Point CARD_DECK_END = new Point(10, 4);

		// Special positioning:
		readonly int DISTANCE_BETWEEN_ITEMS;
		readonly int STACKING_OFFSET = 38;
		readonly Vector2 DECK_POSITION;
		readonly Vector2 DISCARD_POSITION;
		readonly Vector2[] FOUNDATION_POSITIONS;
		readonly Vector2[] TABLEAU_POSITIONS;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			
			// make set for keeping track of which cards have already been drawn (and therefore shouldn't be drawn again),
			// also add our three special 'cards' so we don't get those from our random card generator
			usedCards = new HashSet<Point>();
			usedCards.Add(CARD_BACKING);
			usedCards.Add(CARD_FOUNDATION);
			usedCards.Add(CARD_DECK_END);
			
			rng = new Random(System.DateTime.Now.Millisecond); // new seed every time we run (likely)
			toDraw = new List<DrawInfo>();

			// Generate the various positioning numbers:
			DISTANCE_BETWEEN_ITEMS = (Window.ClientBounds.Width - (7 * CARD_WIDTH)) / 8;

			DECK_POSITION = new Vector2(DISTANCE_BETWEEN_ITEMS, DISTANCE_BETWEEN_ITEMS);
			DISCARD_POSITION = new Vector2(DECK_POSITION.X + CARD_WIDTH + DISTANCE_BETWEEN_ITEMS, DECK_POSITION.Y);
			FOUNDATION_POSITIONS = new Vector2[4];
			FOUNDATION_POSITIONS[0] = new Vector2(DISCARD_POSITION.X + CARD_WIDTH * 2 + DISTANCE_BETWEEN_ITEMS * 2, DECK_POSITION.Y);
			for (int i = 1; i < FOUNDATION_POSITIONS.Length; i++) {
				FOUNDATION_POSITIONS[i].X = FOUNDATION_POSITIONS[i - 1].X + CARD_WIDTH + DISTANCE_BETWEEN_ITEMS;
				FOUNDATION_POSITIONS[i].Y = FOUNDATION_POSITIONS[i - 1].Y;
			}
			TABLEAU_POSITIONS = new Vector2[7];
			TABLEAU_POSITIONS[0] = new Vector2(DECK_POSITION.X, DECK_POSITION.Y + CARD_HEIGHT + DISTANCE_BETWEEN_ITEMS * 3);
			for (int i = 1; i < TABLEAU_POSITIONS.Length; i++) {
				TABLEAU_POSITIONS[i].X = TABLEAU_POSITIONS[i - 1].X + CARD_WIDTH + DISTANCE_BETWEEN_ITEMS;
				TABLEAU_POSITIONS[i].Y = TABLEAU_POSITIONS[i - 1].Y;
			}
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			int tableauNum = 0;

			// add the various cards to the toDraw list for the Draw method later
			toDraw.Add(new DrawInfo(CARD_BACKING, DECK_POSITION, 0));
			toDraw.Add(new DrawInfo(getRandomUnusedCard(), DISCARD_POSITION, 1));
			foreach (Vector2 position in FOUNDATION_POSITIONS) {
				toDraw.Add(new DrawInfo(CARD_FOUNDATION, position, 1));
			}
			foreach (Vector2 tableauPosition in TABLEAU_POSITIONS) {
				Vector2 position = new Vector2(tableauPosition.X, tableauPosition.Y);
				for (int i = 0; i < tableauNum; i++) {
					toDraw.Add(new DrawInfo(CARD_BACKING, position, 1/((float) i + 1)));
					position.Y += STACKING_OFFSET;
				}
				toDraw.Add(new DrawInfo(getRandomUnusedCard(), position, 0));
				tableauNum++;
			}

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			cards = Content.Load<Texture2D>(@"Images/Cards");
			logo = Content.Load<Texture2D>(@"Images/logo");
			logoPosition = new Vector2(Window.ClientBounds.Width / 2 - logo.Width / 4, DECK_POSITION.Y + CARD_HEIGHT - 15);
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(bgColor);

			spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
			foreach (DrawInfo card in toDraw) {
				drawCard(card.card, card.position, card.zIndex);
			}
			spriteBatch.End();
			// use a seperate SpriteBatch.Begin for the logo, to ensure it draws on top.
			spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
			spriteBatch.Draw(logo, logoPosition, null, Color.White, 0f, Vector2.Zero, .50f, SpriteEffects.None, 0f);
			spriteBatch.End();

			base.Draw(gameTime);
		}
		
		/// <summary>
		/// Draws a given card at a position and z-index.
		/// </summary>
		/// <param name="card">Point on the sprite sheet grid corresponding to the card.</param>
		/// <param name="position">Position at which to draw the card.</param>
		/// <param name="zIndex">z-index to use with SpriteBatch.Draw()</param>
		protected void drawCard(Point card, Vector2 position, float zIndex)
		{
			Point startPoint = new Point((CARD_WIDTH + 1) * card.X + 1, (CARD_HEIGHT + 1) * card.Y + 1);
			Rectangle currentCard;
			if (card.X < 0 || card.X > GRID_WIDTH) {
				throw new ArgumentOutOfRangeException(String.Format("Card x-value must be between 0 and {0}, inclusive.", GRID_WIDTH));
			} else if (card.Y < 0 || card.Y > GRID_HEIGHT) {
				throw new ArgumentOutOfRangeException(String.Format("Card y-value must be between 0 and {0}, inclusive.", GRID_HEIGHT));
			}
			currentCard = new Rectangle(startPoint.X, startPoint.Y, CARD_WIDTH, CARD_HEIGHT);
			spriteBatch.Draw(cards, position, currentCard, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, zIndex);
		}

		/// <summary>
		/// Gets a card that has not be previously gotten from this method in a random fashion.
		/// </summary>
		/// <returns>A Point object for use with setCurrentCard(Point) (Point(x, y) represents a card on the Cards.png grid).</returns>
		protected Point getRandomUnusedCard()
		{
			Point card;
			int numCards = (GRID_HEIGHT + 1) * (GRID_WIDTH + 1);
			if (usedCards.Count() >= numCards) {
				throw new InvalidOperationException("All cards have been used.");
			}
			do {
				card = new Point(rng.Next(0, GRID_WIDTH + 1), rng.Next(0, GRID_HEIGHT + 1));
			} while (usedCards.Contains(card));
			usedCards.Add(card);
			return card;
		}
	}
}
