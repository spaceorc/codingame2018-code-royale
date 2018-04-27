namespace Game.Data
{
	public class Unit : Point
	{
		public readonly UnitType type;
		public int health;

		public Unit(int x, int y, int unitType, int health) : base(x, y)
		{
			type = (UnitType) unitType;
			this.health = health;
		}
	}
}