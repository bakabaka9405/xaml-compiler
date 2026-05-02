using System.Collections.Generic;

namespace Antlr4.Runtime.Sharpen;

internal static class ListExtensions
{
	public static T Set<T>(this IList<T> list, int index, T value) where T : class
	{
		T result = list[index];
		list[index] = value;
		return result;
	}
}
