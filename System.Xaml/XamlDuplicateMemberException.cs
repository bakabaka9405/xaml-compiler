using System.Runtime.Serialization;
using System.Security;

namespace System.Xaml;

[Serializable]
public class XamlDuplicateMemberException : XamlException
{
	public XamlMember DuplicateMember { get; set; }

	public XamlType ParentType { get; set; }

	public XamlDuplicateMemberException()
	{
	}

	public XamlDuplicateMemberException(XamlMember member, XamlType type)
		: base(SR.Get("DuplicateMemberSet", (member != null) ? member.Name : null, (type != null) ? type.Name : null))
	{
		DuplicateMember = member;
		ParentType = type;
	}

	public XamlDuplicateMemberException(string message)
		: base(message)
	{
	}

	public XamlDuplicateMemberException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected XamlDuplicateMemberException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		DuplicateMember = (XamlMember)info.GetValue("DuplicateMember", typeof(XamlMember));
		ParentType = (XamlType)info.GetValue("ParentType", typeof(XamlType));
	}

	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("DuplicateMember", DuplicateMember);
		info.AddValue("ParentType", ParentType);
		base.GetObjectData(info, context);
	}
}
