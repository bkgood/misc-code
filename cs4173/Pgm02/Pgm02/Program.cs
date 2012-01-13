// Bill Good
// Assignment 2

using System;

namespace Pgm02
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args) {
			using (Klondike game = new Klondike()) {
				game.Run();
			}
		}
	}
}

