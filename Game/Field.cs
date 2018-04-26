using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Data;

namespace Game
{
	public unsafe struct Field
	{
		public const short WINNER_ROW_MASK = (1 << 0) | (1 << 1) | (1 << 2);
		public const short WINNER_ROW_MASK_CM1 = (1 << 1) | (1 << 2);
		public const short WINNER_ROW_MASK_CM2 = (1 << 0) | (1 << 2);
		public const short WINNER_ROW_MASK_CM3 = (1 << 0) | (1 << 1);

		public const short WINNER_COL_MASK = (1 << 0) | (1 << 3) | (1 << 6);
		public const short WINNER_COL_MASK_CM1 = (1 << 3) | (1 << 6);
		public const short WINNER_COL_MASK_CM2 = (1 << 0) | (1 << 6);
		public const short WINNER_COL_MASK_CM3 = (1 << 0) | (1 << 3);

		public const short WINNER_DIAG_MASK = (1 << 0) | (1 << 4) | (1 << 8);
		public const short WINNER_DIAG2_MASK = (1 << 2) | (1 << 4) | (1 << 6);

		public const short DRAW_MASK = (1 << 9) - 1;

		public const byte GAME_STATE_CONTINUES = 0;
		public const byte GAME_STATE_WINNER1 = 1;
		public const byte GAME_STATE_WINNER2 = 2;
		public const byte GAME_STATE_DRAW = 3;

		public byte currentPlayer;
		public byte currentField; // 0xFF for any
		public byte state;
		public fixed short playerMasks[2]; // masks: 9bit per player
		public fixed short playerFieldsMasks[2 * 9]; // masks: 9bit per player per field

		public Field(string source)
			: this()
		{
			currentField = 0xFF;
			if (!string.IsNullOrEmpty(source))
			{
				var values = source.Split('|');
				currentPlayer = byte.Parse(values[0]);
				currentField = byte.Parse(values[1]);
				state = byte.Parse(values[2]);
				fixed (short* pm = playerMasks)
				fixed (short* pfm = playerFieldsMasks)
				{
					for (var i = 0; i < 2; i++)
						pm[i] = short.Parse(values[3 + i]);
					for (var i = 0; i < 9 * 2; i++)
						pfm[i] = short.Parse(values[5 + i]);
				}
			}
		}

		public string Serialize()
		{
			var values = new List<object>();
			values.Add(currentPlayer);
			values.Add(currentField);
			values.Add(state);
			fixed (short* pm = playerMasks)
			fixed (short* pfm = playerFieldsMasks)
			{
				for (var i = 0; i < 2; i++)
					values.Add(pm[i]);
				for (var i = 0; i < 9 * 2; i++)
					values.Add(pfm[i]);
			}
			return string.Join("|", values);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsWinner(short mask)
		{
			return (mask & WINNER_ROW_MASK) == WINNER_ROW_MASK
					|| (mask & (WINNER_ROW_MASK << 3)) == WINNER_ROW_MASK << 3
					|| (mask & (WINNER_ROW_MASK << 6)) == WINNER_ROW_MASK << 6

					|| (mask & WINNER_COL_MASK) == WINNER_COL_MASK
					|| (mask & (WINNER_COL_MASK << 1)) == WINNER_COL_MASK << 1
					|| (mask & (WINNER_COL_MASK << 2)) == WINNER_COL_MASK << 2

					|| (mask & WINNER_DIAG_MASK) == WINNER_DIAG_MASK
					|| (mask & WINNER_DIAG2_MASK) == WINNER_DIAG2_MASK;
		}

		public void Apply(GameAction gameAction)
		{
			Apply((byte)((gameAction.field.row * 3 + gameAction.field.col) * 9 + gameAction.pos.row * 3 + gameAction.pos.col), out var _);
		}

		public bool CanApply(byte pos)
		{
			fixed (short* pm = playerMasks)
			fixed (short* pfm = playerFieldsMasks)
			{
				// Shift to local field pos
				var field = pos / 9;
				var fieldMask = (short)(1 << field);
				pos %= 9;

				// Couldn't use pos if field is done
				if (((pm[0] | pm[1]) & fieldMask) != 0)
					return false;

				// Couldn't use pos if it's already occupied
				var p1 = pfm[field << 1];
				var p2 = pfm[(field << 1) | 1];
				var posMask = (short)(1 << pos);
				if (((p1 | p2) & posMask) != 0)
					return false;

				return true;
			}
		}

		public void Apply(byte pos)
		{
			fixed (short* pm = playerMasks)
			fixed (short* pfm = playerFieldsMasks)
			{
				// Shift to local field pos
				var field = pos / 9;
				var fieldMask = (short)(1 << field);
				pos %= 9;
				var posMask = (short)(1 << pos);
				
				// Occupy pos
				var newMask = (short)(pfm[(field << 1) | currentPlayer] | posMask);
				var otherMask = pfm[(field << 1) | (currentPlayer ^ 1)];
				pfm[(field << 1) | currentPlayer] = newMask;

				// If we have a local winner - apply it to global masks
				if (IsWinner(newMask))
				{
					pm[currentPlayer] = (short)(pm[currentPlayer] | fieldMask);
					if (IsWinner((short)(pm[currentPlayer] & ~pm[currentPlayer ^ 1])))
						state = currentPlayer == 0 ? GAME_STATE_WINNER1 : GAME_STATE_WINNER2;
					else if ((pm[0] | pm[1]) == DRAW_MASK)
					{
						var m1 = pm[0] & ~pm[1];
						var m2 = pm[0] & ~pm[1];
						var c1 = 0;
						var c2 = 0;
						for (var i = 0; i < 9; i++, m1 >>= 1, m2 >>= 1)
						{
							if ((m1 & 1) != 0)
								c1++;
							if ((m2 & 1) != 0)
								c2++;
						}
						state = c1 > c2 ? GAME_STATE_WINNER1 : c1 < c2 ? GAME_STATE_WINNER2 : GAME_STATE_DRAW;
					}
				}
				else if ((newMask | otherMask) == DRAW_MASK)
				{
					pm[0] = (short)(pm[0] | fieldMask);
					pm[1] = (short)(pm[1] | fieldMask);
					if ((pm[0] | pm[1]) == DRAW_MASK)
					{
						var m1 = pm[0] & ~pm[1];
						var m2 = pm[1] & ~pm[0];
						var c1 = 0;
						var c2 = 0;
						for (var i = 0; i < 9; i++, m1 >>= 1, m2 >>= 1)
						{
							if ((m1 & 3) != 0)
								c1++;
							if ((m2 & 3) != 0)
								c2++;
						}
						state = c1 > c2 ? GAME_STATE_WINNER1 : c1 < c2 ? GAME_STATE_WINNER2 : GAME_STATE_DRAW;
					}
				}

				// Change current field
				if (((pm[0] | pm[1]) & posMask) == 0)
					currentField = pos;
				else
					currentField = 0xFF;

				// Switch player
				currentPlayer ^= 1;
			}
		}

		public bool Apply(byte pos, out byte previousCurrentField)
		{
			previousCurrentField = 0;
			fixed (short* pm = playerMasks)
			fixed (short* pfm = playerFieldsMasks)
			{
				// Shift to local field pos
				var field = pos / 9;
				var fieldMask = (short)(1 << field);
				pos %= 9;

				// Couldn't use pos if field is done
				if (((pm[0] | pm[1]) & fieldMask) != 0)
					return false;

				// Couldn't use pos if it's already occupied
				var p1 = pfm[field << 1];
				var p2 = pfm[(field << 1) | 1];
				var posMask = (short)(1 << pos);
				if (((p1 | p2) & posMask) != 0)
					return false;

				// Occupy pos
				var newMask = (short)(pfm[(field << 1) | currentPlayer] | posMask);
				var otherMask = pfm[(field << 1) | (currentPlayer ^ 1)];
				pfm[(field << 1) | currentPlayer] = newMask;

				// If we have a local winner - apply it to global masks
				if (IsWinner(newMask))
				{
					pm[currentPlayer] = (short)(pm[currentPlayer] | fieldMask);
					if (IsWinner((short)(pm[currentPlayer] & ~pm[currentPlayer ^ 1])))
						state = currentPlayer == 0 ? GAME_STATE_WINNER1 : GAME_STATE_WINNER2;
					else if ((pm[0] | pm[1]) == DRAW_MASK)
					{
						var m1 = pm[0] & ~pm[1];
						var m2 = pm[0] & ~pm[1];
						var c1 = 0;
						var c2 = 0;
						for (var i = 0; i < 9; i++, m1 >>= 1, m2 >>= 1)
						{
							if ((m1 & 1) != 0)
								c1++;
							if ((m2 & 1) != 0)
								c2++;
						}
						state = c1 > c2 ? GAME_STATE_WINNER1 : c1 < c2 ? GAME_STATE_WINNER2 : GAME_STATE_DRAW;
					}
				}
				else if ((newMask | otherMask) == DRAW_MASK)
				{
					pm[0] = (short)(pm[0] | fieldMask);
					pm[1] = (short)(pm[1] | fieldMask);
					if ((pm[0] | pm[1]) == DRAW_MASK)
					{
						var m1 = pm[0] & ~pm[1];
						var m2 = pm[1] & ~pm[0];
						var c1 = 0;
						var c2 = 0;
						for (var i = 0; i < 9; i++, m1 >>= 1, m2 >>= 1)
						{
							if ((m1 & 1) != 0)
								c1++;
							if ((m2 & 1) != 0)
								c2++;
						}
						state = c1 > c2 ? GAME_STATE_WINNER1 : c1 < c2 ? GAME_STATE_WINNER2 : GAME_STATE_DRAW;
					}
				}

				// Change current field
				previousCurrentField = currentField;
				if (((pm[0] | pm[1]) & posMask) == 0)
					currentField = pos;
				else
					currentField = 0xFF;

				// Switch player
				currentPlayer ^= 1;

				return true;
			}
		}

		public void Revert(byte pos, byte previousCurrentField)
		{
			fixed (short* pm = playerMasks)
			fixed (short* pfm = playerFieldsMasks)
			{
				// Switch player back
				currentPlayer ^= 1;

				// Change current field back
				currentField = previousCurrentField;

				// Switch state back to GAME_STATE_CONTINUES
				state = GAME_STATE_CONTINUES;

				// Shift to local field pos
				var field = pos / 9;
				var fieldMask = (short)(1 << field);
				pos %= 9;

				// Undo occupy pos
				var posMask = (short)(1 << pos);
				pfm[field << 1] = (short)(pfm[field << 1] & ~posMask);
				pfm[(field << 1) | 1] = (short)(pfm[(field << 1) | 1] & ~posMask);

				// Undo global masks
				pm[0] = (short)(pm[0] & ~fieldMask);
				pm[1] = (short)(pm[1] & ~fieldMask);

			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void GetAvailablePositions(out byte start, out byte end)
		{
			if (currentField == 0xFF)
			{
				start = 0;
				end = 9 * 9;
			}
			else
			{
				start = (byte)(currentField * 9);
				end = (byte)(start + 9);
			}
		}
	}
}