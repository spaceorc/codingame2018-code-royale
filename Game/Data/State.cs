using System;
using System.Collections.Generic;
using Game.Helpers;

namespace Game.Data
{
	public class State
	{
		public Countdown countdown;
		public Random random;
		public Coord lastOpponentCoord;
		public List<Coord> validCoords = new List<Coord>();
		public int turn;
	}
}