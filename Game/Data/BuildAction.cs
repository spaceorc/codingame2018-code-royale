namespace Game.Data
{
	public class BuildAction : IGameAction
	{
		private readonly int siteId;
		private readonly StructureType structureType;
		private readonly UnitType unitType;

		public BuildAction(int siteId, StructureType structureType, UnitType unitType = UnitType.None)
		{
			this.siteId = siteId;
			this.structureType = structureType;
			this.unitType = unitType;
		}

		public override string ToString()
		{
			if (structureType == StructureType.Barracks)
				return $"BUILD {siteId} {structureType.ToString().ToUpper()}-{unitType.ToString().ToUpper()}";
			return $"BUILD {siteId} {structureType.ToString().ToUpper()}";
		}
	}
}