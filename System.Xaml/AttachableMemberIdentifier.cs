namespace System.Xaml;

public class AttachableMemberIdentifier : IEquatable<AttachableMemberIdentifier>
{
	private Type declaringType;

	private string memberName;

	public string MemberName => memberName;

	public Type DeclaringType => declaringType;

	public AttachableMemberIdentifier(Type declaringType, string memberName)
	{
		this.declaringType = declaringType;
		this.memberName = memberName;
	}

	public static bool operator !=(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
	{
		return !(left == right);
	}

	public static bool operator ==(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as AttachableMemberIdentifier);
	}

	public bool Equals(AttachableMemberIdentifier other)
	{
		if (other == null)
		{
			return false;
		}
		if (declaringType == other.declaringType)
		{
			return memberName == other.memberName;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = ((!(declaringType == null)) ? declaringType.GetHashCode() : 0);
		int num2 = ((memberName != null) ? memberName.GetHashCode() : 0);
		return ((num << 5) + num) ^ num2;
	}

	public override string ToString()
	{
		if (declaringType == null)
		{
			return memberName;
		}
		return declaringType.ToString() + "." + memberName;
	}
}
