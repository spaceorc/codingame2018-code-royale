using System;
using Game.Data;

namespace Game
{
	public unsafe class EntryPoint
	{
		private static void Main(string[] args)
		{
			var reader = new StateReader(Console.ReadLine);
			var strategy = new MonteCarloTreeSearchStrategy();
			//var strategy = new MinimaxStrategy();
			var field = new Field("");
			var player = -1;

			// game loop
			var turn = 0;
			while (true)
			{
				var state = reader.ReadState();
				state.turn = turn;
				Console.Error.WriteLine($"Turn: {turn}");
				if (player == -1)
				{
					player = state.lastOpponentCoord.row >= 0 ? 1 : 0;
					Console.Error.WriteLine($"Player: {player}");
				}
				if (state.lastOpponentCoord.row >= 0)
					field.Apply(new GameAction(state.lastOpponentCoord));

				Console.Error.WriteLine($"field = new Field(\"{field.Serialize()}\")");
				var action = strategy.Decide(player, &field, state.countdown, state.random);
				field.Apply(action);
				Console.Error.WriteLine($"action = {action.field}, {action.pos}");
				Console.Error.WriteLine($"time: {state.countdown.ElapsedMilliseconds}");
				turn++;
				Console.Out.WriteLine(action);
			}
		}
	}
}