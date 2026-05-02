using System.Collections.Generic;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Dfa;

public sealed class SingletonEdgeMap<T> : AbstractEdgeMap<T> where T : class
{
	private readonly int key;

	private readonly T value;

	public int Key => key;

	public T Value => value;

	public override int Count => (value != null) ? 1 : 0;

	public override bool IsEmpty => value == null;

	public override T this[int key]
	{
		get
		{
			if (key == this.key)
			{
				return value;
			}
			return null;
		}
	}

	public SingletonEdgeMap(int minIndex, int maxIndex, int key, T value)
		: base(minIndex, maxIndex)
	{
		if (key >= minIndex && key <= maxIndex)
		{
			this.key = key;
			this.value = value;
		}
		else
		{
			this.key = 0;
			this.value = null;
		}
	}

	public override bool ContainsKey(int key)
	{
		if (key == this.key)
		{
			return value != null;
		}
		return false;
	}

	public override AbstractEdgeMap<T> Put(int key, T value)
	{
		if (key < minIndex || key > maxIndex)
		{
			return this;
		}
		if (key == this.key || this.value == null)
		{
			return new SingletonEdgeMap<T>(minIndex, maxIndex, key, value);
		}
		if (value != null)
		{
			AbstractEdgeMap<T> abstractEdgeMap = new SparseEdgeMap<T>(minIndex, maxIndex);
			abstractEdgeMap = abstractEdgeMap.Put(this.key, this.value);
			return abstractEdgeMap.Put(key, value);
		}
		return this;
	}

	public override AbstractEdgeMap<T> Remove(int key)
	{
		if (key == this.key && value != null)
		{
			return new EmptyEdgeMap<T>(minIndex, maxIndex);
		}
		return this;
	}

	public override AbstractEdgeMap<T> Clear()
	{
		if (value != null)
		{
			return new EmptyEdgeMap<T>(minIndex, maxIndex);
		}
		return this;
	}

	public override IDictionary<int, T> ToMap()
	{
		if (IsEmpty)
		{
			return Collections.EmptyMap<int, T>();
		}
		return Collections.SingletonMap(key, value);
	}
}
