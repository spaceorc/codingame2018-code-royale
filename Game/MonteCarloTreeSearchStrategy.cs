using System;
using Game.Data;
using Game.Helpers;

namespace Game
{
	public unsafe class MonteCarloTreeSearchStrategy
	{
		public GameAction Decide(int player, Field* field, Countdown countdown, Random random)
		{
			var mcts = new MonteCarloTreeSearch(player, random);
			mcts.Run(field, countdown);
			Console.Error.WriteLine($"Depth: {mcts.bestDepth}; Score: {mcts.bestScore}; Simulations: {mcts.simulationsCount}");
			if (mcts.bestAction == 0xFF)
				throw new InvalidOperationException("mcts.bestAction == 0xFF!");
			return new GameAction(mcts.bestAction);
		}
	}
}