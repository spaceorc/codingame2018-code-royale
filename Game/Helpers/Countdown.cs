using System.Diagnostics;

namespace Game.Helpers
{
	public class Countdown
	{
		private readonly int milliseconds;
		private readonly Stopwatch stopwatch = Stopwatch.StartNew();

		public Countdown(int milliseconds)
		{
			this.milliseconds = milliseconds;
		}

		public long ElapsedMilliseconds => stopwatch.ElapsedMilliseconds;
		public bool IsExpired() => stopwatch.ElapsedMilliseconds > milliseconds;
	}
}