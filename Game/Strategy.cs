using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;

namespace Game
{
	public class Strategy
	{
		private readonly InitData data;
		private Point startPos;
		private int runPoint;
		private List<KeyValuePair<int, Site>> nearestSites;
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
			targetSite = nearestSites.FirstOrDefault(s => !state.structures.ContainsKey(s.Key) || state.structures[s.Key].owner == 1 && state.structures[s.Key].structureType != StructureType.Tower);

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
			if (TryEscape(state, out var gameAction))
				return gameAction;

			if (TryBuildBasic(state, out gameAction))
				return gameAction;

			//if (TryUpgradeMines(state, out gameAction))
			//	return gameAction;

			//if (TryBuildAdvanced(state, out gameAction))
			//	return gameAction;

			return new WaitAction();
		}

		private bool TryBuildBasic(State state, out IGameAction buildAction)
		{
			buildAction = null;
			if (targetSite.Value == null || state.structures.Any(s => s.Value.owner == 1
			                                                          && s.Value.structureType == StructureType.Tower
			                                                          && data.sites[s.Key].DistanceTo(targetSite.Value) <=
			                                                          s.Value.TowerAttackRadius))

				return false;

			if (state.structures.Count(s =>
				    s.Value.owner == 0 && s.Value.structureType == StructureType.Mine) < 3)
			{
				buildAction = new BuildAction(targetSite.Key, StructureType.Mine);
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
				    s.Value.owner == 0 && s.Value.structureType == StructureType.Tower) < 3)
			{
				buildAction = new BuildAction(targetSite.Key, StructureType.Tower);
				return true;
			}

			return false;
		}

		private bool TryEscape(State state, out IGameAction gameAction)
		{
			if (state.units[1].Any(u => u.DistanceTo(state.queens[0]) <= 300))
			{
				if (state.structures.Any(s => s.Value.owner == 0 && s.Value.structureType == StructureType.Tower))
				{
					var runTower = nearestSites.First(x =>
						state.structures.TryGetValue(x.Key, out var s) && s.owner == 0 &&
						s.structureType == StructureType.Tower);
					var runStructure = state.structures[runTower.Key];
					var runPoints = new[]
					{
						new Point(runTower.Value.x - runStructure.TowerAttackRadius * 2 / 3, runTower.Value.y),
						new Point(runTower.Value.x, runTower.Value.y - runStructure.TowerAttackRadius * 2 / 3),
						new Point(runTower.Value.x + runStructure.TowerAttackRadius * 2 / 3, runTower.Value.y),
						new Point(runTower.Value.x, runTower.Value.y + runStructure.TowerAttackRadius * 2 / 3)
					};
					foreach (var point in runPoints)
					{
						point.Limit(Constants.QUEEN_RADIUS);
						while (data.sites.Any(s => s.Key != runTower.Key && s.Value.DistanceTo(point) < s.Value.radius + Constants.QUEEN_RADIUS))
							point.MoveTo(runTower.Value, 10);
					}

					if (state.units[1].Any(u => u.DistanceTo(state.queens[0]) <= 150))
					{
						if (state.queens[0].DistanceTo(runPoints[runPoint]) < Constants.QUEEN_RADIUS)
						{
							runPoint = (runPoint + 1) % runPoints.Length;
							Console.Error.WriteLine($"RunPoint={runPoint}");
						}

						gameAction = new MoveAction(runPoints[runPoint]);
						return true;
					}

					runPoint = Array.IndexOf(runPoints, runPoints.OrderByDescending(p => state.units[1].Max(u => u.DistanceTo(p))).First());
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