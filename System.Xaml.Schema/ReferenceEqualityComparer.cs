using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Xaml.Schema;

internal class ReferenceEqualityComparer<T> : EqualityComparer<T> where T : class
{
	internal static ReferenceEqualityComparer<T> Singleton = new ReferenceEqualityComparer<T>();

	public override bool Equals(T x, T y)
	{
		return x == y;
	}

	public override int GetHashCode(T obj)
	{
		return RuntimeHelpers.GetHashCode(obj);
	}
}
