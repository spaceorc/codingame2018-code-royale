using System;

namespace Game.Data
{
	public class GameAction : IEquatable<GameAction>
	{
		public readonly Coord field;
		public readonly Coord pos;

		public GameAction(Coord coord)
			: this(new Coord(coord.row / 3, coord.col / 3), new Coord(coord.row % 3, coord.col % 3))
		{
		}

		public GameAction(byte pos)
			: this(pos / 9, pos % 9)
		{
		}

		private GameAction(int fieldNo, int pos)
			: this(new Coord(fieldNo / 3, fieldNo % 3), new Coord(pos / 3, pos % 3))
		{
		}

		public GameAction(Coord field, Coord pos)
		{
			this.field = field;
			this.pos = pos;
		}

		public Coord ToCoord() => new Coord(field.row*3 + pos.row, field.col*3 + pos.col);

		public override string ToString()
		{
			return ToCoord().ToString();
		}

		public bool Equals(GameAction other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(field, other.field) && Equals(pos, other.pos);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((GameAction)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((field != null ? field.GetHashCode() : 0)*397) ^ (pos != null ? pos.GetHashCode() : 0);
			}
		}

		public static bool operator==(GameAction left, GameAction right)
		{
			return Equals(left, right);
		}

		public static bool operator!=(GameAction left, GameAction right)
		{
			return !Equals(left, right);
		}
	}
}