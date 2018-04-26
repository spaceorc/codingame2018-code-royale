using System;
using Game.Helpers;

namespace Game
{
	public unsafe struct Minimax
	{
		private readonly int player;
		public byte bestAction;
		public int bestDepth;
		public int bestScore;
		public int evaluations;
		public fixed int prunes[81];

		public Minimax(int player)
			: this()
		{
			this.player = player;
		}

		public int[] Prunes
		{
			get
			{
				var result = new int[bestDepth];
				fixed (int* p = prunes)
				{
					for (var i = 0; i < bestDepth; i++)
						result[i] = p[i];
				}

				return result;
			}
		}

		public void Alphabeta(Field* field, Countdown countdown)
		{
			bestScore = 0;
			bestDepth = 0;
			var actions = stackalloc byte[81];
			for (var i = 0; i < 64; i++)
				actions[i] = 0xFF;
			var depth = 1;
			fixed (int* p = prunes)
			{
				while (!countdown.IsExpired() && depth <= 81)
				{
					for (var i = 0; i < depth; i++)
						p[i] = 0;
					var score = Alphabeta(field, depth, int.MinValue, int.MaxValue, countdown, actions, p);
					if (score == int.MaxValue)
						break;
					bestScore = score;
					bestDepth = depth;
					bestAction = *actions;
					depth++;
				}
			}
		}

		private int Alphabeta(Field* field, int depth, int a, int b, Countdown countdown, byte* actions, int* prune)
		{
			if (countdown.IsExpired())
				return int.MaxValue;

			if (depth == 0 || field->state != Field.GAME_STATE_CONTINUES)
			{
				evaluations++;
				return Evaluation.Evaluate(player, field);
			}

			var maximizingPlayer = field->currentPlayer == player;
			byte excludePos = 0xFF;
			if (*actions != 0xFF)
			{
				field->Apply(*actions, out var previousCurrentField);
				var childScore = Alphabeta(field, depth - 1, a, b, countdown, actions + 1, prune + 1);
				if (childScore == int.MaxValue)
				{
					field->Revert(*actions, previousCurrentField);
					return childScore;
				}
				if (maximizingPlayer)
				{
					if (childScore > a)
						a = childScore;
					else
						throw new InvalidOperationException($"Expected childScore({childScore}) > a({a})");
				}
				else
				{
					if (childScore < b)
						b = childScore;
					else
						throw new InvalidOperationException($"Expected childScore({childScore}) < b({b})");
				}
				field->Revert(*actions, previousCurrentField);
				if (a >= b)
				{
					(*prune)++;
					return maximizingPlayer ? a : b;
				}

				excludePos = *actions;
			}

			field->GetAvailablePositions(out var start, out var end);
			var childActions = stackalloc byte[depth - 1];
			for (var pos = start; pos < end; pos++)
			{
				if (!field->Apply(pos, out var previousCurrentField))
					continue;

				if (pos != excludePos)
				{
					for (var i = 0; i < depth - 1; i++)
						childActions[i] = 0xFF;
					var childScore = Alphabeta(field, depth - 1, a, b, countdown, childActions, prune + 1);
					if (childScore == int.MaxValue)
					{
						field->Revert(pos, previousCurrentField);
						return childScore;
					}

					if (maximizingPlayer)
					{
						if (childScore > a)
						{
							a = childScore;
							*actions = pos;
							for (var i = 1; i < depth; i++)
								actions[i] = childActions[i - 1];
						}
					}
					else
					{
						if (childScore < b)
						{
							b = childScore;
							*actions = pos;
							for (var i = 1; i < depth; i++)
								actions[i] = childActions[i - 1];
						}
					}
				}

				field->Revert(pos, previousCurrentField);
				if (a >= b)
				{
					(*prune)++;
					break;
				}
			}

			return maximizingPlayer ? a : b;
		}
	}
}