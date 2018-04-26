using System;
using Game.Data;
using Game.Helpers;

namespace Game
{
	public unsafe class MinimaxStrategy
	{
		public GameAction Decide(int player, Field* field, Countdown countdown)
		{
			var minimax = new Minimax(player);
			minimax.Alphabeta(field, countdown);
			Console.Error.WriteLine($"Depth: {minimax.bestDepth}; Score: {minimax.bestScore}; Evaluations: {minimax.evaluations}; Prunes: {string.Join(",", minimax.Prunes)}");
			if (minimax.bestDepth == 0)
				throw new InvalidOperationException("minimax.bestDepth == 0!");
			return new GameAction(minimax.bestAction);
		}
	}
}