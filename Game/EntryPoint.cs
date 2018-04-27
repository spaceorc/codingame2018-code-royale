using System;
using Game.Data;

namespace Game
{
	public unsafe class EntryPoint
	{
		private static void Main(string[] args)
		{
			var reader = new StateReader(Console.ReadLine);
			var data = reader.ReadInitData();
			var strategy = new Strategy(data);

			// game loop
			var turn = 0;
			while (true)
			{
				var state = reader.ReadState(data);
				state.turn = turn;
				Console.Error.WriteLine($"Turn: {turn}");
				var action = strategy.Decide(state);
				Console.Error.WriteLine($"time: {state.countdown.ElapsedMilliseconds}");
				turn++;
				action.Write();
			}
		}
	}
}