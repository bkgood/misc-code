using System;

namespace Pgm05
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args) {
			using (BrickInvaders game = new BrickInvaders()) {
				game.Run();
			}
		}
	}
}

