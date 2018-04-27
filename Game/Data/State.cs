using System;
using System.Collections.Generic;
using Game.Helpers;

namespace Game.Data
{
	public class State
	{
		public Countdown countdown;
		public Random random;
		public int turn;
		public int gold;
		public int touchedSite;
		public readonly Dictionary<int, Structure> structures = new Dictionary<int, Structure>();
		public readonly Queen[] queens = new Queen[2];
		public readonly List<Unit>[] units = { new List<Unit>(), new List<Unit>() };
	}
}