using System;

namespace Game.Data
{
	public class Point : IEquatable<Point>
	{
		public double x;
		public double y;

		public Point(Point other)
			: this(other.x, other.y)
		{
		}

		public Point(double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		public override string ToString()
		{
			return $"{x},{y}";
		}

		public double QDistanceTo(Point other) => (x - other.x) * (x - other.x) + (y - other.y) * (y - other.y);
		public double DistanceTo(Point other) => Math.Sqrt(QDistanceTo(other));

		public void Limit(double radius)
		{
			if (x < radius) x = radius;
			if (x > Constants.WORLD_WIDTH - radius) x = Constants.WORLD_WIDTH - radius;
			if (y < radius) y = radius;
			if (y > 1000) y = 1000;
		}

		public void MoveTo(Point other, double dist)
		{
			var d = DistanceTo(other);

			if (d < Constants.EPSILON)
				return;

			var dx = other.x - x;
			var dy = other.y - y;
			var coef = dist / d;

			Move((int) (x + dx * coef), (int) (y + dy * coef));
		}

		// Move the point to x and y
		public void Move(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public bool Equals(Point other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return x.Equals(other.x) && y.Equals(other.y);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Point) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (x.GetHashCode() * 397) ^ y.GetHashCode();
			}
		}

		public static bool operator ==(Point left, Point right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Point left, Point right)
		{
			return !Equals(left, right);
		}
	}
}