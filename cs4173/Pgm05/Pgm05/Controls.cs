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


namespace Pgm05
{
	public abstract class Controls : GameComponent
	{
		public abstract bool IsLeftPressed {
			get;
		}

		public abstract bool IsRightPressed {
			get;
		}

		public abstract bool PlayPressed {
			get;
		}

		public abstract bool QuitPressed {
			get;
		}
		
		public Controls(Game game)
			: base(game) {
			UpdateOrder = BrickInvaders.ControlPriority;
		}
	}

	public class KeyboardControls : Controls
	{
		public KeyboardState PrevFrame {
			get;
			private set;
		}

		public KeyboardState CurrFrame {
			get;
			private set;
		}

		public override bool IsLeftPressed {
			get {
				return CurrFrame.IsKeyDown(Keys.Left);
			}
		}

		public override bool IsRightPressed {
			get {
				return CurrFrame.IsKeyDown(Keys.Right);
			}
		}

		public override bool PlayPressed {
			get {
				return PrevFrame.IsKeyDown(Keys.Enter) && CurrFrame.IsKeyUp(Keys.Enter) ? true : false;
			}
		}

		public override bool QuitPressed {
			get {
				return PrevFrame.IsKeyDown(Keys.Q) && CurrFrame.IsKeyUp(Keys.Q) ? true : false;
			}
		}

		public KeyboardState State {
			get {
				return CurrFrame;
			}
		}

		public KeyboardControls(Game game)
			: base(game) {
		}

		public override void Initialize() {
			PrevFrame = CurrFrame = Keyboard.GetState();
			base.Initialize();
		}

		public override void Update(GameTime gameTime) {
			PrevFrame = CurrFrame;
			CurrFrame = Keyboard.GetState();
			if (CurrFrame.IsKeyDown(Keys.Escape)) {
				Game.Exit();
			}
			base.Update(gameTime);
		}
	}

	public class GamePadControls : Controls
	{
		public GamePadState PrevFrame {
			get;
			private set;
		}

		public GamePadState CurrFrame {
			get;
			private set;
		}

		public override bool IsLeftPressed {
			get {
				return PrevFrame.ThumbSticks.Left.X < 0;
			}
		}

		public override bool IsRightPressed {
			get {
				return PrevFrame.ThumbSticks.Left.X > 0;
			}
		}

		public override bool PlayPressed {
			get {
				return PrevFrame.Buttons.A == ButtonState.Pressed && CurrFrame.Buttons.A == ButtonState.Released ? true : false;
			}
		}

		public override bool QuitPressed {
			get {
				return PrevFrame.Buttons.X == ButtonState.Pressed && CurrFrame.Buttons.X == ButtonState.Released ? true : false;
			}
		}

		public GamePadState State {
			get {
				return CurrFrame;
			}
		}

		public readonly PlayerIndex PIndex;

		public GamePadControls(Game game, PlayerIndex pIndex)
			: base(game) {
			PIndex = pIndex;
		}

		public GamePadControls(Game game)
			: base(game) {
			PIndex = PlayerIndex.One;
		}

		public override void Initialize() {
			PrevFrame = CurrFrame = GamePad.GetState(PIndex);
			base.Initialize();
		}

		public override void Update(GameTime gameTime) {
			PrevFrame = CurrFrame;
			CurrFrame = GamePad.GetState(PIndex);
			if (CurrFrame.Buttons.Back == ButtonState.Pressed) {
				Game.Exit();
			}
			base.Update(gameTime);
		}
	}
}