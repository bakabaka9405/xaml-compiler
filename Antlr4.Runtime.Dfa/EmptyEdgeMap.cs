using System.Collections.Generic;

namespace Antlr4.Runtime.Dfa;

public sealed class EmptyEdgeMap<T> : AbstractEdgeMap<T> where T : class
{
	public override int Count => 0;

	public override bool IsEmpty => true;

	public override T this[int key] => null;

	public EmptyEdgeMap(int minIndex, int maxIndex)
		: base(minIndex, maxIndex)
	{
	}

	public override AbstractEdgeMap<T> Put(int key, T value)
	{
		if (value == null || key < minIndex || key > maxIndex)
		{
			return this;
		}
		return new SingletonEdgeMap<T>(minIndex, maxIndex, key, value);
	}

	public override AbstractEdgeMap<T> Clear()
	{
		return this;
	}

	public override AbstractEdgeMap<T> Remove(int key)
	{
		return this;
	}

	public override bool ContainsKey(int key)
	{
		return false;
	}

	public override IDictionary<int, T> ToMap()
	{
		return new Dictionary<int, T>();
	}
}
