using System;

namespace Antlr4.Runtime.Misc;

public struct Interval(int a, int b)
{
	public static readonly Interval Invalid = new Interval(-1, -2);

	public readonly int a = a;

	public readonly int b = b;

	public int Length
	{
		get
		{
			if (b < a)
			{
				return 0;
			}
			return b - a + 1;
		}
	}

	public static Interval Of(int a, int b)
	{
		return new Interval(a, b);
	}

	public override bool Equals(object o)
	{
		if (!(o is Interval interval))
		{
			return false;
		}
		if (a == interval.a)
		{
			return b == interval.b;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 23;
		num = num * 31 + a;
		return num * 31 + b;
	}

	public bool StartsBeforeDisjoint(Interval other)
	{
		if (a < other.a)
		{
			return b < other.a;
		}
		return false;
	}

	public bool StartsBeforeNonDisjoint(Interval other)
	{
		if (a <= other.a)
		{
			return b >= other.a;
		}
		return false;
	}

	public bool StartsAfter(Interval other)
	{
		return a > other.a;
	}

	public bool StartsAfterDisjoint(Interval other)
	{
		return a > other.b;
	}

	public bool StartsAfterNonDisjoint(Interval other)
	{
		if (a > other.a)
		{
			return a <= other.b;
		}
		return false;
	}

	public bool Disjoint(Interval other)
	{
		if (!StartsBeforeDisjoint(other))
		{
			return StartsAfterDisjoint(other);
		}
		return true;
	}

	public bool Adjacent(Interval other)
	{
		if (a != other.b + 1)
		{
			return b == other.a - 1;
		}
		return true;
	}

	public bool ProperlyContains(Interval other)
	{
		if (other.a >= a)
		{
			return other.b <= b;
		}
		return false;
	}

	public Interval Union(Interval other)
	{
		return Of(Math.Min(a, other.a), Math.Max(b, other.b));
	}

	public Interval Intersection(Interval other)
	{
		return Of(Math.Max(a, other.a), Math.Min(b, other.b));
	}

	public Interval? DifferenceNotProperlyContained(Interval other)
	{
		Interval? result = null;
		if (other.StartsBeforeNonDisjoint(this))
		{
			result = Of(Math.Max(a, other.b + 1), b);
		}
		else if (other.StartsAfterNonDisjoint(this))
		{
			result = Of(a, other.a - 1);
		}
		return result;
	}

	public override string ToString()
	{
		return a + ".." + b;
	}
}
