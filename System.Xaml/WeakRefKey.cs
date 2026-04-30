namespace System.Xaml;

internal class WeakRefKey : WeakReference
{
	private int _hashCode;

	public WeakRefKey(object target)
		: base(target)
	{
		_hashCode = target.GetHashCode();
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	public override bool Equals(object o)
	{
		WeakRefKey weakRefKey = o as WeakRefKey;
		if (weakRefKey != null)
		{
			object target = Target;
			object target2 = weakRefKey.Target;
			if (target != null && target2 != null)
			{
				return target == target2;
			}
		}
		return base.Equals(o);
	}

	public static bool operator ==(WeakRefKey left, WeakRefKey right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(WeakRefKey left, WeakRefKey right)
	{
		return !(left == right);
	}
}
