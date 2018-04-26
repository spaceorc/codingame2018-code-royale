namespace Game
{
	public unsafe struct Node
	{
		public byte pos;
		public fixed int score[2];
		public int simulations;
		public byte childrenCount;
		public Node* children;
		public Node* parent;

		public override string ToString()
		{
			fixed(int* s = score)
				return $"{nameof(pos)}: {pos}, {nameof(score)}: {s[0]}-{s[1]}, {nameof(simulations)}: {simulations}, {nameof(childrenCount)}: {childrenCount}";
		}
	}
}