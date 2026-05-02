using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal static class StringBuilderPool
{
	private const int DefaultCapacity = 128;

	private const int MaxListSize = 5;

	private const int MaxCapacity = 4096;

	private static StringBuilder[] s_pool = new StringBuilder[5];

	private static object s_synclock = new object();

	public static StringBuilder Get()
	{
		return Get(128);
	}

	public static StringBuilder Get(int capacity)
	{
		StringBuilder stringBuilder = null;
		lock (s_synclock)
		{
			for (int i = 0; i < s_pool.Length; i++)
			{
				if (s_pool[i] != null)
				{
					stringBuilder = s_pool[i];
					s_pool[i] = null;
					break;
				}
			}
		}
		if (stringBuilder == null)
		{
			stringBuilder = new StringBuilder(capacity);
		}
		stringBuilder.Length = 0;
		stringBuilder.EnsureCapacity(capacity);
		return stringBuilder;
	}

	public static void Release(ref StringBuilder builder)
	{
		if (builder != null && builder.Capacity < 4096)
		{
			lock (s_synclock)
			{
				for (int i = 0; i < s_pool.Length; i++)
				{
					if (s_pool[i] == null)
					{
						s_pool[i] = builder;
						break;
					}
				}
			}
		}
		builder = null;
	}
}
