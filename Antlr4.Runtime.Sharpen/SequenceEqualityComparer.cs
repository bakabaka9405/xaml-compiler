using System.Collections.Generic;
using System.Linq;

namespace Antlr4.Runtime.Sharpen;

internal class SequenceEqualityComparer<T> : EqualityComparer<IEnumerable<T>>
{
	private static readonly SequenceEqualityComparer<T> _default = new SequenceEqualityComparer<T>();

	private readonly IEqualityComparer<T> _elementEqualityComparer = EqualityComparer<T>.Default;

	public new static SequenceEqualityComparer<T> Default => _default;

	public SequenceEqualityComparer()
		: this((IEqualityComparer<T>)null)
	{
	}

	public SequenceEqualityComparer(IEqualityComparer<T> elementComparer)
	{
		_elementEqualityComparer = elementComparer ?? EqualityComparer<T>.Default;
	}

	public override bool Equals(IEnumerable<T> x, IEnumerable<T> y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null || y == null)
		{
			return false;
		}
		return x.SequenceEqual(y, _elementEqualityComparer);
	}

	public override int GetHashCode(IEnumerable<T> obj)
	{
		if (obj == null)
		{
			return 0;
		}
		int num = 1;
		foreach (T item in obj)
		{
			num = 31 * num + _elementEqualityComparer.GetHashCode(item);
		}
		return num;
	}
}
