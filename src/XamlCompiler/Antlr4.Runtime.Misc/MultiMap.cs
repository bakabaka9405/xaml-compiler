using System;
using System.Collections.Generic;

namespace Antlr4.Runtime.Misc;

[Serializable]
public class MultiMap<K, V> : Dictionary<K, IList<V>>
{
	private const long serialVersionUID = -4956746660057462312L;

	public virtual void Map(K key, V value)
	{
		if (!TryGetValue(key, out var value2))
		{
			value2 = (base[key] = new List<V>());
		}
		value2.Add(value);
	}

	public virtual IList<Tuple<K, V>> GetPairs()
	{
		IList<Tuple<K, V>> list = new List<Tuple<K, V>>();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<K, IList<V>> current = enumerator.Current;
			foreach (V item in current.Value)
			{
				list.Add(Tuple.Create(current.Key, item));
			}
		}
		return list;
	}
}
