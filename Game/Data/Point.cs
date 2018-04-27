using System;

namespace Game.Data
{
	public class Point
	{
		public int x;
		public int y;

		public Point(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public int QDistanceTo(Point other) => (x - other.x) * (x - other.x) + (y - other.y) * (y - other.y);
		public double DistanceTo(Point other) => Math.Sqrt(QDistanceTo(other));
	}
}