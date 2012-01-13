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
	/// <summary>
	/// Direction of a camera move (pan).
	/// </summary>
	public enum CameraDirection
	{
		Up,
		Down,
		Left,
		Right,
	}

	/// <summary>
	/// Data used by Camera to aid in monitoring.
	/// </summary>
	public struct MonitorInfo
	{
		public Point PixelDimensions;
		public Point AbsolutePosition;
	}

	/// <summary>
	/// Describes necessary class members needed for Camera to monitor them.
	/// </summary>
	public interface ICameraMonitorable
	{
		MonitorInfo GetMonitorInfo();
	}

	/// <summary>
	/// Class representing a "camera" controlling the viewport of a 2D game.
	/// Can monitor multiple objects to keep them in bounds.
	/// Can monitor a single object to keep it centered in the viewport (roughly centered that is, the bounded objects have priority).
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class Camera : Microsoft.Xna.Framework.GameComponent
	{
		/// <summary>
		/// Offset to be used with SpriteBatch.Draw, heart of Camera. Creates "panning" of camera.
		/// </summary>
		public Vector2 Offset {
			get {
				return offset; // much easier than using default get/set, don't have to o = Offset; o.X++; Offset = o a million times
			}
		}

		private Point WindowCenter {
			get {
				return new Point(((int) offset.X) + Game.Window.ClientBounds.Width / 2, ((int) offset.Y) + Game.Window.ClientBounds.Height / 2);
			}
		}

		List<ICameraMonitorable> boundeds;
		ICameraMonitorable centered; // only one may be centered at this point
		Vector2 offset;
		bool newCentered = false;

		public Camera(Game game)
			: base(game) {
			boundeds = new List<ICameraMonitorable>();
			offset = Vector2.Zero;
		}

		/// <summary>
		/// Set a bounded object to be monitored.
		/// </summary>
		/// <param name="monitorable">Object to be monitored</param>
		public void Monitor(ICameraMonitorable monitorable) {
			Monitor(monitorable, false);
		}

		/// <summary>
		/// Set an object to be monitored.
		/// </summary>
		/// <param name="monitorable">bject to be monitored</param>
		/// <param name="isCentered">True if this is to be the centered object</param>
		public void Monitor(ICameraMonitorable monitorable, bool isCentered) {
			if (isCentered) {
				centered = monitorable;
				newCentered = true;
			} else {
				boundeds.Add(monitorable);
			}
		}

		/// <summary>
		/// Remove object from those being monitored.
		/// </summary>
		/// <param name="monitorable">Object to be removed</param>
		public void UnMonitor(ICameraMonitorable monitorable) {
			while (boundeds.Contains(monitorable)) {
				boundeds.Remove(monitorable);
			}
			if (centered == monitorable) {
				newCentered = false;
				centered = null;
			}
		}

		/// <summary>
		/// Check if the camera can move in a direction in a number of pixels.
		/// </summary>
		/// <param name="dir">Direction</param>
		/// <param name="amount">Number of pixels</param>
		/// <returns>False if there are bounded objects blocking move, otherwise true.</returns>
		bool CanMove(CameraDirection dir, int amount) {
				// checks the bounded
			Vector2 newOffset = offset;
			foreach (ICameraMonitorable monitorable in boundeds) {
				MonitorInfo mi = monitorable.GetMonitorInfo();
				switch (dir) {
				case CameraDirection.Up:
					newOffset.Y -= amount;
					if (newOffset.Y <= mi.AbsolutePosition.Y) {
						return false;
					}
					break;
				case CameraDirection.Down:
					if (newOffset.Y + Game.Window.ClientBounds.Height >= mi.PixelDimensions.Y) {
						return false;
					}
					newOffset.Y += amount;
					break;
				case CameraDirection.Left:
					if (newOffset.X <= mi.AbsolutePosition.X) {
						return false;
					}
					newOffset.X -= amount;
					break;
				case CameraDirection.Right:
					if (newOffset.X + Game.Window.ClientBounds.Width >= mi.PixelDimensions.X) {
						return false;
					}
					newOffset.X += amount;
					break;
				}
			}
			return true;
		}

		/// <summary>
		/// Moves the camera, if possible.
		/// </summary>
		/// <param name="dir">Direction to move</param>
		/// <param name="amount">Number of pixels to move</param>
		public void Move(CameraDirection dir, int amount) {
			if (!CanMove(dir, amount)) {
				return;
			}
			switch (dir) {
			case CameraDirection.Down:
				offset.Y += amount;
				break;
			case CameraDirection.Left:
				offset.X -= amount;
				break;
			case CameraDirection.Right:
				offset.X += amount;
				break;
			case CameraDirection.Up:
				offset.Y -= amount;
				break;
			}
		}

		/// <summary>
		/// Make an attempt to center the object needing to be centered.
		/// Only moves the camera once at a time (although may be in a large chunk).
		/// </summary>
		void Center() {
			const int DefaultChange = 2;
			int change = DefaultChange;
			Point windowCenter = WindowCenter;
			MonitorInfo mi = centered.GetMonitorInfo();
			if (windowCenter != mi.AbsolutePosition) {
				if (Math.Abs(windowCenter.Y - mi.AbsolutePosition.Y) != 0) {
					change = Math.Min(Math.Abs(windowCenter.Y - mi.AbsolutePosition.Y), DefaultChange);
					if (mi.AbsolutePosition.Y > windowCenter.Y) {
						Move(CameraDirection.Down, change);
					}
					if (mi.AbsolutePosition.Y < windowCenter.Y) {
						Move(CameraDirection.Up, change);
					}
				}
				if (Math.Abs(windowCenter.X - mi.AbsolutePosition.X) != change) {
					change = Math.Min(Math.Abs(windowCenter.X - mi.AbsolutePosition.X), DefaultChange);
					if (mi.AbsolutePosition.X > windowCenter.X) {
						Move(CameraDirection.Right, change);
					}
					if (mi.AbsolutePosition.X < windowCenter.X) {
						Move(CameraDirection.Left, change);
					}
				}
				change = DefaultChange;
			}
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize() {
			base.Initialize();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime) {
			if (newCentered) {
				Point windowCenter = WindowCenter;
				Point prevCenter = new Point(-1, -1);
				Point position = centered.GetMonitorInfo().AbsolutePosition;
				while (position != windowCenter && windowCenter != prevCenter) {
					Center();
					prevCenter = windowCenter;
					windowCenter = WindowCenter;
				}
				newCentered = false;
			}
			if (centered != null) {
				Center();
			}
			base.Update(gameTime);
		}
	}
}