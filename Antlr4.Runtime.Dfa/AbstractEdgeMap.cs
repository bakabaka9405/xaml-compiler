using System.Collections;
using System.Collections.Generic;

namespace Antlr4.Runtime.Dfa;

public abstract class AbstractEdgeMap<T> : IEdgeMap<T>, IEnumerable<KeyValuePair<int, T>>, IEnumerable where T : class
{
	protected internal readonly int minIndex;

	protected internal readonly int maxIndex;

	public abstract T this[int arg1] { get; }

	public abstract bool IsEmpty { get; }

	public abstract int Count { get; }

	protected AbstractEdgeMap(int minIndex, int maxIndex)
	{
		this.minIndex = minIndex;
		this.maxIndex = maxIndex;
	}

	public abstract AbstractEdgeMap<T> Put(int key, T value);

	IEdgeMap<T> IEdgeMap<T>.Put(int key, T value)
	{
		return Put(key, value);
	}

	public virtual AbstractEdgeMap<T> PutAll(IEdgeMap<T> m)
	{
		AbstractEdgeMap<T> abstractEdgeMap = this;
		foreach (KeyValuePair<int, T> item in m)
		{
			abstractEdgeMap = abstractEdgeMap.Put(item.Key, item.Value);
		}
		return abstractEdgeMap;
	}

	IEdgeMap<T> IEdgeMap<T>.PutAll(IEdgeMap<T> m)
	{
		return PutAll(m);
	}

	public abstract AbstractEdgeMap<T> Clear();

	IEdgeMap<T> IEdgeMap<T>.Clear()
	{
		return Clear();
	}

	public abstract AbstractEdgeMap<T> Remove(int key);

	IEdgeMap<T> IEdgeMap<T>.Remove(int key)
	{
		return Remove(key);
	}

	public abstract bool ContainsKey(int arg1);

	public abstract IDictionary<int, T> ToMap();

	public virtual IEnumerator<KeyValuePair<int, T>> GetEnumerator()
	{
		return ToMap().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
