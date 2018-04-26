using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Game
{
	public static unsafe class Memory
	{
		private const int batchSize = 10*1024;

		private static readonly List<IntPtr> storage = new List<IntPtr>();
		public static int capacity;
		public static int currentCapacity;
		public static int count;
		public static Node* current;

		public static Node* Alloc(int count)
		{
			if (count > batchSize)
				throw new InvalidOperationException($"Invalid alloc: count is too big: {count}");
			if (Memory.count + count > currentCapacity)
			{
				Memory.count = currentCapacity;
				currentCapacity += batchSize;
				if (currentCapacity > capacity)
				{
					var ptr = Marshal.AllocHGlobal(batchSize*sizeof (Node));
					storage.Add(ptr);
					current = (Node*)ptr.ToPointer();
					capacity = currentCapacity;
				}
				else
					current = (Node*)storage[currentCapacity/batchSize - 1].ToPointer();
			}

			var result = current;
			current += count;
			Memory.count += count;
			return result;
		}

		public static void Release(int count)
		{
			if (Memory.count - count < currentCapacity - batchSize)
				throw new InvalidOperationException($"Invalid release: count is too big: {count}; cuurentCapacity: {currentCapacity}");
			current -= count;
			Memory.count -= count;
		}

		public static void Clear()
		{
			if (storage.Count <= 0)
				return;
			current = (Node*)storage[0].ToPointer();
			count = 0;
			currentCapacity = batchSize;
		}
	}
}