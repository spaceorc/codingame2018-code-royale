using System.Linq;

namespace Game.Data
{
	public class TrainAction : IGameAction
	{
		private readonly int[] siteIds;

		public TrainAction(params int[] siteIds)
		{
			this.siteIds = siteIds;
		}

		public override string ToString()
		{
			return "TRAIN" + (siteIds.Any() ? $" {string.Join(" ", siteIds)}" : "");
		}
	}
}