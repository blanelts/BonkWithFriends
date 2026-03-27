using System;
using System.Runtime.InteropServices;

namespace Megabonk.BonkWithFriends.IO;

internal sealed class NativeMemoryPool : IDisposable
{
	internal static readonly NativeMemoryPool Shared = new NativeMemoryPool(1500, 1024);

	private readonly object _syncRoot = new object();

	private readonly int _size;

	private readonly int _amount;

	private readonly IntPtr[] _allocatedPools;

	private readonly bool[] _takenAllocatedPools;

	private bool _disposedValue;

	private NativeMemoryPool(int size, int amount)
	{
		_size = size;
		_amount = amount;
		lock (_syncRoot)
		{
			_allocatedPools = new IntPtr[_amount];
			_takenAllocatedPools = new bool[_amount];
			for (int i = 0; i < _allocatedPools.Length; i++)
			{
				IntPtr intPtr = Marshal.AllocHGlobal(_size);
				if (intPtr != IntPtr.Zero)
				{
					_allocatedPools[i] = intPtr;
				}
			}
		}
	}

	internal IntPtr Rent()
	{
		lock (_syncRoot)
		{
			for (int i = 0; i < _allocatedPools.Length; i++)
			{
			}
		}
		return IntPtr.Zero;
	}

	internal void Return(IntPtr allocatedMemoryPointer)
	{
	}

	private void FreePools()
	{
		lock (_syncRoot)
		{
			for (int i = 0; i < _allocatedPools.Length; i++)
			{
				IntPtr intPtr = _allocatedPools[i];
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
		}
	}

	private void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				FreePools();
			}
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
