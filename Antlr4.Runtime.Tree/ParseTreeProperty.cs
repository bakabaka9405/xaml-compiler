using System.Collections.Concurrent;

namespace Antlr4.Runtime.Tree;

public class ParseTreeProperty<V>
{
	protected internal ConcurrentDictionary<IParseTree, V> annotations = new ConcurrentDictionary<IParseTree, V>();

	public virtual V Get(IParseTree node)
	{
		if (!annotations.TryGetValue(node, out var value))
		{
			return default(V);
		}
		return value;
	}

	public virtual void Put(IParseTree node, V value)
	{
		annotations[node] = value;
	}

	public virtual V RemoveFrom(IParseTree node)
	{
		if (!annotations.TryRemove(node, out var value))
		{
			return default(V);
		}
		return value;
	}
}
