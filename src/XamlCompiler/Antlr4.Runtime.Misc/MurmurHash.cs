namespace Antlr4.Runtime.Misc;

public sealed class MurmurHash
{
	private const int DefaultSeed = 0;

	public static int Initialize()
	{
		return Initialize(0);
	}

	public static int Initialize(int seed)
	{
		return seed;
	}

	public static int Update(int hash, int value)
	{
		int num = -862048943;
		int num2 = 461845907;
		int num3 = 15;
		int num4 = 13;
		int num5 = 5;
		int num6 = -430675100;
		int num7 = value;
		num7 *= num;
		num7 = (num7 << num3) | (num7 >>> 32 - num3);
		num7 *= num2;
		hash ^= num7;
		hash = (hash << num4) | (hash >>> 32 - num4);
		hash = hash * num5 + num6;
		return hash;
	}

	public static int Update(int hash, object value)
	{
		return Update(hash, value?.GetHashCode() ?? 0);
	}

	public static int Finish(int hash, int numberOfWords)
	{
		hash ^= numberOfWords * 4;
		hash ^= hash >>> 16;
		hash *= -2048144789;
		hash ^= hash >>> 13;
		hash *= -1028477387;
		hash ^= hash >>> 16;
		return hash;
	}

	public static int HashCode<T>(T[] data, int seed)
	{
		int hash = Initialize(seed);
		foreach (T val in data)
		{
			hash = Update(hash, val);
		}
		return Finish(hash, data.Length);
	}

	private MurmurHash()
	{
	}
}
