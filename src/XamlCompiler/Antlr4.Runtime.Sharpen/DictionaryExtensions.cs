using System.Collections.Generic;

namespace Antlr4.Runtime.Sharpen;

internal static class DictionaryExtensions
{
	public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : class
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public static TValue Put<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TValue : class
	{
		if (!dictionary.TryGetValue(key, out var value2))
		{
			value2 = null;
		}
		dictionary[key] = value;
		return value2;
	}
}
