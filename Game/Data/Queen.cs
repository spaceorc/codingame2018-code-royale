namespace Game.Data
{
	public class Queen : Point
	{
		public int health;

		public Queen(int x, int y, int health) : base(x, y)
		{
			this.health = health;
		}
	}
}