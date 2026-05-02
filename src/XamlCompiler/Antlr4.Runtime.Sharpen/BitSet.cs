using System;
using System.Text;

namespace Antlr4.Runtime.Sharpen;

public class BitSet
{
	private static readonly ulong[] EmptyBits = new ulong[0];

	private const int BitsPerElement = 64;

	private ulong[] _data = EmptyBits;

	private static readonly int[] index64 = new int[64]
	{
		0, 47, 1, 56, 48, 27, 2, 60, 57, 49,
		41, 37, 28, 16, 3, 61, 54, 58, 35, 52,
		50, 42, 21, 44, 38, 32, 29, 23, 17, 11,
		4, 62, 46, 55, 26, 59, 40, 36, 15, 53,
		34, 51, 20, 43, 31, 22, 10, 45, 25, 39,
		14, 33, 19, 30, 9, 24, 13, 18, 8, 12,
		7, 6, 5, 63
	};

	public BitSet()
	{
	}

	public BitSet(int nbits)
	{
		if (nbits < 0)
		{
			throw new ArgumentOutOfRangeException("nbits");
		}
		if (nbits > 0)
		{
			int num = (nbits + 64 - 1) / 64;
			_data = new ulong[num];
		}
	}

	private static int GetBitCount(ulong[] value)
	{
		int num = 0;
		uint num2 = (uint)value.Length;
		uint num3 = 0u;
		uint num4 = num2 - num2 % 30;
		uint num5 = 0u;
		while (num5 < num4)
		{
			ulong num6 = 0uL;
			for (uint num7 = 0u; num7 < 30; num7 += 3)
			{
				ulong num8 = value[num + num7];
				ulong num9 = value[num + num7 + 1];
				ulong num10 = value[num + num7 + 2];
				ulong num11 = num10;
				num10 &= 0x5555555555555555L;
				num11 = (num11 >> 1) & 0x5555555555555555L;
				num8 -= (num8 >> 1) & 0x5555555555555555L;
				num9 -= (num9 >> 1) & 0x5555555555555555L;
				num8 += num10;
				num9 += num11;
				num8 = (num8 & 0x3333333333333333L) + ((num8 >> 2) & 0x3333333333333333L);
				num8 += (num9 & 0x3333333333333333L) + ((num9 >> 2) & 0x3333333333333333L);
				num6 += (num8 & 0xF0F0F0F0F0F0F0FL) + ((num8 >> 4) & 0xF0F0F0F0F0F0F0FL);
			}
			num6 = (num6 & 0xFF00FF00FF00FFL) + ((num6 >> 8) & 0xFF00FF00FF00FFL);
			num6 = (num6 + (num6 >> 16)) & 0xFFFF0000FFFFL;
			num6 += num6 >> 32;
			num3 += (uint)(int)num6;
			num5 += 30;
			num += 30;
		}
		for (uint num12 = 0u; num12 < num2 - num4; num12++)
		{
			ulong num13 = value[num + num12];
			num13 -= (num13 >> 1) & 0x5555555555555555L;
			num13 = (num13 & 0x3333333333333333L) + ((num13 >> 2) & 0x3333333333333333L);
			num13 = (num13 + (num13 >> 4)) & 0xF0F0F0F0F0F0F0FL;
			num3 += (uint)(int)(num13 * 72340172838076673L >> 56);
		}
		return (int)num3;
	}

	private static int BitScanForward(ulong value)
	{
		if (value == 0L)
		{
			return -1;
		}
		return index64[(value ^ (value - 1)) * 285870213051386505L >> 58];
	}

	public BitSet Clone()
	{
		BitSet bitSet = new BitSet();
		bitSet._data = (ulong[])_data.Clone();
		return bitSet;
	}

	public void Clear(int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		int num = index / 64;
		if (num < _data.Length)
		{
			_data[num] &= (ulong)(~(1L << index % 64));
		}
	}

	public bool Get(int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		int num = index / 64;
		if (num >= _data.Length)
		{
			return false;
		}
		return (_data[num] & (ulong)(1L << index % 64)) != 0;
	}

	public void Set(int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		int num = index / 64;
		if (num >= _data.Length)
		{
			Array.Resize(ref _data, Math.Max(_data.Length * 2, num + 1));
		}
		_data[num] |= (ulong)(1L << index % 64);
	}

	public bool IsEmpty()
	{
		for (int i = 0; i < _data.Length; i++)
		{
			if (_data[i] != 0L)
			{
				return false;
			}
		}
		return true;
	}

	public int Cardinality()
	{
		return GetBitCount(_data);
	}

	public int NextSetBit(int fromIndex)
	{
		if (fromIndex < 0)
		{
			throw new ArgumentOutOfRangeException("fromIndex");
		}
		if (IsEmpty())
		{
			return -1;
		}
		int num = fromIndex / 64;
		if (num >= _data.Length)
		{
			return -1;
		}
		ulong value = _data[num] & (ulong)(~((1L << fromIndex % 64) - 1));
		while (true)
		{
			int num2 = BitScanForward(value);
			if (num2 >= 0)
			{
				return num2 + num * 64;
			}
			num++;
			if (num >= _data.Length)
			{
				break;
			}
			value = _data[num];
		}
		return -1;
	}

	public void And(BitSet set)
	{
		if (set == null)
		{
			throw new ArgumentNullException("set");
		}
		int num = Math.Min(_data.Length, set._data.Length);
		for (int i = 0; i < num; i++)
		{
			_data[i] &= set._data[i];
		}
		for (int j = num; j < _data.Length; j++)
		{
			_data[j] = 0uL;
		}
	}

	public void Or(BitSet set)
	{
		if (set == null)
		{
			throw new ArgumentNullException("set");
		}
		if (set._data.Length > _data.Length)
		{
			Array.Resize(ref _data, set._data.Length);
		}
		for (int i = 0; i < set._data.Length; i++)
		{
			_data[i] |= set._data[i];
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BitSet bitSet))
		{
			return false;
		}
		if (IsEmpty())
		{
			return bitSet.IsEmpty();
		}
		int num = Math.Min(_data.Length, bitSet._data.Length);
		for (int i = 0; i < num; i++)
		{
			if (_data[i] != bitSet._data[i])
			{
				return false;
			}
		}
		for (int j = num; j < _data.Length; j++)
		{
			if (_data[j] != 0L)
			{
				return false;
			}
		}
		for (int k = num; k < bitSet._data.Length; k++)
		{
			if (bitSet._data[k] != 0L)
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		ulong num = 1uL;
		for (uint num2 = 0u; num2 < _data.Length; num2++)
		{
			if (_data[num2] != 0L)
			{
				num = (num * 31) ^ num2;
				num = (num * 31) ^ _data[num2];
			}
		}
		return num.GetHashCode();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('{');
		for (int num = NextSetBit(0); num >= 0; num = NextSetBit(num + 1))
		{
			if (stringBuilder.Length > 1)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(num);
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}
}
