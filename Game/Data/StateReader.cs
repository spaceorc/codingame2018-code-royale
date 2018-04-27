using System;
using System.Linq;
using Game.Helpers;

namespace Game.Data
{
	public class StateReader
	{
		private string lastLine;
		private readonly bool logToError = true;
		private readonly Func<string> readLine;
		private readonly Func<int> getSeed;

		public StateReader(string input)
		{
			var lines = input.Split('|');
			var seedLine = lines.SingleOrDefault(l => l.StartsWith("seed:"));
			if (seedLine != null)
			{
				var seed = int.Parse(seedLine.Substring("seed:".Length));
				getSeed = () => seed;
				lines = lines.Where(l => !l.StartsWith("seed:")).ToArray();
			}
			
			var index = 0;
			logToError = false;
			readLine = () => index < lines.Length ? lines[index++] : null;
		}

		public StateReader(Func<string> consoleReadLine)
		{
			readLine = () =>
			{
				lastLine = consoleReadLine();
				if (logToError)
					Console.Error.Write(lastLine + "|");
				return lastLine;
			};
			getSeed = () =>
			{
				var seed = Guid.NewGuid().GetHashCode();
				if (logToError)
					Console.Error.Write($"seed:{seed}|");
				return seed;
			};
		}
		
		public InitData ReadInitData()
		{
			var result = new InitData();
			string[] inputs;
			int numSites = int.Parse(readLine());
			for (int i = 0; i < numSites; i++)
			{
				inputs = readLine().Split(' ');
				int siteId = int.Parse(inputs[0]);
				int x = int.Parse(inputs[1]);
				int y = int.Parse(inputs[2]);
				int radius = int.Parse(inputs[3]);
				result.sites.Add(siteId, new Site(x, y, radius));
			}
			if (logToError)
				Console.Error.WriteLine();
			return result;
		}

		public State ReadState(InitData data)
		{
			try
			{
				var state = new State();
				var inputs = readLine().Split(' ');
				state.gold = int.Parse(inputs[0]);
				state.touchedSite = int.Parse(inputs[1]); // -1 if none
				for (int i = 0; i < data.sites.Count; i++)
				{
					inputs = readLine().Split(' ');
					int siteId = int.Parse(inputs[0]);
					int mineGold = int.Parse(inputs[1]); // used in future leagues
					int maxMineSize = int.Parse(inputs[2]); // used in future leagues
					int structureType = int.Parse(inputs[3]); // -1 = No structure, 2 = Barracks
					int owner = int.Parse(inputs[4]); // -1 = No structure, 0 = Friendly, 1 = Enemy
					int param1 = int.Parse(inputs[5]);
					int param2 = int.Parse(inputs[6]);
					state.structures.Add(siteId, new Structure(structureType, owner, param1, param2, mineGold, maxMineSize));
				}
				int numUnits = int.Parse(readLine());
				for (int i = 0; i < numUnits; i++)
				{
					inputs = readLine().Split(' ');
					int x = int.Parse(inputs[0]);
					int y = int.Parse(inputs[1]);
					int owner = int.Parse(inputs[2]);
					int unitType = int.Parse(inputs[3]); // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
					int health = int.Parse(inputs[4]);
					if (unitType == -1)
						state.queens[owner] = new Queen(x, y, health);
					else
						state.units[owner].Add(new Unit(x, y, unitType, health));
				}

				state.random = new Random(getSeed());
				state.countdown = new Countdown(90);
				if (logToError)
					Console.Error.WriteLine();
				return state;
			}
			catch (Exception e)
			{
				throw new FormatException($"Line [{lastLine}]", e);
			}
		}
	}
}