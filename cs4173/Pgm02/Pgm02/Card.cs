// Bill Good
// Assignment 2

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


namespace Pgm02
{
	#region Enumerations
	public enum CardRank
	{
		Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King,
	}

	public enum CardSuit
	{
		Spades, Hearts, Clubs, Diamonds,
	}

	public enum CardColor
	{
		Red, Black,
	}

	public enum CardLocation
	{
		Stock, Waste, Foundation, Tableau
	}
	#endregion

	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class Card : Microsoft.Xna.Framework.DrawableGameComponent
	{
		#region Fields
		public static readonly Point CardSize = new Point(71, 107);
		static readonly Point GridSize = new Point(11, 5);
		static readonly Rectangle CardBacking = new Rectangle(576, 432, CardSize.X, CardSize.Y);

		public readonly CardSuit Suit;
		public readonly CardRank Rank;
		public readonly Point SpriteSheetIndex;
		CardLocation location;
		int pileNum; // which pile card's in... only defined for foundations and tableaux
		int pileIndex; // where card is in the pile... only defined for tableaux and waste
		Rectangle _destination;
		Rectangle texture; // set this in constructor please!
		bool locationChanged = false;
		bool isUpturned = false; // true if card rank and suit visible
		bool isHighlighted = false;

		static Game staticGame;
		static Texture2D spriteSheet;
		static SpriteBatch spriteBatch;
		static List<WeakReference> allCards = new List<WeakReference>(); // used for position manipulation on screen size change

		// Positions
		static Point stockPosition;
		static Point wastePosition;
		static Point[] foundationPositions;
		static Point[] tableauPositions;
		static int spacing;
		const int StackOffset = 20;
		#endregion

		#region Properties
		public CardLocation Location {
			get {
				return location;
			}
		}

		public int PileNumber {
			get {
				return pileNum;
			}
		}

		public int PileIndex {
			get {
				return pileIndex;
			}
		}

		public Boolean IsHighlighted {
			get {
				return isHighlighted;
			}
		}

		public Boolean IsUpturned {
			get {
				return isUpturned;
			}
		}

		public CardColor Color {
			get {
				if (Suit == CardSuit.Clubs || Suit == CardSuit.Spades) {
					return CardColor.Black;
				} else {
					return CardColor.Red;
				}
			}
		}

		public Rectangle Destination {
			get {
				if (locationChanged) {
					Point drawPosition;
					switch (location) {
						case CardLocation.Foundation:
							drawPosition = foundationPositions[pileNum];
							break;
						case CardLocation.Stock:
							drawPosition = stockPosition;
							break;
						case CardLocation.Tableau:
							drawPosition = tableauPositions[pileNum];
							drawPosition.Y += pileIndex * StackOffset;
							break;
						case CardLocation.Waste:
							drawPosition = wastePosition;
							drawPosition.X += pileIndex * StackOffset;
							break;
						default:
							drawPosition = Point.Zero;
							break;
					}
					_destination = new Rectangle(drawPosition.X, drawPosition.Y, CardSize.X, CardSize.Y);
					locationChanged = false;
				}
				return _destination;
			}
		}

		protected Rectangle Source {
			get {
				if (this.isUpturned) {
					return texture;
				}
				return CardBacking;
			}
		}

		public static Point StockPosition {
			get {
				return Card.stockPosition;
			}
		}

		public static Point WastePosition {
			get {
				return Card.wastePosition;
			}
		}

		public static Point[] FoundationPositions {
			get {
				return Card.foundationPositions;
			}
		}

		public static Point[] TableauPositions {
			get {
				return Card.tableauPositions;
			}
		}

		public static Texture2D SpriteSheet {
			get {
				return Card.spriteSheet;
			}
		}
		#endregion

		#region Methods
		public Card(Game game, CardSuit suit, CardRank rank)
			: base(game) {
			Card.staticGame = game;
			int cardNum;

			this.Suit = suit;
			this.Rank = rank;
			cardNum = 13 * (int) suit + (int) rank;
			SpriteSheetIndex = new Point(cardNum % GridSize.X, cardNum / GridSize.X);
			texture = new Rectangle((CardSize.X + 1) * SpriteSheetIndex.X + 1, (CardSize.Y + 1) * SpriteSheetIndex.Y + 1, CardSize.X, CardSize.Y);
			Card.allCards.Add(new WeakReference(this));
		}

		/// <summary>
		/// Flip this card.
		/// </summary>
		public void Flip() {
			isUpturned = !isUpturned;
		}

		public void ToggleHighlight() {
			isHighlighted = !isHighlighted;
		}

		public void Highlight() {
			isHighlighted = true;
		}

		public void Unhighlight() {
			isHighlighted = false;
		}

		public override string ToString() {
			string rank = System.Enum.GetName(typeof(CardRank), this.Rank).ToLower();
			string suit = System.Enum.GetName(typeof(CardSuit), this.Suit).ToLower();
			return String.Format("Card: {0} of {1}, sprite sheet @ {2}", rank, suit, SpriteSheetIndex);
		}

		public void SetLocation(CardLocation location, int pileNum, int pileIndex) {
			if (pileNum < 0) {
				throw new ArgumentException(String.Format("pileNum must be positive or zero, got {0} for card {1}\n", pileNum, this));
			}
			if (pileIndex < 0) {
				throw new ArgumentException(String.Format("pileIndex must be positive or zero, got {0} for card {1}\n", pileIndex, this));
			}
			this.location = location;
			this.pileNum = pileNum;
			this.pileIndex = pileIndex;
			this.locationChanged = true;
		}

		public void SetLocation(CardLocation location, int arg) {
			switch (location) {
				case CardLocation.Tableau:
					throw new ArgumentException("SetLocation must receive two int arguments for tableau locations.");
				case CardLocation.Foundation:
					SetLocation(location, arg, 0);
					break;
				case CardLocation.Stock:
					SetLocation(location);
					break;
				case CardLocation.Waste:
					SetLocation(location, 0, arg);
					break;
			}
		}

		public void SetLocation(CardLocation location) {
			// somewhat unfortunate that these methods have to exist but the card must know where to draw itself
			if (location != CardLocation.Stock) {
				throw new ArgumentException("SetLocation must receive at least one int arguments for locations other than the stock.");
			}
			SetLocation(location, 0, 0);
		}
		#endregion

		#region Game Methods
		/* I realized all of just after finishing that this is _not_ the intended use of DrawableGameComponent...
		 * Oh well, it works how it is. Although it'd certainly be an interesting exercise to convert to the proper
		 * structure/methodology. Lesson learned: read the docs before/while exploring new territory.
		 */
		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize() {
			base.Initialize();
		}

		/// <summary>
		/// Allows the game component to draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Draw(GameTime gameTime) {
			//spriteBatch.Draw(spriteSheet, Destination, Source, IsHighlighted ? Microsoft.Xna.Framework.Graphics.Color.LightGray : Microsoft.Xna.Framework.Graphics.Color.White);
			spriteBatch.Draw(spriteSheet, Destination, Source, IsHighlighted ? Microsoft.Xna.Framework.Graphics.Color.LightGray : Microsoft.Xna.Framework.Graphics.Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			base.Draw(gameTime);
		}
		#endregion

		#region Static Methods
		/// <summary>
		/// Sets the sprite sheet used for all cards.
		/// </summary>
		/// <param name="texture"></param>
		public static void SetTexture(Texture2D texture) {
			Card.spriteSheet = texture;
		}

		public static void SetSpriteBatch(SpriteBatch spriteBatch) {
			Card.spriteBatch = spriteBatch;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static List<Card> MakeDeck(Game game) {
			List<Card> deck = new List<Card>(52);

			foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit))) {
				foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank))) {
					deck.Add(new Card(game, suit, rank));
				}
			}
			return deck;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="deck"></param>
		public static List<Card> ShuffleDeck(List<Card> deck) {
			Random random = new Random();

			for (int i = 0; i < deck.Count; i++) {
				int swapWith = random.Next(i, deck.Count);
				Card temp = deck[i];
				deck[i] = deck[swapWith];
				deck[swapWith] = temp;
			}
			return deck;
		}

		public static void SetPositioning() {
			spacing = (Card.staticGame.Window.ClientBounds.Width - (7 * Card.CardSize.X)) / 8;
			stockPosition = new Point(spacing, spacing);
			wastePosition = new Point(stockPosition.X + CardSize.X + spacing, stockPosition.Y);
			foundationPositions = new Point[4];
			foundationPositions[0] = new Point(wastePosition.X + CardSize.X * 2 + spacing * 2, stockPosition.Y);
			for (int i = 1; i < foundationPositions.Length; i++) {
				foundationPositions[i].X = foundationPositions[i - 1].X + CardSize.X + spacing;
				foundationPositions[i].Y = foundationPositions[i - 1].Y;
			}
			tableauPositions = new Point[7];
			tableauPositions[0] = new Point(stockPosition.X, stockPosition.Y + CardSize.Y + spacing * 3);
			for (int i = 1; i < tableauPositions.Length; i++) {
				tableauPositions[i].X = tableauPositions[i - 1].X + CardSize.X + spacing;
				tableauPositions[i].Y = tableauPositions[i - 1].Y;
			}
			foreach (WeakReference wr in allCards) {
				Card card = wr.Target as Card;
				if (card != null) {
					card.locationChanged = true;
				}
			}
		}
		#endregion
	}
}