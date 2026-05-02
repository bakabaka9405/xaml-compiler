using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antlr4.Runtime.Misc;

public class IntervalSet : IIntSet
{
	public static readonly IntervalSet CompleteCharSet;

	public static readonly IntervalSet EmptySet;

	protected internal IList<Interval> intervals;

	protected internal bool @readonly;

	public virtual bool IsNil
	{
		get
		{
			if (intervals != null)
			{
				return intervals.Count == 0;
			}
			return true;
		}
	}

	public virtual int SingleElement
	{
		get
		{
			if (intervals != null && intervals.Count == 1)
			{
				Interval interval = intervals[0];
				if (interval.a == interval.b)
				{
					return interval.a;
				}
			}
			return 0;
		}
	}

	public virtual int MaxElement
	{
		get
		{
			if (IsNil)
			{
				return 0;
			}
			return intervals[intervals.Count - 1].b;
		}
	}

	public virtual int MinElement
	{
		get
		{
			if (IsNil)
			{
				return 0;
			}
			return intervals[0].a;
		}
	}

	public virtual int Count
	{
		get
		{
			int num = 0;
			int count = intervals.Count;
			if (count == 1)
			{
				Interval interval = intervals[0];
				return interval.b - interval.a + 1;
			}
			for (int i = 0; i < count; i++)
			{
				Interval interval2 = intervals[i];
				num += interval2.b - interval2.a + 1;
			}
			return num;
		}
	}

	public virtual bool IsReadOnly => @readonly;

	static IntervalSet()
	{
		CompleteCharSet = Of(0, 65534);
		EmptySet = new IntervalSet();
		CompleteCharSet.SetReadonly(@readonly: true);
		EmptySet.SetReadonly(@readonly: true);
	}

	public IntervalSet(IList<Interval> intervals)
	{
		this.intervals = intervals;
	}

	public IntervalSet(IntervalSet set)
		: this()
	{
		AddAll(set);
	}

	public IntervalSet(params int[] els)
	{
		if (els == null)
		{
			intervals = new List<Interval>(2);
			return;
		}
		intervals = new List<Interval>(els.Length);
		foreach (int el in els)
		{
			Add(el);
		}
	}

	[return: NotNull]
	public static IntervalSet Of(int a)
	{
		IntervalSet intervalSet = new IntervalSet();
		intervalSet.Add(a);
		return intervalSet;
	}

	public static IntervalSet Of(int a, int b)
	{
		IntervalSet intervalSet = new IntervalSet();
		intervalSet.Add(a, b);
		return intervalSet;
	}

	public virtual void Clear()
	{
		if (@readonly)
		{
			throw new InvalidOperationException("can't alter readonly IntervalSet");
		}
		intervals.Clear();
	}

	public virtual void Add(int el)
	{
		if (@readonly)
		{
			throw new InvalidOperationException("can't alter readonly IntervalSet");
		}
		Add(el, el);
	}

	public virtual void Add(int a, int b)
	{
		Add(Interval.Of(a, b));
	}

	protected internal virtual void Add(Interval addition)
	{
		if (@readonly)
		{
			throw new InvalidOperationException("can't alter readonly IntervalSet");
		}
		if (addition.b < addition.a)
		{
			return;
		}
		for (int i = 0; i < intervals.Count; i++)
		{
			Interval interval = intervals[i];
			if (addition.Equals(interval))
			{
				return;
			}
			if (addition.Adjacent(interval) || !addition.Disjoint(interval))
			{
				Interval value = addition.Union(interval);
				intervals[i] = value;
				while (i < intervals.Count - 1)
				{
					i++;
					Interval other = intervals[i];
					if (value.Adjacent(other) || !value.Disjoint(other))
					{
						intervals.RemoveAt(i);
						i--;
						intervals[i] = value.Union(other);
						continue;
					}
					break;
				}
				return;
			}
			if (addition.StartsBeforeDisjoint(interval))
			{
				intervals.Insert(i, addition);
				return;
			}
		}
		intervals.Add(addition);
	}

	public static IntervalSet Or(IntervalSet[] sets)
	{
		IntervalSet intervalSet = new IntervalSet();
		foreach (IntervalSet set in sets)
		{
			intervalSet.AddAll(set);
		}
		return intervalSet;
	}

	public virtual IntervalSet AddAll(IIntSet set)
	{
		if (set == null)
		{
			return this;
		}
		if (set is IntervalSet)
		{
			IntervalSet intervalSet = (IntervalSet)set;
			int count = intervalSet.intervals.Count;
			for (int i = 0; i < count; i++)
			{
				Interval interval = intervalSet.intervals[i];
				Add(interval.a, interval.b);
			}
		}
		else
		{
			foreach (int item in set.ToList())
			{
				Add(item);
			}
		}
		return this;
	}

	public virtual IntervalSet Complement(int minElement, int maxElement)
	{
		return Complement(Of(minElement, maxElement));
	}

	public virtual IntervalSet Complement(IIntSet vocabulary)
	{
		if (vocabulary == null || vocabulary.IsNil)
		{
			return null;
		}
		IntervalSet intervalSet;
		if (vocabulary is IntervalSet)
		{
			intervalSet = (IntervalSet)vocabulary;
		}
		else
		{
			intervalSet = new IntervalSet();
			intervalSet.AddAll(vocabulary);
		}
		return intervalSet.Subtract(this);
	}

	public virtual IntervalSet Subtract(IIntSet a)
	{
		if (a == null || a.IsNil)
		{
			return new IntervalSet(this);
		}
		if (a is IntervalSet)
		{
			return Subtract(this, (IntervalSet)a);
		}
		IntervalSet intervalSet = new IntervalSet();
		intervalSet.AddAll(a);
		return Subtract(this, intervalSet);
	}

	[return: NotNull]
	public static IntervalSet Subtract(IntervalSet left, IntervalSet right)
	{
		if (left == null || left.IsNil)
		{
			return new IntervalSet();
		}
		IntervalSet intervalSet = new IntervalSet(left);
		if (right == null || right.IsNil)
		{
			return intervalSet;
		}
		int num = 0;
		int num2 = 0;
		while (num < intervalSet.intervals.Count && num2 < right.intervals.Count)
		{
			Interval interval = intervalSet.intervals[num];
			Interval interval2 = right.intervals[num2];
			if (interval2.b < interval.a)
			{
				num2++;
				continue;
			}
			if (interval2.a > interval.b)
			{
				num++;
				continue;
			}
			Interval? interval3 = null;
			Interval? interval4 = null;
			if (interval2.a > interval.a)
			{
				interval3 = new Interval(interval.a, interval2.a - 1);
			}
			if (interval2.b < interval.b)
			{
				interval4 = new Interval(interval2.b + 1, interval.b);
			}
			if (interval3.HasValue)
			{
				if (interval4.HasValue)
				{
					intervalSet.intervals[num] = interval3.Value;
					intervalSet.intervals.Insert(num + 1, interval4.Value);
					num++;
					num2++;
				}
				else
				{
					intervalSet.intervals[num] = interval3.Value;
					num++;
				}
			}
			else if (interval4.HasValue)
			{
				intervalSet.intervals[num] = interval4.Value;
				num2++;
			}
			else
			{
				intervalSet.intervals.RemoveAt(num);
			}
		}
		return intervalSet;
	}

	public virtual IntervalSet Or(IIntSet a)
	{
		IntervalSet intervalSet = new IntervalSet();
		intervalSet.AddAll(this);
		intervalSet.AddAll(a);
		return intervalSet;
	}

	public virtual IntervalSet And(IIntSet other)
	{
		if (other == null)
		{
			return null;
		}
		IList<Interval> list = intervals;
		IList<Interval> list2 = ((IntervalSet)other).intervals;
		IntervalSet intervalSet = null;
		int count = list.Count;
		int count2 = list2.Count;
		int num = 0;
		int num2 = 0;
		while (num < count && num2 < count2)
		{
			Interval other2 = list[num];
			Interval other3 = list2[num2];
			if (other2.StartsBeforeDisjoint(other3))
			{
				num++;
			}
			else if (other3.StartsBeforeDisjoint(other2))
			{
				num2++;
			}
			else if (other2.ProperlyContains(other3))
			{
				if (intervalSet == null)
				{
					intervalSet = new IntervalSet();
				}
				intervalSet.Add(other2.Intersection(other3));
				num2++;
			}
			else if (other3.ProperlyContains(other2))
			{
				if (intervalSet == null)
				{
					intervalSet = new IntervalSet();
				}
				intervalSet.Add(other2.Intersection(other3));
				num++;
			}
			else if (!other2.Disjoint(other3))
			{
				if (intervalSet == null)
				{
					intervalSet = new IntervalSet();
				}
				intervalSet.Add(other2.Intersection(other3));
				if (other2.StartsAfterNonDisjoint(other3))
				{
					num2++;
				}
				else if (other3.StartsAfterNonDisjoint(other2))
				{
					num++;
				}
			}
		}
		if (intervalSet == null)
		{
			return new IntervalSet();
		}
		return intervalSet;
	}

	public virtual bool Contains(int el)
	{
		int count = intervals.Count;
		for (int i = 0; i < count; i++)
		{
			Interval interval = intervals[i];
			int a = interval.a;
			int b = interval.b;
			if (el < a)
			{
				break;
			}
			if (el >= a && el <= b)
			{
				return true;
			}
		}
		return false;
	}

	public virtual IList<Interval> GetIntervals()
	{
		return intervals;
	}

	public override int GetHashCode()
	{
		int hash = MurmurHash.Initialize();
		foreach (Interval interval in intervals)
		{
			hash = MurmurHash.Update(hash, interval.a);
			hash = MurmurHash.Update(hash, interval.b);
		}
		return MurmurHash.Finish(hash, intervals.Count * 2);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is IntervalSet))
		{
			return false;
		}
		IntervalSet intervalSet = (IntervalSet)obj;
		return intervals.SequenceEqual(intervalSet.intervals);
	}

	public override string ToString()
	{
		return ToString(elemAreChar: false);
	}

	public virtual string ToString(bool elemAreChar)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (intervals == null || intervals.Count == 0)
		{
			return "{}";
		}
		if (Count > 1)
		{
			stringBuilder.Append("{");
		}
		bool flag = true;
		foreach (Interval interval in intervals)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			flag = false;
			int a = interval.a;
			int b = interval.b;
			if (a == b)
			{
				if (a == -1)
				{
					stringBuilder.Append("<EOF>");
				}
				else if (elemAreChar)
				{
					stringBuilder.Append("'").Append((char)a).Append("'");
				}
				else
				{
					stringBuilder.Append(a);
				}
			}
			else if (elemAreChar)
			{
				stringBuilder.Append("'").Append((char)a).Append("'..'")
					.Append((char)b)
					.Append("'");
			}
			else
			{
				stringBuilder.Append(a).Append("..").Append(b);
			}
		}
		if (Count > 1)
		{
			stringBuilder.Append("}");
		}
		return stringBuilder.ToString();
	}

	public virtual string ToString(IVocabulary vocabulary)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (intervals == null || intervals.Count == 0)
		{
			return "{}";
		}
		if (Count > 1)
		{
			stringBuilder.Append("{");
		}
		bool flag = true;
		foreach (Interval interval in intervals)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			flag = false;
			int a = interval.a;
			int b = interval.b;
			if (a == b)
			{
				stringBuilder.Append(ElementName(vocabulary, a));
				continue;
			}
			for (int i = a; i <= b; i++)
			{
				if (i > a)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(ElementName(vocabulary, i));
			}
		}
		if (Count > 1)
		{
			stringBuilder.Append("}");
		}
		return stringBuilder.ToString();
	}

	[return: NotNull]
	protected internal virtual string ElementName(IVocabulary vocabulary, int a)
	{
		return a switch
		{
			-1 => "<EOF>", 
			-2 => "<EPSILON>", 
			_ => vocabulary.GetDisplayName(a), 
		};
	}

	public virtual List<int> ToIntegerList()
	{
		List<int> list = new List<int>(Count);
		int count = intervals.Count;
		for (int i = 0; i < count; i++)
		{
			Interval interval = intervals[i];
			int a = interval.a;
			int b = interval.b;
			for (int j = a; j <= b; j++)
			{
				list.Add(j);
			}
		}
		return list;
	}

	public virtual IList<int> ToList()
	{
		IList<int> list = new List<int>();
		int count = intervals.Count;
		for (int i = 0; i < count; i++)
		{
			Interval interval = intervals[i];
			int a = interval.a;
			int b = interval.b;
			for (int j = a; j <= b; j++)
			{
				list.Add(j);
			}
		}
		return list;
	}

	public virtual HashSet<int> ToSet()
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (Interval interval in intervals)
		{
			int a = interval.a;
			int b = interval.b;
			for (int i = a; i <= b; i++)
			{
				hashSet.Add(i);
			}
		}
		return hashSet;
	}

	public virtual int[] ToArray()
	{
		return ToIntegerList().ToArray();
	}

	public virtual void Remove(int el)
	{
		if (@readonly)
		{
			throw new InvalidOperationException("can't alter readonly IntervalSet");
		}
		int count = intervals.Count;
		for (int i = 0; i < count; i++)
		{
			Interval interval = intervals[i];
			int a = interval.a;
			int b = interval.b;
			if (el >= a)
			{
				if (el == a && el == b)
				{
					intervals.RemoveAt(i);
					break;
				}
				if (el == a)
				{
					intervals[i] = Interval.Of(interval.a + 1, interval.b);
					break;
				}
				if (el == b)
				{
					intervals[i] = Interval.Of(interval.a, interval.b - 1);
					break;
				}
				if (el > a && el < b)
				{
					int b2 = interval.b;
					intervals[i] = Interval.Of(interval.a, el - 1);
					Add(el + 1, b2);
				}
				continue;
			}
			break;
		}
	}

	public virtual void SetReadonly(bool @readonly)
	{
		if (this.@readonly && !@readonly)
		{
			throw new InvalidOperationException("can't alter readonly IntervalSet");
		}
		this.@readonly = @readonly;
	}

	IIntSet IIntSet.AddAll(IIntSet set)
	{
		return AddAll(set);
	}

	IIntSet IIntSet.And(IIntSet a)
	{
		return And(a);
	}

	IIntSet IIntSet.Complement(IIntSet elements)
	{
		return Complement(elements);
	}

	IIntSet IIntSet.Or(IIntSet a)
	{
		return Or(a);
	}

	IIntSet IIntSet.Subtract(IIntSet a)
	{
		return Subtract(a);
	}
}
