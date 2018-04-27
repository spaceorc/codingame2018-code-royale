namespace Game.Data
{
	public class Structure
	{
		public readonly StructureType structureType;
		public readonly int owner;
		public readonly int param1;
		public readonly int param2;

		public Structure(int structureType, int owner, int param1, int param2)
		{
			this.structureType = (StructureType) structureType;
			this.owner = owner;
			this.param1 = param1;
			this.param2 = param2;
		}

		public int MineIncome => param1;
		public int TowerHP => param1;
		public int BarracksTrainTurnsLeft => param1;

		public int TowerAttackRadius => param2;
		public UnitType BarracksCreepType => (UnitType) param2;
	}
}