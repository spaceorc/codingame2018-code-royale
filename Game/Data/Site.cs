namespace Game.Data
{
	public class Site : Point
	{
		public readonly int radius;

		public Site(int x, int y, int radius) : base(x, y)
		{
			this.radius = radius;
		}
	}
}