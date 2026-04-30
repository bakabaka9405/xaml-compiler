using System.Collections.Generic;

namespace System.Xaml;

internal static class ArrayHelper
{
	internal static S[] ConvertArrayType<R, S>(ICollection<R> src, Func<R, S> f)
	{
		if (src == null)
		{
			return null;
		}
		int count = src.Count;
		int num = 0;
		S[] array = new S[count];
		foreach (R item in src)
		{
			array[num++] = f(item);
		}
		return array;
	}

	internal static void ForAll<R>(R[] src, Action<R> f)
	{
		foreach (R obj in src)
		{
			f(obj);
		}
	}

	internal static List<T> ToList<T>(IEnumerable<T> src)
	{
		if (src == null)
		{
			return null;
		}
		return new List<T>(src);
	}
}
