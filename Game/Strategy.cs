using System.Collections.Generic;
using System.Linq;
using Game.Data;

namespace Game
{
	public class Strategy
	{
		private readonly InitData data;

		public Strategy(InitData data)
		{
			this.data = data;
		}

		public Desision Decide(State state)
		{
			return new Desision
			{
				queenAction = ChooseQueenAction(state),
				trainAction = ChooseTrainAction(state)
			};
		}

		private TrainAction ChooseTrainAction(State state)
		{
			var sitesCanTrainArchers = data.sites
				.Where(x => state.structures.TryGetValue(x.Key, out var structure) && structure.owner == 0 && structure.trainTurns == 0 && structure.creepType == UnitType.Archer)
				.OrderBy(x => x.Value.QDistanceTo(state.queens[1]))
				.ToList();
			var sitesCanTrainKnights = data.sites
				.Where(x => state.structures.TryGetValue(x.Key, out var structure) && structure.owner == 0 && structure.trainTurns == 0 && structure.creepType == UnitType.Knight)
				.OrderBy(x => x.Value.QDistanceTo(state.queens[1]))
				.ToList();
			var gold = state.gold;
			var sitesToTrain = new List<int>();
			if (state.units[0].All(u => u.type != UnitType.Archer) && sitesCanTrainArchers.Any())
			{
				if (gold < 100)
					return new TrainAction();
				gold -= 100;
				sitesToTrain.Add(sitesCanTrainArchers.First().Key);
			}
			foreach (var s in sitesCanTrainKnights)
			{
				if (gold >= 80)
				{
					gold -= 80;
					sitesToTrain.Add(s.Key);
				}
			}
			return new TrainAction(sitesToTrain.ToArray());
		}

		private IGameAction ChooseQueenAction(State state)
		{
			if (state.structures.Any(s => s.Value.owner == 0 && s.Value.creepType == UnitType.Archer)
			    && state.structures.Count(s => s.Value.owner == 0 && s.Value.creepType == UnitType.Knight) >= 2)
			{
				var points = new[] { new Point(0, 0), new Point(1920, 1000), new Point(0, 1000), new Point(1920, 0) };
				return new MoveAction(points
					.OrderByDescending(p => p.QDistanceTo(state.queens[1]))
					.First());
			}

			if (state.touchedSite != -1 && !state.structures.ContainsKey(state.touchedSite))
			{
				if (!state.structures.Any(s => s.Value.owner == 0 && s.Value.creepType == UnitType.Archer))
					return new BuildAction(state.touchedSite, StructureType.Barracks, UnitType.Archer);
				return new BuildAction(state.touchedSite, StructureType.Barracks, UnitType.Knight);
			}

			var nearestSites = data.sites.OrderBy(x => state.queens[0].QDistanceTo(x.Value)).ToList();
			var targetSite = nearestSites.FirstOrDefault(s => s.Key != state.touchedSite && !state.structures.ContainsKey(s.Key) || state.structures[s.Key].owner != 0);
			if (targetSite.Value == null)
				return new WaitAction();
			return new MoveAction(targetSite.Value);
		}
	}
}