using System.Collections;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Dfa;

public interface IEdgeMap<T> : IEnumerable<KeyValuePair<int, T>>, IEnumerable
{
	int Count { get; }

	bool IsEmpty { get; }

	T this[int key] { get; }

	bool ContainsKey(int key);

	[return: NotNull]
	IEdgeMap<T> Put(int key, T value);

	[return: NotNull]
	IEdgeMap<T> Remove(int key);

	[return: NotNull]
	IEdgeMap<T> PutAll(IEdgeMap<T> m);

	[return: NotNull]
	IEdgeMap<T> Clear();

	[return: NotNull]
	IDictionary<int, T> ToMap();
}
