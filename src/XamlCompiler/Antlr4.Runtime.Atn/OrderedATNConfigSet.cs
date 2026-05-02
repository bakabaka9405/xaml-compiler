namespace Antlr4.Runtime.Atn;

public class OrderedATNConfigSet : ATNConfigSet
{
	public OrderedATNConfigSet()
	{
	}

	public OrderedATNConfigSet(ATNConfigSet set, bool @readonly)
		: base(set, @readonly)
	{
	}

	public override ATNConfigSet Clone(bool @readonly)
	{
		OrderedATNConfigSet orderedATNConfigSet = new OrderedATNConfigSet(this, @readonly);
		if (!@readonly && base.IsReadOnly)
		{
			orderedATNConfigSet.AddAll(this);
		}
		return orderedATNConfigSet;
	}

	protected internal override long GetKey(ATNConfig e)
	{
		return e.GetHashCode();
	}

	protected internal override bool CanMerge(ATNConfig left, long leftKey, ATNConfig right)
	{
		return left.Equals(right);
	}
}
