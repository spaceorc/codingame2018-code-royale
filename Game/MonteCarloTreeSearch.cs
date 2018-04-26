using System;
using Game.Helpers;

namespace Game
{
	public unsafe struct MonteCarloTreeSearch
	{
		private const double explorationCoeff = 2;
		private readonly int player;
		public byte bestAction;
		public int bestDepth;
		public int simulationsCount;
		public double bestScore;
		public Random random;

		public MonteCarloTreeSearch(int player, Random random)
			: this()
		{
			this.player = player;
			this.random = random;
		}

		public void Run(Field* field, Countdown countdown)
		{
			Memory.Clear();
			var root = Memory.Alloc(1);
			root->pos = 0xFF;
			root->score[0] = 0;
			root->score[1] = 0;
			root->simulations = 0;
			root->childrenCount = 0xFF;
			root->parent = null;
			simulationsCount = 0;
			bestAction = 0xFF;

			while (!countdown.IsExpired())
			{
				var fieldCopy = *field;
				var current = root;
				var depth = 0;
				while (current->childrenCount != 0xFF)
				{
					Node* best = null;
					var bestUcb1 = double.MinValue;
					for (var i = 0; i < current->childrenCount; i++)
					{
						var child = &current->children[i];
						var ucb1 = Ucb1(current, child, fieldCopy.currentPlayer);
						if (ucb1 > bestUcb1)
						{
							best = child;
							bestUcb1 = ucb1;
						}
					}
					current = best;
					depth++;
					fieldCopy.Apply(best->pos);
				}

				if (depth > bestDepth)
					bestDepth = depth;

				if (fieldCopy.state != Field.GAME_STATE_CONTINUES)
				{
					var score1 = 0;
					var score2 = 0;
					if (fieldCopy.state == Field.GAME_STATE_WINNER1)
						score1 = 2;
					else if (fieldCopy.state == Field.GAME_STATE_WINNER2)
						score2 = 2;
					else
					{
						score1 = 1;
						score2 = 1;
					}
					for (var n = current; n != null; n = n->parent)
					{
						n->simulations++;
						n->score[0] += score1;
						n->score[1] += score2;
					}
				}
				else
				{
					fieldCopy.GetAvailablePositions(out var start, out var end);
					current->children = Memory.Alloc(end - start);
					current->childrenCount = 0;
					var fieldBackup = fieldCopy;
					for (var pos = start; pos < end; pos++)
					{
						if (fieldCopy.Apply(pos, out var _))
						{
							var child = &current->children[current->childrenCount];
							child->parent = current;
							child->pos = pos;
							child->childrenCount = 0xFF;
							Simulate(&fieldCopy);
							child->simulations = 1;
							if (fieldCopy.state == Field.GAME_STATE_WINNER1)
							{
								child->score[0] = 2;
								child->score[1] = 0;
							}
							else if (fieldCopy.state == Field.GAME_STATE_WINNER2)
							{
								child->score[0] = 0;
								child->score[1] = 2;
							}
							else
							{
								child->score[0] = 1;
								child->score[1] = 1;
							}
							current->childrenCount++;

							for (var n = current; n != null; n = n->parent)
							{
								n->simulations++;
								n->score[0] += child->score[0];
								n->score[1] += child->score[1];
							}
							fieldCopy = fieldBackup;
						}
					}
					Memory.Release(end - start - current->childrenCount);
				}
			}

			if (root->childrenCount == 0xFF)
				throw new InvalidOperationException("root->childrenCount == 0xFF");

			bestScore = double.MinValue;
			for (var i = 0; i < root->childrenCount; i++)
			{
				var child = &root->children[i];
				var score = Score(child, player);
				if (score > bestScore)
				{
					bestScore = score;
					bestAction = child->pos;
				}
			}
		}

		private void Simulate(Field* field)
		{
			simulationsCount++;
			var state = field->state;
			var positions = stackalloc byte[81];
			while (state == Field.GAME_STATE_CONTINUES)
			{
				field->GetAvailablePositions(out var start, out var end);
				var positionsCount = 0;
				for (var pos = start; pos < end; pos++)
				{
					if (field->CanApply(pos))
						positions[positionsCount++] = pos;
				}

				var position = positions[(byte)random.Next(positionsCount)];
				field->Apply(position);
				state = field->state;
			}
		}

		private static double Ucb1(Node* parentNode, Node* node, int player)
		{
			return (double)node->score[player] / node->simulations + explorationCoeff * Math.Sqrt(Math.Log(parentNode->simulations) / node->simulations);
		}

		private static double Score(Node* node, int player)
		{
			return (double)node->score[player] / node->simulations;
		}
	}
}