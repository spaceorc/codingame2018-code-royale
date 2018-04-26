using System;
using Game;
using Game.Helpers;

namespace Experiments
{
	internal unsafe class Program
	{
		public static void Main()
		{
			Console.Out.WriteLine(sizeof(Field));
			var player = 0;
			var strategy = new MonteCarloTreeSearchStrategy();
			var field = new Field("0|8|0|48|192|338|161|48|267|70|400|34|25|73|16|73|34|128|84|130|292|28|1");

			Dump(&field);

			var gameAction = strategy.Decide(player, &field, new Countdown(1000), new Random(324128495));
			Console.Out.WriteLine(gameAction);

			//gameAction = strategy.Decide(player, &field, new Countdown(900), new Random(324128495));
			//Console.Out.WriteLine(gameAction);
		}

		private static void Dump(Field* field)
		{
			Console.Error.WriteLine($"currentPlayer = {field->currentPlayer}, currentField = {field->currentField}, state = {field->state}");

			var pm = field->playerMasks;
			var pfm = field->playerFieldsMasks;
			DumpSimpleMask(pm[0], pm[1]);
			for (var row = 0; row < 3; row++)
			{
				for (var line = 0; line < 3; line++)
				{
					for (var col = 0; col < 3; col++)
					{
						var i = row*3 + col;
						if (Field.IsWinner(pfm[i << 1]))
							DumpMask(0xFFF, 0, line);
						else if (Field.IsWinner(pfm[(i << 1) | 1]))
							DumpMask(0, 0xFFF, line);
						else if ((pfm[i << 1] | pfm[(i << 1) | 1]) == Field.DRAW_MASK)
							DumpMask(0xFFF, 0xFFF, line);
						else
							DumpMask(pfm[i << 1], pfm[(i << 1) | 1], line);
					}

					Console.Error.WriteLine();
				}

				Console.Error.WriteLine();
			}
		}

		private static void DumpSimpleMask(short s1, short s2)
		{
			for (var i = 0; i < 3; i++)
			{
				DumpMask(s1, s2, i);
				Console.Error.WriteLine();
			}
			Console.Error.WriteLine();
		}

		private static void DumpMask(short s1, short s2, int line)
		{
			s1 >>= line * 3;
			s2 >>= line * 3;
			for (var i = line * 3; i < line * 3 + 3; i++)
			{
				Console.Error.Write((s1 & 1) == 0 && (s2 & 1) == 0
					? '.'
					: (s1 & 1) == 0
						? 'o'
						: (s2 & 1) == 0
							? 'x'
							: '?');
				s1 >>= 1;
				s2 >>= 1;
			}
			Console.Error.Write(' ');
		}
	}
}