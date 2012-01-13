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

/*
 * Functionality/features implemented not required in the assignment (that I can still remember):
 *	Window is resizeable and items move proportionally.
 *	On the opening screen, one may change the number of cards drawn between 1 and 3 -- however this is the only time,
 *		the game must be restarted to change it again.
 *	If the user pauses the game (my words for hitting "new game" button), they may press c (otherwise undocumented)
 *		to unpause the game and then win it (during testing the winning/restart code I realized there's only so much solitare
 *		I can play, so I decided to make the game win for me :).
 *	Changed card backing to Pistol Pete.
 *	Button becomes depressed upon click-down.
 * I think that's it, could be more.
 */
namespace Pgm02
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Klondike : Microsoft.Xna.Framework.Game
	{
		#region Enumerations
		enum GameState
		{
			New, Playing, Won, Paused, // paused used to signify state in which new game diag is up
		}
		#endregion

		#region Fields
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D newGameImage;
		Texture2D newGameDiag;
		Texture2D wonImage;
		Texture2D button;
		Texture2D button_pressed;
		Song song;
		SpriteFont font;
		GameState currentState;
		KeyboardState prevKeyboardState;
		MouseState prevMouseState;
		Rectangle prevClientBounds = Rectangle.Empty;
		Rectangle buttonArea = Rectangle.Empty;
		Point clickPoint = Point.Zero;
		int score;
		int numCardsToFlip = 3;
		bool buttonPressed = false;
		List<Card> deck;
		List<Card> stock;
		List<Card> waste;
		List<Card>[] foundations;
		List<Card>[] tableaux;
		List<Card> highlightedCards;

		const int WindowDim = 640;
		readonly Color BGColor = Color.Green;
		readonly Color EmptySlotColor = Color.ForestGreen;
		readonly Rectangle EmptySlot = new Rectangle(648, 432, Card.CardSize.X, Card.CardSize.Y);
		#endregion

		#region Properties
		String Status {
			get {
				return String.Format("Score: {0}, flipping {1} card{2}.", score, numCardsToFlip, numCardsToFlip > 1 ? "s" : "");
			}
		}
		#endregion

		#region Methods
		public Klondike() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}
		
		/// <summary>
		/// Sets up a new game.
		/// </summary>
		void NewGame() {
			Card card;
			score = 0;
			stock = new List<Card>();
			waste = new List<Card>();
			foundations = new List<Card>[4];
			tableaux = new List<Card>[7];
			highlightedCards = new List<Card>();
			deck = new List<Card>(Card.ShuffleDeck(Card.MakeDeck(this)));
			for (int i = 0; i < foundations.Count(); i++) {
				foundations[i] = new List<Card>();
			}
			for (int i = 0; i < tableaux.Count(); i++) {
				tableaux[i] = new List<Card>();
			}

			int tableauNum = 1;
			foreach (List<Card> tableau in tableaux) {
				for (int cardNum = 1; cardNum <= tableauNum; cardNum++) {
					card = deck.Pop();
					if (tableauNum == cardNum) {
					    card.Flip();
					}
					card.SetLocation(CardLocation.Tableau, tableauNum - 1, cardNum - 1);
					tableau.Add(card);
				}
				tableauNum++;
			}
			while (deck.Count() > 0) {
				card = deck.Pop();
				card.SetLocation(CardLocation.Stock);
				stock.Add(card);
			}
		}

		/// <summary>
		/// Handler for a click made when no cards are in the highlight queue.
		/// Will look for a card and act on it, if it can't find one it will attempt
		/// to look for a special location and act on it.
		/// </summary>
		/// <param name="point">Location of click</param>
		void HandleFirstClick(Point point) {
			Card clicked = GetCardAtPoint(point);
			CardLocation? clickedLocation = GetLocationAtPoint(point);
			if (clicked != null) {
				HandleFirstClickOnCard(point, clicked);
			} else if (clickedLocation != null) {
				HandleFirstClickOnLocation(point, (CardLocation) clickedLocation);
			}
		}

		/// <summary>
		/// Handles an initial click (i.e. no cards highlighted) on a card.
		/// </summary>
		/// <param name="point">Location of click</param>
		/// <param name="clickedCard">Card which was clicked</param>
		void HandleFirstClickOnCard(Point point, Card clickedCard) {
			switch (clickedCard.Location) {
				case CardLocation.Waste:
					if (clickedCard == waste[waste.Count - 1]) {
						clickedCard.Highlight();
						highlightedCards.Add(clickedCard);
					}
					break;
				case CardLocation.Tableau:
					if (clickedCard.IsUpturned) {
						List<Card> sublist = tableaux[clickedCard.PileNumber].GetRange(clickedCard.PileIndex, tableaux[clickedCard.PileNumber].Count - clickedCard.PileIndex);
						CardColor lastColor = clickedCard.Color == CardColor.Red ? CardColor.Black : CardColor.Red;
						foreach (Card card in sublist) {
							if (lastColor != card.Color) {
								card.Highlight();
								highlightedCards.Add(card);
								lastColor = card.Color;
							} else {
								break;
							}
						}
						if (!sublist[sublist.Count - 1].IsHighlighted) {
							foreach (Card card in sublist) {
								card.Unhighlight();
							}
							highlightedCards.Clear();
						}
					} else {
						if (clickedCard.PileIndex == (tableaux[clickedCard.PileNumber].Count - 1)) {
							clickedCard.Flip();
							UpdateScore(CardLocation.Tableau);
						}
					}
					break;
			}
		}

		/// <summary>
		/// Handles an initial click (i.e. no cards highlighted) on a special location.
		/// </summary>
		/// <param name="point">Location of click</param>
		/// <param name="clickedLocation">Location which was clicked</param>
		void HandleFirstClickOnLocation(Point point, CardLocation clickedLocation) {
			switch (clickedLocation) {
				case CardLocation.Stock:
					if (stock.Count == 0) { // reset stock
						foreach (Card card in waste.Reverse<Card>()) {
							if (card.IsUpturned) {
								card.Flip();
							}
							card.SetLocation(CardLocation.Stock);
							stock.Add(card);
						}
						waste.Clear();
					} else { // flip over up to numCardsToFlip
						foreach (Card card in waste) {
							card.SetLocation(CardLocation.Waste, 0);
						}
						for (int i = 0; i < numCardsToFlip && stock.Count != 0; i++) {
							Card card = stock.Pop();
							card.SetLocation(CardLocation.Waste, i);
							if (!card.IsUpturned) {
								card.Flip();
							}
							waste.Add(card);
						}
					}
					break;
			}
		}

		/// <summary>
		/// Handler for a click made when there are cards in the highlight queue.
		/// Will look for a card and act on it, if it can't find one it will attempt
		/// to look for a special location and act on it.
		/// Finally, it empties the highlight queue.
		/// </summary>
		/// <param name="point">Location of click</param>
		void HandleSecondClick(Point point) {
			CardLocation? clickLocation = GetLocationAtPoint(point);
			Card clickedCard = GetCardAtPoint(point);

			if (clickedCard != null) {
				HandleSecondClickOnCard(point, clickedCard);
			} else if (clickLocation != null) {
				HandleSecondClickOnLocation(point, (CardLocation) clickLocation);
			}
			foreach (Card card in highlightedCards) {
				card.Unhighlight();
			}
			highlightedCards.Clear();
		}

		/// <summary>
		/// Handles a second click (i.e. the highlight queue is populated) on a card.
		/// Since the only place we can move onto a card is a tableau
		/// (from another tableau or the waste), that's the only scenario handled.
		/// </summary>
		/// <param name="point">Location of click</param>
		/// <param name="clickedCard">Card which was clicked</param>
		void HandleSecondClickOnCard(Point point, Card clickedCard) {
			if (clickedCard.Location != CardLocation.Tableau) {
				return;
			}
			int tableauNum = clickedCard.PileNumber;
			int lastIndex = tableaux[tableauNum].Last().PileIndex;
			if (IsValidMove(CardLocation.Tableau, tableauNum, highlightedCards)) {
				switch (highlightedCards.First().Location) {
					case CardLocation.Waste:
						waste.Remove(highlightedCards[0]);
						UpdateScore(CardLocation.Waste, CardLocation.Tableau);
						break;
					case CardLocation.Tableau:
						foreach (Card card in highlightedCards) {
							tableaux[card.PileNumber].Remove(card);
						}
						break;
				}
				int count = lastIndex + 1;
				foreach (Card card in highlightedCards) {
					card.SetLocation(CardLocation.Tableau, tableauNum, count);
					tableaux[tableauNum].Add(card);
					count++;
				}
			}
		}

		/// <summary>
		/// Handles a second click (i.e. the highlight queue is populated) on a special location.
		/// Handles moving card(s) to a foundation or an empty tableau.
		/// </summary>
		/// <param name="point">Location of click</param>
		/// <param name="clickedLocation">Location which was clicked</param>
		void HandleSecondClickOnLocation(Point point, CardLocation clickedLocation) {
			switch (clickedLocation) {
				case CardLocation.Foundation:
					int foundationNum = GetFoundationNumberAtPoint(point);
					if (IsValidMove(CardLocation.Foundation, foundationNum, highlightedCards)) {
						// preform move
						Card toMove = highlightedCards[0];
						switch (toMove.Location) {
							case CardLocation.Waste:
								waste.Remove(toMove);
								UpdateScore(CardLocation.Waste, CardLocation.Foundation);
								break;
							case CardLocation.Tableau:
								tableaux[toMove.PileNumber].Remove(toMove);
								UpdateScore(CardLocation.Tableau, CardLocation.Foundation);
								break;
						}
						toMove.SetLocation(CardLocation.Foundation, foundationNum);
						foundations[foundationNum].Add(toMove);
					}
					break;
				case CardLocation.Tableau: // only works on empty tableaux
					int tableauNum = GetTableauNumberAtPoint(point);
					if (IsValidMove(CardLocation.Tableau, tableauNum, highlightedCards)) {
						switch (highlightedCards[0].Location) {
							case CardLocation.Waste:
								waste.Remove(highlightedCards[0]);
								UpdateScore(CardLocation.Waste, CardLocation.Tableau);
								break;
							case CardLocation.Tableau:
								foreach (Card card in highlightedCards) {
									tableaux[card.PileNumber].Remove(card);
								}
								break;
						}
						int count = 0;
						foreach (Card card in highlightedCards) {
							card.SetLocation(CardLocation.Tableau, tableauNum, count);
							tableaux[tableauNum].Add(card);
							count++;
						}
					}
					break;
			}
		}

		/// <summary>
		/// Checks that moving a set of cards to a certain location is within the rules of our game.
		/// </summary>
		/// <param name="to">Which sort of location we're moving to.</param>
		/// <param name="pileNum">The index of the foundation or tableau we're moving to.</param>
		/// <param name="cards">A list of cards to be moved. MUST BE A PROPER PILE (i.e. alternating colors), THIS METHOD ASSUMES SO.</param>
		/// <returns>True if proper move, false otherwise.</returns>
		bool IsValidMove(CardLocation to, int pileNum, List<Card> cards) {
			if (cards.Count == 0) {
				return false;
			}
			switch (to) {
				case CardLocation.Foundation:
					List<Card> foundation = foundations[pileNum];
					if (cards.Count != 1) {
						return false;
					}
					if (foundation.Count == 0 && cards[0].Rank == CardRank.Ace) {
						return true;
					}
					if (foundation.Count > 0) {
						Card lastCard = foundation.Last();
						if (lastCard.Suit == cards[0].Suit && lastCard.Rank == (cards[0].Rank - 1)) {
							return true;
						}
					}
					return false;
				case CardLocation.Tableau:
					List<Card> tableau = tableaux[pileNum];
					Card first = cards.First();
					Card last;
					if (tableau.Count == 0 && first.Rank == CardRank.King) {
						return true;
					} else if (tableau.Count == 0 && first.Rank != CardRank.King) {
						return false;
					}
					last = tableau.Last();
					if (first.Color != last.Color && first.Rank == (last.Rank - 1)) {
						return true;
					}
					break;
			}
			return false;
		}

		/// <summary>
		/// Increments score where there's only one location used (ex. a card is flipped on a tableau).
		/// </summary>
		/// <param name="loc">The CardLocation where the action occured.</param>
		void UpdateScore(CardLocation loc) {
			//Turn over Tableau card	5
			if (loc == CardLocation.Tableau) {
				score += 5;
			}
		}

		/// <summary>
		/// Increments the score based upon where a card was moved to and from.
		/// </summary>
		/// <param name="from">Where the card began.</param>
		/// <param name="to">Where the card was moved.</param>
		void UpdateScore(CardLocation from, CardLocation to) {
			//Waste to Tableau	5
			//Waste to Foundation	10
			//Tableau to Foundation	10
			if (from == CardLocation.Waste && to == CardLocation.Tableau) {
				score += 5;
			} else if (from == CardLocation.Waste && to == CardLocation.Foundation) {
				score += 10;
			} else if (from == CardLocation.Tableau && to == CardLocation.Foundation) {
				score += 10;
			}
		}

		/// <summary>
		/// Checks for game-winning conditions.
		/// </summary>
		/// <returns>True is game has been won.</returns>
		bool IsGameWon() {
			foreach (List<Card> foundation in foundations) {
				if (foundation.Count == 0 || foundation.Last().Rank != CardRank.King) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Looks at a point to see if there is a either a card in a tableau or the top card of the waste.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		Card GetCardAtPoint(Point point) {
			foreach (List<Card> tableau in tableaux) {
				if (tableau.Count == 0) {
					continue;
				}
				foreach (Card card in tableau.Reverse<Card>()) {
					if (card.Destination.Contains(point)) {
						return card;
					}
				}
			}
			if (waste.Count > 0) {
				if ((waste[waste.Count - 1]).Destination.Contains(point)) {
					return waste[waste.Count - 1];
				}
			}
			return null;
		}

		/// <summary>
		/// If the given point is contained in the stock, a tableau
		/// (only the "base" location, intended use is for empty tableaux) or a foundation.
		/// </summary>
		/// <param name="point">point to check</param>
		/// <returns>A CardLocation, or null.</returns>
		CardLocation? GetLocationAtPoint(Point point) {
			if (new Rectangle(Card.StockPosition.X, Card.StockPosition.Y, Card.CardSize.X, Card.CardSize.Y).Contains(point)) {
				return CardLocation.Stock;
			}
			foreach (Point tableau in Card.TableauPositions) {
				if (new Rectangle(tableau.X, tableau.Y, Card.CardSize.X, Card.CardSize.Y).Contains(point)) {
					return CardLocation.Tableau;
				}
			}
			foreach (Point foundation in Card.FoundationPositions) {
				if (new Rectangle(foundation.X, foundation.Y, Card.CardSize.X, Card.CardSize.Y).Contains(point)) {
					return CardLocation.Foundation;
				}
			}
			return null;
		}

		/// <summary>
		/// Given a point known to reside in a tableau (base location), gives the 0-based index of the
		/// tableau from the leftmost one.
		/// </summary>
		/// <param name="point">point to check</param>
		/// <returns>0-based index of tableau</returns>
		/// <exception cref="ArgumentException">Thrown if point is not in a tableau base.</exception>
		int GetTableauNumberAtPoint(Point point) {
			int count = 0;
			foreach (Point tableau in Card.TableauPositions) {
				if (new Rectangle(tableau.X, tableau.Y, Card.CardSize.X, Card.CardSize.Y).Contains(point)) {
					return count;
				}
				count++;
			}
			throw new ArgumentException("The Point given to GetTableauNumberAtPoint must be in a tableau!");
		}

		/// <summary>
		/// Given a point known to reside in a foundation, gives the 0-based index of the
		/// foundation from the leftmost one.
		/// </summary>
		/// <param name="point">point to check</param>
		/// <returns>0-based index of foundation</returns>
		/// <exception cref="ArgumentException">Thrown if point is not in a foundation base.</exception>
		int GetFoundationNumberAtPoint(Point point) {
			int count = 0;
			foreach (Point foundation in Card.FoundationPositions) {
				if (new Rectangle(foundation.X, foundation.Y, Card.CardSize.X, Card.CardSize.Y).Contains(point)) {
					return count;
				}
				count++;
			}
			throw new ArgumentException("The Point given to GetFoundationNumberAtPoint must be in a foundation!");
		}

		/// <summary>
		/// Cheat at solitare! (You're only cheating yourself)
		/// </summary>
		void Cheat() {
			foreach (List<Card> foundation in foundations) {
				while (foundation.Count > 0) {
					Card card = foundation.Pop();
					card.SetLocation(CardLocation.Stock);
					stock.Add(card);
				}
			}
			foreach (List<Card> tableau in tableaux) {
				while (tableau.Count > 0) {
					Card card = tableau.Pop();
					card.SetLocation(CardLocation.Stock);
					stock.Add(card);
				}
			}
			while (waste.Count > 0) {
				Card card = waste.Pop();
				card.SetLocation(CardLocation.Stock);
				stock.Add(card);
			}
			// ok all cards in stock, now move them to the piles
			int foundationNum = 0;
			foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit))) {
				foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank))) {
					Card card;
					foreach (Card test in stock) {
						if (test.Suit == suit && test.Rank == rank) {
							stock.Remove(test);
							card = test;
							foundations[foundationNum].Add(card);
							card.SetLocation(CardLocation.Foundation, foundationNum);
							if (!card.IsUpturned) {
								card.Flip();
							}
							break;
						}
					}
				}
				foundationNum++;
			}
		}

		/// <summary>
		/// Looks to see if a keyboard key was pressed in the previous update and is no longer pressed.
		/// </summary>
		/// <param name="key">Key to look for.</param>
		/// <returns>True if key was pressed and is now released, false otherwise.</returns>
		bool KeyHit(Keys key, KeyboardState kbState) {
			return prevKeyboardState.GetPressedKeys().Contains(key) && !kbState.GetPressedKeys().Contains(key);
		}
		#endregion
		
		#region Game Methods
		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize() {
			graphics.PreferredBackBufferWidth = graphics.PreferredBackBufferHeight = WindowDim;
			graphics.ApplyChanges();

			Window.Title = "Klondike Solitaire – CS 4173, Assignment 2 – Bill Good";
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
			currentState = GameState.New;
			NewGame();

			Card.SetPositioning();

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
			spriteBatch = new SpriteBatch(GraphicsDevice);

			Card.SetSpriteBatch(spriteBatch);
			/// Pistol Pete backing copyright Oklahoma State University
			Card.SetTexture(Content.Load<Texture2D>(@"Images\Cards2"));
			newGameImage = Content.Load<Texture2D>(@"Images\start_image");
			newGameDiag = Content.Load<Texture2D>(@"Images\new_game_diag");
			wonImage = Content.Load<Texture2D>(@"Images\won_image");
			button = Content.Load<Texture2D>(@"Images\button");
			button_pressed = Content.Load<Texture2D>(@"Images\button_pressed");
			/// Clip from "We Like To Party", copyright 1998 by "Vengaboys"
			song = Content.Load<Song>(@"Music\We Like To Party");
			font = Content.Load<SpriteFont>(@"Fonts\Verdana");

			buttonArea = new Rectangle((int) font.MeasureString(Status).X + 10, (int) Window.ClientBounds.Height - button_pressed.Height, button.Width, button.Height);
			MediaPlayer.IsRepeating = true;
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent() {
			// Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime) {
			KeyboardState kbState = Keyboard.GetState();
			MouseState mouseState = Mouse.GetState();
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			if (gameTime.IsRunningSlowly && gameTime.ElapsedRealTime.Seconds > 5) { // s/b enough time to load
				Console.WriteLine("game running slowly?");
			}
			if (!Window.ClientBounds.Equals(prevClientBounds)) { // window size changed, update positioning (not required functionality)
				Card.SetPositioning();
				prevClientBounds = Window.ClientBounds;
			}
			if (!this.IsActive) { // don't do anything if we don't have focus (prevents responding to clicks outside window)
				base.Update(gameTime);
				return;
			}
			switch (currentState) { // main game functionality
				case GameState.New:
					if (MediaPlayer.State != MediaState.Playing) {
						MediaPlayer.Play(song);
					}
					if (prevKeyboardState != kbState) {
						if (KeyHit(Keys.D1, kbState) || KeyHit(Keys.NumPad1, kbState)) {
							numCardsToFlip = 1;
						} else if (KeyHit(Keys.D3, kbState) || KeyHit(Keys.NumPad3, kbState)) {
							numCardsToFlip = 3;
						} else if (prevKeyboardState.GetPressedKeys().Length < kbState.GetPressedKeys().Length) {
							break;
						} else {
							MediaPlayer.Stop();
							currentState = GameState.Playing;
						}
					}
					break;
				case GameState.Playing:
					if (IsGameWon()) {
						currentState = GameState.Won;
						break;
					}
					if (buttonPressed == true && !buttonArea.Contains(mouseState.Point())) {
						buttonPressed = false;
					}
					if (prevMouseState.LeftButton != mouseState.LeftButton && mouseState.LeftButton == ButtonState.Pressed) {
						if (buttonArea.Contains(mouseState.Point())) {
							buttonPressed = true;
						}
					} else if (prevMouseState.LeftButton != mouseState.LeftButton && mouseState.LeftButton == ButtonState.Released) {
						if (buttonPressed == true && buttonArea.Contains(mouseState.Point())) {
							buttonPressed = false;
							currentState = GameState.Paused;
						} else if (highlightedCards.Count > 0) {
							HandleSecondClick(new Point(mouseState.X, mouseState.Y));
						} else {
							HandleFirstClick(new Point(mouseState.X, mouseState.Y));
						}
					}
					break;
				case GameState.Paused:
					if (prevKeyboardState != kbState) {
						if (KeyHit(Keys.Y, kbState)) {
							NewGame();
							currentState = GameState.Playing;
						} else if (KeyHit(Keys.N, kbState)) {
							currentState = GameState.Playing;
						} else if (KeyHit(Keys.C, kbState)) {
							currentState = GameState.Playing;
							Cheat();
						}
					}
					break;
				case GameState.Won:
					if (prevKeyboardState != kbState) {
						if (prevKeyboardState.GetPressedKeys().Length < kbState.GetPressedKeys().Length) {
							break;
						}
						NewGame();
						currentState = GameState.Playing;
					}
					break;
			}
			buttonArea = new Rectangle((int) font.MeasureString(Status).X + 10, (int) Window.ClientBounds.Height - button_pressed.Height, button.Width, button.Height);
			prevKeyboardState = kbState;
			prevMouseState = mouseState;
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(BGColor);
			spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);
			foreach (Point position in new Point[] { Card.StockPosition, Card.WastePosition }.Concat(Card.FoundationPositions).Concat(Card.TableauPositions)) {
				spriteBatch.Draw(Card.SpriteSheet, new Rectangle(position.X, position.Y, Card.CardSize.X, Card.CardSize.Y), EmptySlot, EmptySlotColor);
			}
			foreach (Card card in stock) {
				card.Draw(gameTime);
			}
			foreach (Card card in waste) {
				card.Draw(gameTime);
			}
			foreach (List<Card> foundation in foundations) {
				foreach (Card card in foundation) {
					card.Draw(gameTime);
				}
			}
			foreach (List<Card> tableau in tableaux) {
				foreach (Card card in tableau) {
					card.Draw(gameTime);
				}
			}
			spriteBatch.DrawString(font, Status, new Vector2(0, Window.ClientBounds.Height - font.MeasureString(Status).Y), Color.White);
			switch (currentState) {
				case GameState.New:
					spriteBatch.Draw(newGameImage, new Vector2(Window.ClientBounds.Width / 2 - newGameImage.Width / 2, Window.ClientBounds.Y / 2), Color.White);
					break;
				case GameState.Playing:
					// draw button
					if (buttonPressed) {
						spriteBatch.Draw(button_pressed, buttonArea, Color.White);
					} else {
						spriteBatch.Draw(button, buttonArea, Color.White);
					}
					break;
				case GameState.Paused:
					spriteBatch.Draw(newGameDiag, new Vector2(Window.ClientBounds.Width / 2 - newGameDiag.Width / 2, Window.ClientBounds.Y / 2), Color.White);
					break;
				case GameState.Won:
					spriteBatch.Draw(wonImage, new Vector2(Window.ClientBounds.Width / 2 - newGameImage.Width / 2, Window.ClientBounds.Y / 2), Color.White);
					break;
			}
			spriteBatch.End();
			base.Draw(gameTime);
		}
		#endregion
	}
	#region Extensions
	public static class MyExtensions
	{
		public static T Pop<T>(this List<T> list) {
			T obj = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return obj;
		}

		public static Point Point(this MouseState ms) {
			return new Point(ms.X, ms.Y);
		}
	}
	#endregion
}
