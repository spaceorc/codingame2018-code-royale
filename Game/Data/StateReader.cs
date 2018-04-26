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

		public State ReadState()
		{
			try
			{
				var state = new State();
				var inputs = readLine().Split(' ');
				var opponentRow = int.Parse(inputs[0]);
				var opponentCol = int.Parse(inputs[1]);
				state.lastOpponentCoord = new Coord(opponentRow, opponentCol);
				var validActionCount = int.Parse(readLine());
				
				for (var i = 0; i < validActionCount; i++)
				{
					inputs = readLine().Split(' ');
					var row = int.Parse(inputs[0]);
					var col = int.Parse(inputs[1]);
					state.validCoords.Add(new Coord(row, col));
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