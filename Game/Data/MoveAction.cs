namespace Game.Data
{
	public class MoveAction : IGameAction
	{
		private readonly int x;
		private readonly int y;

		public MoveAction(Point target)
			: this(target.x, target.y)
		{
		}

		public MoveAction(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public override string ToString()
		{
			return $"MOVE {x} {y}";
		}
	}
}