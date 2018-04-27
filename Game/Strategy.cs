using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;

namespace Game
{
	public class Strategy
	{
		private readonly InitData data;
		private List<KeyValuePair<int, Site>> nearestSites;
		private int runPoint;
		private Point startPos;
		private HashSet<int> noGold = new HashSet<int>();
		private KeyValuePair<int, Site> targetSite;

		public Strategy(InitData data)
		{
			this.data = data;
		}

		public Desision Decide(State state)
		{
			if (startPos == null)
			{
				startPos = new Point(state.queens[0]);
				nearestSites = data.sites.OrderBy(x => startPos.QDistanceTo(x.Value)).ToList();
			}

			noGold.UnionWith(state.structures.Where(x => x.Value.gold == 0).Select(x => x.Key));

			targetSite = nearestSites.FirstOrDefault(s =>
				state.structures[s.Key].owner == -1 ||
				state.structures[s.Key].owner == 1 &&
				state.structures[s.Key].structureType != StructureType.Tower);

			return new Desision
			{
				queenAction = ChooseQueenAction(state),
				trainAction = ChooseTrainAction(state)
			};
		}

		private TrainAction ChooseTrainAction(State state)
		{
			var sitesCanTrainGiants = data.sites
				.Where(x => state.structures[x.Key].owner == 0 
				            && state.structures[x.Key].structureType == StructureType.Barracks 
				            && state.structures[x.Key].BarracksTrainTurnsLeft == 0 
				            && state.structures[x.Key].BarracksCreepType == UnitType.Giant)
				.OrderBy(x => x.Value.QDistanceTo(state.queens[1]))
				.ToList();
			var sitesCanTrainArchers = data.sites
				.Where(x => state.structures[x.Key].owner == 0
				            && state.structures[x.Key].structureType == StructureType.Barracks
				            && state.structures[x.Key].BarracksTrainTurnsLeft == 0
				            && state.structures[x.Key].BarracksCreepType == UnitType.Archer)
				.OrderBy(x => x.Value.QDistanceTo(state.queens[1]))
				.ToList();
			var sitesCanTrainKnights = data.sites
				.Where(x => state.structures[x.Key].owner == 0
				            && state.structures[x.Key].structureType == StructureType.Barracks
				            && state.structures[x.Key].BarracksTrainTurnsLeft == 0
				            && state.structures[x.Key].BarracksCreepType == UnitType.Knight)
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

			//if (state.structures.Any(s => s.Value.owner == 1 && s.Value.structureType == StructureType.Tower)
			//    && state.units[0].All(u => u.type != UnitType.Giant) && sitesCanTrainGiants.Any())
			//{
			//	if (gold < 140)
			//		return new TrainAction(sitesToTrain.ToArray());
			//	gold -= 140;
			//	sitesToTrain.Add(sitesCanTrainGiants.First().Key);
			//}

			if (state.units[0].All(u => u.type != UnitType.Archer) && sitesCanTrainArchers.Any())
			{
				if (gold < 100)
					return new TrainAction(sitesToTrain.ToArray());
				gold -= 100;
				sitesToTrain.Add(sitesCanTrainArchers.First().Key);
			}

			foreach (var s in sitesCanTrainKnights)
				if (gold >= 80)
				{
					gold -= 80;
					sitesToTrain.Add(s.Key);
				}

			return new TrainAction(sitesToTrain.ToArray());
		}

		private IGameAction ChooseQueenAction(State state)
		{
			if (TryEscape(state, out var gameAction))
				return gameAction;

			if (TryUpgradeMines(state, out gameAction))
				return gameAction;

			if (TryBuildBasic(state, out gameAction))
				return gameAction;
			
			if (TryUpgradeTowers(state, out gameAction))
				return gameAction;

			if (TryBuildAdvanced(state, out gameAction))
				return gameAction;

			if (TryUpgradeTowers2(state, out gameAction))
				return gameAction;

			return new WaitAction();
		}

		private bool TryBuildAdvanced(State state, out IGameAction buildAction)
		{
			buildAction = null;

			var targetSites = nearestSites.Where(s =>
					state.structures[s.Key].owner == -1 ||
					state.structures[s.Key].owner == 1 &&
					state.structures[s.Key].structureType != StructureType.Tower)
				.ToList();

			var freeSites = targetSites
				.TakeWhile(x => !state.structures
					.Any(s => s.Value.owner == 1 
					          && s.Value.structureType == StructureType.Tower 
					          && data.sites[s.Key].DistanceTo(x.Value) <= s.Value.TowerAttackRadius))
				.ToList();

			if (!freeSites.Any())
				return false;

			if (state.structures.Count(s =>
				    s.Value.owner == 0 && s.Value.structureType == StructureType.Barracks &&
				    s.Value.BarracksCreepType == UnitType.Archer) < 1)
			{
				buildAction = new BuildAction(targetSite.Key, StructureType.Barracks, UnitType.Archer);
				return true;
			}

			var freeSitesWithGold = freeSites.Where(x => !noGold.Contains(x.Key) && (state.structures[x.Key].gold == -1 || state.structures[x.Key].gold > 0)).ToList();

			if (state.structures.Count(s =>
				    s.Value.owner == 0 && s.Value.structureType == StructureType.Mine) < 5
			    && freeSitesWithGold.Any())
			{
				buildAction = new BuildAction(freeSitesWithGold.First().Key, StructureType.Mine);
				return true;
			}

			return false;
		}

		private bool TryBuildBasic(State state, out IGameAction buildAction)
		{
			buildAction = null;

			var targetSites = nearestSites.Where(s =>
					state.structures[s.Key].owner == -1 ||
					state.structures[s.Key].owner == 1 &&
					state.structures[s.Key].structureType != StructureType.Tower)
				.ToList();

			var freeSites = targetSites
				.TakeWhile(x => !state.structures
					.Any(s => s.Value.owner == 1 
					          && s.Value.structureType == StructureType.Tower 
					          && data.sites[s.Key].DistanceTo(x.Value) <= s.Value.TowerAttackRadius))
				.ToList();

			if (!freeSites.Any())
				return false;

			var freeSitesWithGold = freeSites.Where(x => !noGold.Contains(x.Key) && (state.structures[x.Key].gold == -1 || state.structures[x.Key].gold > 0)).ToList();
			
			if (state.structures.Count(s =>
				    s.Value.owner == 0 && s.Value.structureType == StructureType.Mine) < 3
			    && freeSitesWithGold.Any())
			{
				buildAction = new BuildAction(freeSitesWithGold.First().Key, StructureType.Mine);
				return true;
			}

			if (state.structures.Count(s =>
				    s.Value.owner == 0 && s.Value.structureType == StructureType.Barracks &&
				    s.Value.BarracksCreepType == UnitType.Knight) < 1)
			{
				buildAction = new BuildAction(targetSite.Key, StructureType.Barracks, UnitType.Knight);
				return true;
			}

			if (state.structures.Count(s =>
				    s.Value.owner == 0 && s.Value.structureType == StructureType.Tower) < 2)
			{
				buildAction = new BuildAction(targetSite.Key, StructureType.Tower);
				return true;
			}

			return false;
		}

		private bool TryUpgradeMines(State state, out IGameAction buildAction)
		{
			var candidates = new HashSet<int>(state.structures
				.Where(s => s.Value.owner == 0 && s.Value.structureType == StructureType.Mine &&
				            s.Value.MineIncome < s.Value.maxMineSize)
				.Select(s => s.Key));
			if (!candidates.Any())
			{
				buildAction = null;
				return false;
			}

			buildAction = new BuildAction(nearestSites.First(s => candidates.Contains(s.Key)).Key, StructureType.Mine);
			return true;
		}

		private bool TryUpgradeTowers(State state, out IGameAction buildAction)
		{
			var candidates = new HashSet<int>(state.structures
				.Where(s => s.Value.owner == 0 && s.Value.structureType == StructureType.Tower &&
				            s.Value.TowerAttackRadius <= 150)
				.Select(s => s.Key));
			if (!candidates.Any())
			{
				buildAction = null;
				return false;
			}

			buildAction = new BuildAction(nearestSites.First(s => candidates.Contains(s.Key)).Key, StructureType.Tower);
			return true;
		}

		private bool TryUpgradeTowers2(State state, out IGameAction buildAction)
		{
			var candidates = new HashSet<int>(state.structures
				.Where(s => s.Value.owner == 0 && s.Value.structureType == StructureType.Tower)
				.OrderBy(s => s.Value.TowerAttackRadius)
				.Select(s => s.Key));
			if (!candidates.Any())
			{
				buildAction = null;
				return false;
			}

			buildAction = new BuildAction(candidates.First(), StructureType.Tower);
			return true;
		}

		private bool TryEscape(State state, out IGameAction gameAction)
		{
			if (state.units[1].Any(u => u.DistanceTo(state.queens[0]) <= 350))
			{
				var towerCandidate = data.sites
					.OrderBy(s => s.Value.DistanceTo(state.queens[0]))
					.FirstOrDefault(s => state.structures[s.Key].owner == -1 && s.Value.DistanceTo(state.queens[0]) <= 200);
				if (towerCandidate.Value != null)
				{
					gameAction = new BuildAction(
						towerCandidate.Key,
						StructureType.Tower);
					return true;
				}

				if (state.structures.Any(s => s.Value.owner == 0 && s.Value.structureType == StructureType.Tower))
				{
					var runTower = nearestSites.First(x =>
						state.structures[x.Key].owner == 0 &&
						state.structures[x.Key].structureType == StructureType.Tower);
					var runStructure = state.structures[runTower.Key];
					var runPoints = new[]
					{
						new Point(runTower.Value.x - runStructure.TowerAttackRadius * 2.5 / 3, runTower.Value.y),
						new Point(runTower.Value.x, runTower.Value.y - runStructure.TowerAttackRadius * 2.5 / 3),
						new Point(runTower.Value.x + runStructure.TowerAttackRadius * 2.5 / 3, runTower.Value.y),
						new Point(runTower.Value.x, runTower.Value.y + runStructure.TowerAttackRadius * 2.5 / 3)
					};
					foreach (var point in runPoints)
					{
						point.Limit(Constants.QUEEN_RADIUS);
						while (data.sites.Any(s =>
							s.Key != runTower.Key && s.Value.DistanceTo(point) < s.Value.radius + Constants.QUEEN_RADIUS))
							point.MoveTo(runTower.Value, 10);
					}

					if (state.units[1].Any(u => u.DistanceTo(state.queens[0]) <= 200))
					{
						if (state.queens[0].DistanceTo(runPoints[runPoint]) < Constants.QUEEN_RADIUS)
						{
							runPoint = (runPoint + 1) % runPoints.Length;
							Console.Error.WriteLine($"RunPoint={runPoint}");
						}

						gameAction = new MoveAction(runPoints[runPoint]);
						return true;
					}

					runPoint = Array.IndexOf(runPoints,
						runPoints.OrderByDescending(p => state.units[1].Max(u => u.DistanceTo(p))).First());
					Console.Error.WriteLine($"RunPoint={runPoint}");
					gameAction = new BuildAction(
						runTower.Key,
						StructureType.Tower);
					return true;
				}

				{
					gameAction = new BuildAction(targetSite.Key, StructureType.Tower);
					return true;
				}
			}

			gameAction = null;
			return false;
		}
	}
}