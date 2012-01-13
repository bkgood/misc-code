using System;

namespace Pgm04
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args) {
			using (MapGame game = new MapGame()) {
				game.Run();
			}
		}
	}
}

