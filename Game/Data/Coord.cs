using System;

namespace Game.Data
{
	public class Coord : IEquatable<Coord>
	{
		public readonly int row;
		public readonly int col;

		public Coord(int row, int col)
		{
			this.row = row;
			this.col = col;
		}

		public override string ToString()
		{
			return $"{row} {col}";
		}

		public bool Equals(Coord other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return row == other.row && col == other.col;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((Coord)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (row*397) ^ col;
			}
		}

		public static bool operator==(Coord left, Coord right)
		{
			return Equals(left, right);
		}

		public static bool operator!=(Coord left, Coord right)
		{
			return !Equals(left, right);
		}
	}
}