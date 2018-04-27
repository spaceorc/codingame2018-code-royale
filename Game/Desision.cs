using System;
using Game.Data;

namespace Game
{
	public class Desision
	{
		public IGameAction queenAction = new WaitAction();
		public TrainAction trainAction = new TrainAction();

		public void Write()
		{
			Console.Out.WriteLine(queenAction);
			Console.Out.WriteLine(trainAction);
		}
	}
}