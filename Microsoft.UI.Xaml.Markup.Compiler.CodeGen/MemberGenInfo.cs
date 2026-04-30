namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class MemberGenInfo
{
	private TypeGenInfo typeInfo;

	private InternalXamlUserMemberInfo memberInfo;

	public bool IsAttachable => memberInfo.IsAttachable;

	public bool IsDeprecated => memberInfo.IsDeprecated;

	public bool IsDependencyProperty => memberInfo.IsDependencyProperty;

	public bool IsValueType => memberInfo.IsValueType;

	public bool IsEnum => memberInfo.IsEnum;

	public string Name => memberInfo.Name;

	public bool HasPublicGetter => memberInfo.HasPublicGetter;

	public bool HasPublicSetter => memberInfo.HasPublicSetter;

	public InternalTypeEntry DeclaringType => memberInfo.DeclaringType;

	public InternalTypeEntry TargetType => memberInfo.TargetType;

	public InternalTypeEntry Type => memberInfo.Type;

	public bool HasGetAttachableMember
	{
		get
		{
			if (HasPublicGetter)
			{
				return IsAttachable;
			}
			return false;
		}
	}

	public LanguageSpecificString GetAttachableMemberName
	{
		get
		{
			if (!typeInfo.IncrementalTypeInfo)
			{
				return new LanguageSpecificString(() => "GetAttachableMember_" + Name + "<" + DeclaringType.FullName.CppCXName() + ", " + TargetType.FullName.CppCXName() + ">", () => "GetAttachableMember_" + Name + "<" + DeclaringType.FullName.CppWinRTName() + ", " + TargetType.FullName.CppWinRTName() + ">", null, null);
			}
			return new LanguageSpecificString(() => "GetAttachableMember_" + Name.GetMemberFriendlyName() + "_" + DeclaringType.StandardName.GetMemberFriendlyName() + "_" + TargetType.StandardName.GetMemberFriendlyName());
		}
	}

	public bool HasGetValueTypeMember
	{
		get
		{
			if (HasPublicGetter && !IsAttachable)
			{
				return IsValueType;
			}
			return false;
		}
	}

	public LanguageSpecificString GetValueTypeMemberName
	{
		get
		{
			if (!typeInfo.IncrementalTypeInfo)
			{
				return new LanguageSpecificString(() => "GetValueTypeMember_" + Name + "<" + DeclaringType.FullName.CppCXName() + ", " + Type.FullName.CppCXName() + ">", () => "GetValueTypeMember_" + Name + "<" + DeclaringType.FullName.CppWinRTName() + ", " + Type.FullName.CppWinRTName() + ">", null, null);
			}
			return new LanguageSpecificString(() => "GetValueTypeMember_" + Name.GetMemberFriendlyName() + "_" + DeclaringType.StandardName.GetMemberFriendlyName());
		}
	}

	public bool HasGetReferenceTypeMember
	{
		get
		{
			if (HasPublicGetter && !IsAttachable)
			{
				return !IsValueType;
			}
			return false;
		}
	}

	public LanguageSpecificString GetReferenceTypeMemberName
	{
		get
		{
			string memberName;
			if (memberInfo != null && memberInfo.IsString)
			{
				memberName = "StringMember";
			}
			else
			{
				memberName = "Member";
			}
			if (!typeInfo.IncrementalTypeInfo)
			{
				return new LanguageSpecificString(() => "GetReferenceTypeMember_" + Name + "<" + DeclaringType.FullName.CppCXName() + ">", () => "GetReferenceType" + memberName + "_" + Name + "<" + DeclaringType.FullName.CppWinRTName() + ">", null, null);
			}
			return new LanguageSpecificString(() => "GetReferenceTypeMember_" + Name.GetMemberFriendlyName() + "_" + DeclaringType.StandardName.GetMemberFriendlyName());
		}
	}

	public bool HasSetAttachableMember
	{
		get
		{
			if (HasPublicSetter)
			{
				return IsAttachable;
			}
			return false;
		}
	}

	public LanguageSpecificString SetAttachableMemberName
	{
		get
		{
			string hatNeeded = (IsValueType ? string.Empty : "^");
			if (!typeInfo.IncrementalTypeInfo)
			{
				return new LanguageSpecificString(() => "SetAttachableMember_" + Name + "<" + DeclaringType.FullName.CppCXName() + ", " + TargetType.FullName.CppCXName() + ", " + Type.FullName.CppCXName() + hatNeeded + ">", () => "SetAttachableMember_" + Name + "<" + DeclaringType.FullName.CppWinRTName() + ", " + TargetType.FullName.CppWinRTName() + ", " + Type.FullName.CppWinRTName() + ">", null, null);
			}
			return new LanguageSpecificString(() => "SetAttachableMember_" + Name.GetMemberFriendlyName() + "_" + DeclaringType.StandardName.GetMemberFriendlyName() + "_" + TargetType.StandardName.GetMemberFriendlyName() + "_" + Type.StandardName.GetMemberFriendlyName());
		}
	}

	public bool HasSetValueTypeMember
	{
		get
		{
			if (HasPublicSetter && !IsAttachable && IsValueType)
			{
				return !IsEnum;
			}
			return false;
		}
	}

	public LanguageSpecificString SetValueTypeMemberName
	{
		get
		{
			if (!typeInfo.IncrementalTypeInfo)
			{
				return new LanguageSpecificString(() => "SetValueTypeMember_" + Name + "<" + DeclaringType.FullName.CppCXName() + ", " + Type.FullName.CppCXName() + ">", () => "SetValueTypeMember_" + Name + "<" + DeclaringType.FullName.CppWinRTName() + ", " + Type.FullName.CppWinRTName() + ">", null, null);
			}
			return new LanguageSpecificString(() => "SetValueTypeMember_" + Name.GetMemberFriendlyName() + "_" + DeclaringType.StandardName.GetMemberFriendlyName());
		}
	}

	public bool HasSetEnumMember
	{
		get
		{
			if (HasPublicSetter && !IsAttachable && IsValueType)
			{
				return IsEnum;
			}
			return false;
		}
	}

	public LanguageSpecificString SetEnumMemberName
	{
		get
		{
			if (!typeInfo.IncrementalTypeInfo)
			{
				return new LanguageSpecificString(() => "SetEnumMember_" + Name + "<" + DeclaringType.FullName.CppCXName() + ", " + Type.FullName.CppCXName() + ">", () => "SetEnumMember_" + Name + "<" + DeclaringType.FullName.CppWinRTName() + ", " + Type.FullName.CppWinRTName() + ">", null, null);
			}
			return new LanguageSpecificString(() => "SetEnumMember_" + Name.GetMemberFriendlyName() + "_" + DeclaringType.StandardName.GetMemberFriendlyName() + "_" + Type.StandardName.GetMemberFriendlyName());
		}
	}

	public bool HasSetReferenceTypeMember
	{
		get
		{
			if (HasPublicSetter && !IsAttachable)
			{
				return !IsValueType;
			}
			return false;
		}
	}

	public LanguageSpecificString SetReferenceTypeMemberName
	{
		get
		{
			string memberName;
			if (memberInfo != null && memberInfo.IsString)
			{
				memberName = "StringMember";
			}
			else
			{
				memberName = "Member";
			}
			if (!typeInfo.IncrementalTypeInfo)
			{
				return new LanguageSpecificString(() => "SetReferenceTypeMember_" + Name + "<" + DeclaringType.FullName.CppCXName() + ", " + Type.FullName.CppCXName() + ">", () => "SetReferenceType" + memberName + "_" + Name + "<" + DeclaringType.FullName.CppWinRTName() + ", " + Type.FullName.CppWinRTName() + ">", null, null);
			}
			return new LanguageSpecificString(() => "SetReferenceTypeMember_" + Name.GetMemberFriendlyName() + "_" + DeclaringType.StandardName.GetMemberFriendlyName());
		}
	}

	public MemberGenInfo(TypeGenInfo typeInfo, InternalXamlUserMemberInfo memberInfo)
	{
		this.typeInfo = typeInfo;
		this.memberInfo = memberInfo;
	}
}
