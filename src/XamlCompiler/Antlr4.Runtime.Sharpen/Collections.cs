using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Antlr4.Runtime.Sharpen;

internal static class Collections
{
	private static class EmptyListImpl<T>
	{
		public static readonly T[] Instance = new T[0];
	}

	private static class EmptyMapImpl<TKey, TValue>
	{
		public static IDictionary<TKey, TValue> Instance => new Dictionary<TKey, TValue>();
	}

	public static T[] EmptyList<T>()
	{
		return EmptyListImpl<T>.Instance;
	}

	public static IDictionary<TKey, TValue> EmptyMap<TKey, TValue>()
	{
		return EmptyMapImpl<TKey, TValue>.Instance;
	}

	public static ReadOnlyCollection<T> SingletonList<T>(T item)
	{
		return new ReadOnlyCollection<T>(new T[1] { item });
	}

	public static IDictionary<TKey, TValue> SingletonMap<TKey, TValue>(TKey key, TValue value)
	{
		return new Dictionary<TKey, TValue> { { key, value } };
	}
}
