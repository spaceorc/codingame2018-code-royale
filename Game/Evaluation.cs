using System.Runtime.CompilerServices;

namespace Game
{
	public static unsafe class Evaluation
	{
		public static int Evaluate(int player, Field* field)
		{
			// Term states
			switch (field->state)
			{
				case Field.GAME_STATE_WINNER1: // 1st is winner - best
					return player == 0 ? 1000000 : -1000000;
				case Field.GAME_STATE_WINNER2: // 2st is winner - worst
					return player == 0 ? -1000000 : 1000000;
				case Field.GAME_STATE_DRAW: // draw - neutral
					return 0;
			}

			// Game continues
			var fp1 = field->playerMasks[0];
			var fp2 = field->playerMasks[1];
			var sequenceDelta = GetSequenceDelta(fp1, fp2);
			var subfieldsWinsDelta = 0;
			var centerWinDelta = 0;
			var subfieldsTotalSequenceDelta = 0;
			var subfieldsTotalNormalizedSequenceDelta = 0;
			for (var i = 0; i < 9; i++)
			{
				var p1 = field->playerFieldsMasks[i << 1];
				var p2 = field->playerFieldsMasks[(i << 1) | 1];
				if (Field.IsWinner(p1))
				{
					subfieldsWinsDelta++;
					if (i == 4)
						centerWinDelta++;
				}
				else if (Field.IsWinner(p2))
				{
					subfieldsWinsDelta--;
					if (i == 4)
						centerWinDelta--;
				}
				else if ((p1 | p2) != Field.DRAW_MASK)
				{
					var subfieldSequenceDelta = GetSequenceDelta(p1, p2);
					subfieldsTotalSequenceDelta += subfieldSequenceDelta;
					if (subfieldSequenceDelta > 0)
						subfieldsTotalNormalizedSequenceDelta++;
					else if (subfieldSequenceDelta < 0)
						subfieldsTotalNormalizedSequenceDelta--;
					break;
				}
			}

			var score = subfieldsWinsDelta * 10000 + centerWinDelta * 1000 + sequenceDelta * 100 + subfieldsTotalNormalizedSequenceDelta * 10 + subfieldsTotalSequenceDelta;
			return player == 0 ? score : -score;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetSequenceDelta(short p1, short p2)
		{
			return GetSequenceDelta(p1, p2, Field.WINNER_ROW_MASK, Field.WINNER_ROW_MASK_CM1, Field.WINNER_ROW_MASK_CM2, Field.WINNER_ROW_MASK_CM3)
					+ GetSequenceDelta(p1, p2, Field.WINNER_ROW_MASK << 3, Field.WINNER_ROW_MASK_CM1 << 3, Field.WINNER_ROW_MASK_CM2 << 3, Field.WINNER_ROW_MASK_CM3 << 3)
					+ GetSequenceDelta(p1, p2, Field.WINNER_ROW_MASK << 6, Field.WINNER_ROW_MASK_CM1 << 6, Field.WINNER_ROW_MASK_CM2 << 6, Field.WINNER_ROW_MASK_CM3 << 6)

					+ GetSequenceDelta(p1, p2, Field.WINNER_COL_MASK, Field.WINNER_COL_MASK_CM1, Field.WINNER_COL_MASK_CM2, Field.WINNER_COL_MASK_CM3)
					+ GetSequenceDelta(p1, p2, Field.WINNER_COL_MASK << 1, Field.WINNER_COL_MASK_CM1 << 1, Field.WINNER_COL_MASK_CM2 << 1, Field.WINNER_COL_MASK_CM3 << 1)
					+ GetSequenceDelta(p1, p2, Field.WINNER_COL_MASK << 2, Field.WINNER_COL_MASK_CM1 << 2, Field.WINNER_COL_MASK_CM2 << 2, Field.WINNER_COL_MASK_CM3 << 2)

					+ GetSequenceDelta(p1, p2, Field.WINNER_DIAG_MASK)
					+ GetSequenceDelta(p1, p2, Field.WINNER_DIAG2_MASK);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetSequenceDelta(short p1, short p2, short winnerMask)
		{
			if ((p1 & winnerMask) != 0 && (p2 & winnerMask) != 0)
				return 0;
			if ((p1 & winnerMask) == 0 && (p2 & winnerMask) == 0)
				return 0;
			return (p1 & winnerMask) != 0 ? 1 : -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetSequenceDelta(short p1, short p2, short winnerMask, short checkMask1, short checkMask2, short checkMask3)
		{
			if ((p1 & winnerMask) != 0 && (p2 & winnerMask) != 0)
				return 0;

			if ((p1 & winnerMask) == 0 && (p2 & winnerMask) == 0)
				return 0;

			if ((p1 & winnerMask) != 0)
			{
				if ((p1 & checkMask1) != 0 || (p1 & checkMask2) != 0 || (p1 & checkMask3) != 0)
					return 1;
			}
			else
			{
				if ((p2 & checkMask1) != 0 || (p2 & checkMask2) != 0 || (p2 & checkMask3) != 0)
					return -1;
			}

			return 0;
		}
	}
}