using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public class ConflictInfo
{
	private readonly BitSet conflictedAlts;

	private readonly bool exact;

	public BitSet ConflictedAlts => conflictedAlts;

	public bool IsExact => exact;

	public ConflictInfo(BitSet conflictedAlts, bool exact)
	{
		this.conflictedAlts = conflictedAlts;
		this.exact = exact;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is ConflictInfo))
		{
			return false;
		}
		ConflictInfo conflictInfo = (ConflictInfo)obj;
		if (IsExact == conflictInfo.IsExact)
		{
			return object.Equals(ConflictedAlts, conflictInfo.ConflictedAlts);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ConflictedAlts.GetHashCode();
	}
}
