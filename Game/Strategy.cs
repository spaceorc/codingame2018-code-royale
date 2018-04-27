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
			var sitesCanTrainGiants = data.sites
				.Where(x => state.structures.TryGetValue(x.Key, out var s) && s.owner == 0 && s.structureType == StructureType.Barracks && s.BarracksTrainTurnsLeft == 0 && s.BarracksCreepType == UnitType.Giant)
				.OrderBy(x => x.Value.QDistanceTo(state.queens[1]))
				.ToList();
			var sitesCanTrainArchers = data.sites
				.Where(x => state.structures.TryGetValue(x.Key, out var s) && s.owner == 0 && s.structureType == StructureType.Barracks && s.BarracksTrainTurnsLeft == 0 && s.BarracksCreepType == UnitType.Archer)
				.OrderBy(x => x.Value.QDistanceTo(state.queens[1]))
				.ToList();
			var sitesCanTrainKnights = data.sites
				.Where(x => state.structures.TryGetValue(x.Key, out var s) && s.owner == 0 && s.structureType == StructureType.Barracks && s.BarracksTrainTurnsLeft == 0 && s.BarracksCreepType == UnitType.Knight)
				.OrderBy(x => x.Value.QDistanceTo(state.queens[1]))
				.ToList();

			var gold = state.gold;
			var sitesToTrain = new List<int>();

			if (state.units[0].All(u => u.type != UnitType.Knight) && sitesCanTrainKnights.Any())
			{
				if (gold < 80)
					return new TrainAction(sitesToTrain.ToArray());
				gold -= 80;
				sitesToTrain.Add(sitesCanTrainKnights.First().Key);
				sitesCanTrainKnights.RemoveAt(0);
			}

			if (state.structures.Any(s => s.Value.owner == 1 && s.Value.structureType == StructureType.Tower)
				&& state.units[0].All(u => u.type != UnitType.Giant) && sitesCanTrainGiants.Any())
			{
				if (gold < 140)
					return new TrainAction(sitesToTrain.ToArray());
				gold -= 140;
				sitesToTrain.Add(sitesCanTrainGiants.First().Key);
			}

			if (state.units[0].All(u => u.type != UnitType.Archer) && sitesCanTrainArchers.Any())
			{
				if (gold < 100)
					return new TrainAction(sitesToTrain.ToArray());
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
			var nearestSites = data.sites.OrderBy(x => state.queens[0].QDistanceTo(x.Value)).ToList();
			var targetSite = nearestSites.FirstOrDefault(s => !state.structures.ContainsKey(s.Key) || state.structures[s.Key].owner == 1 && state.structures[s.Key].structureType != StructureType.Tower);

			if (state.units[1].Any(u => u.DistanceTo(state.queens[0]) <= 100))
			{
				if (state.structures.Any(s => s.Value.owner == 0 && s.Value.structureType == StructureType.Tower))
					return new BuildAction(
						state.structures.First(s => s.Value.owner == 0 && s.Value.structureType == StructureType.Tower).Key,
						StructureType.Tower);

				return new BuildAction(targetSite.Key, StructureType.Tower);
			}

			if (targetSite.Value != null)
			{
				if (state.structures.Any(s => s.Value.owner == 1
											&& s.Value.structureType == StructureType.Tower
											&& data.sites[s.Key].DistanceTo(targetSite.Value) <= s.Value.TowerAttackRadius))
				{
					if (state.structures.Any(s => s.Value.owner == 0 && s.Value.structureType == StructureType.Tower))
						return new BuildAction(
							state.structures.First(s => s.Value.owner == 0 && s.Value.structureType == StructureType.Tower).Key,
							StructureType.Tower);

					return new BuildAction(targetSite.Key, StructureType.Tower);
				}

				for (int i = 1; i <= 10; i++)
				{
					if (i <= 5 && state.structures.Count(s =>
						s.Value.owner == 0 && s.Value.structureType == StructureType.Mine) < i)
						return new BuildAction(targetSite.Key, StructureType.Mine);

					if (i <= 3 && state.structures.Count(s =>
						s.Value.owner == 0 && s.Value.structureType == StructureType.Barracks &&
						s.Value.BarracksCreepType == UnitType.Knight) < i)
						return new BuildAction(targetSite.Key, StructureType.Barracks, UnitType.Knight);

					if (state.structures.Count(s =>
						s.Value.owner == 0 && s.Value.structureType == StructureType.Tower) < i)
						return new BuildAction(targetSite.Key, StructureType.Tower);

					if (i <= 1 && state.structures.Count(s =>
						s.Value.owner == 0 && s.Value.structureType == StructureType.Barracks &&
						s.Value.BarracksCreepType == UnitType.Archer) < i)
						return new BuildAction(targetSite.Key, StructureType.Barracks, UnitType.Archer);

					if (i <= 1 && state.structures.Count(s =>
						s.Value.owner == 0 && s.Value.structureType == StructureType.Barracks &&
						s.Value.BarracksCreepType == UnitType.Giant) < i)
						return new BuildAction(targetSite.Key, StructureType.Barracks, UnitType.Giant);
				}
			}

			if (state.structures.Any(s => s.Value.owner == 0 && s.Value.structureType == StructureType.Tower))
				return new BuildAction(
					state.structures.First(s => s.Value.owner == 0 && s.Value.structureType == StructureType.Tower).Key,
					StructureType.Tower);

			return new WaitAction();
		}
	}
}