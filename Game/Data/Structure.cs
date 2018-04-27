namespace Game.Data
{
	public class Structure
	{
		public readonly StructureType structureType;
		public readonly int owner;
		public readonly int trainTurns;
		public readonly UnitType creepType;

		public Structure(int structureType, int owner, int param1, int param2)
		{
			this.structureType = (StructureType) structureType;
			this.owner = owner;
			trainTurns = param1;
			creepType = (UnitType) param2;
		}
	}
}