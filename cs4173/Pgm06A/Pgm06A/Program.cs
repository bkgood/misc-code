using System;

namespace Pgm06A
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args) {
			using (Game1 game = new Game1()) {
				game.Run();
			}
		}
	}
}

