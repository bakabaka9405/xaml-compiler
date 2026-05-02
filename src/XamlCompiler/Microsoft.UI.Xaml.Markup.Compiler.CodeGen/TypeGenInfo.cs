using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class TypeGenInfo
{
	public InternalTypeEntry TypeEntry { get; protected set; }

	public bool IncrementalTypeInfo { get; }

	public InternalXamlUserTypeInfo UserTypeInfo { protected get; set; }

	public bool IsSystemType => TypeEntry.IsSystemType;

	public string StandardName => TypeEntry.StandardName;

	public string BaseTypeStandardName
	{
		get
		{
			if (UserTypeInfo == null || UserTypeInfo.BaseType == null)
			{
				return "";
			}
			return UserTypeInfo.BaseType.StandardName;
		}
	}

	public string BoxedTypeStandardName
	{
		get
		{
			if (UserTypeInfo == null || UserTypeInfo.BoxedType == null)
			{
				return "";
			}
			return UserTypeInfo.BoxedType.StandardName;
		}
	}

	public bool IsLocalType
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return false;
			}
			return UserTypeInfo.IsLocalType;
		}
	}

	public bool IsBindable
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return false;
			}
			return UserTypeInfo.IsBindable;
		}
	}

	public bool IsMarkupExtension
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return false;
			}
			return UserTypeInfo.IsMarkupExtension;
		}
	}

	public bool HasCreateFromStringMethod
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return false;
			}
			return UserTypeInfo.HasCreateFromStringMethod;
		}
	}

	public CreateFromStringMethod CreateFromStringMethod => UserTypeInfo?.CreateFromStringMethod;

	public bool IsReturnTypeStub
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return false;
			}
			return UserTypeInfo.IsReturnTypeStub;
		}
	}

	public bool IsConstructible
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return false;
			}
			return UserTypeInfo.IsConstructible;
		}
	}

	public bool IsCollection
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return false;
			}
			return UserTypeInfo.IsCollection;
		}
	}

	public bool IsDictionary
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return false;
			}
			return UserTypeInfo.IsDictionary;
		}
	}

	public bool IsDeprecated
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return false;
			}
			return UserTypeInfo.IsDeprecated;
		}
	}

	public bool HasEnumValues
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return false;
			}
			return UserTypeInfo.HasEnumValues;
		}
	}

	public bool HasMembers
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return false;
			}
			return UserTypeInfo.HasMembers;
		}
	}

	public LanguageSpecificString FullName
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return TypeEntry.FullName;
			}
			return UserTypeInfo.FullName;
		}
	}

	public string ContentPropertyName
	{
		get
		{
			if (UserTypeInfo == null || UserTypeInfo.ContentProperty == null)
			{
				return "";
			}
			return $"{UserTypeInfo.ContentProperty.DeclaringType.StandardName}.{UserTypeInfo.ContentProperty.Name}";
		}
	}

	public InternalTypeEntry KeyType => UserTypeInfo.KeyType;

	public InternalTypeEntry ItemType => UserTypeInfo.ItemType;

	public IEnumerable<MemberGenInfo> Members
	{
		get
		{
			List<InternalXamlUserMemberInfo> list = ((UserTypeInfo != null) ? UserTypeInfo.Members : new List<InternalXamlUserMemberInfo>());
			foreach (InternalXamlUserMemberInfo item in list)
			{
				yield return new MemberGenInfo(this, item);
			}
		}
	}

	public List<string> EnumValues
	{
		get
		{
			if (UserTypeInfo == null)
			{
				return new List<string>();
			}
			return UserTypeInfo.EnumValues;
		}
	}

	public bool HasActivator
	{
		get
		{
			if (IsConstructible)
			{
				return !IsReturnTypeStub;
			}
			return false;
		}
	}

	public LanguageSpecificString ActivatorName
	{
		get
		{
			if (!IncrementalTypeInfo)
			{
				return new LanguageSpecificString(() => "ActivateType<" + FullName.CppCXName() + ">", () => (!IsLocalType) ? ("ActivateType<" + FullName.CppWinRTName() + ">") : ("ActivateLocalType<" + FullName.CppWinRTName().ToLocalCppWinRTTypeName() + ">"), null, null);
			}
			return new LanguageSpecificString(() => "ActivateType_" + StandardName.GetMemberFriendlyName());
		}
	}

	public LanguageSpecificString CollectionAddName
	{
		get
		{
			if (!IncrementalTypeInfo)
			{
				return new LanguageSpecificString(() => "CollectionAdd<" + FullName.CppCXName() + ", " + ItemType.FullName.CppCXName() + ItemType.RefHat + ">", () => "CollectionAdd<" + FullName.CppWinRTName() + ", " + ItemType.FullName.CppWinRTName() + ">", null, null);
			}
			return new LanguageSpecificString(() => "CollectionAdd_" + StandardName.GetMemberFriendlyName() + "_" + ItemType.StandardName.GetMemberFriendlyName());
		}
	}

	public LanguageSpecificString DictionaryAddName
	{
		get
		{
			if (!IncrementalTypeInfo)
			{
				return new LanguageSpecificString(() => "DictionaryAdd<" + FullName.CppCXName() + ", " + KeyType.FullName.CppCXName() + KeyType.RefHat + ", " + ItemType.FullName.CppCXName() + ItemType.RefHat + ">", () => "DictionaryAdd<" + FullName.CppWinRTName() + ", " + KeyType.FullName.CppWinRTName() + ", " + ItemType.FullName.CppWinRTName() + ">", null, null);
			}
			return new LanguageSpecificString(() => "DictionaryAdd_" + StandardName.GetMemberFriendlyName() + "_" + KeyType.StandardName.GetMemberFriendlyName() + "_" + ItemType.StandardName.GetMemberFriendlyName());
		}
	}

	public LanguageSpecificString FromStringConverterName
	{
		get
		{
			if (!IncrementalTypeInfo)
			{
				return new LanguageSpecificString(() => "FromStringConverter<" + FullName.CppCXName() + ">", () => "FromStringConverter<" + FullName.CppWinRTName() + ">", null, null);
			}
			return new LanguageSpecificString(() => "FromStringConverter_" + StandardName.GetMemberFriendlyName());
		}
	}

	public TypeGenInfo(InternalTypeEntry typeEntry, bool incrementalTypeInfo)
	{
		IncrementalTypeInfo = incrementalTypeInfo;
		TypeEntry = typeEntry;
		UserTypeInfo = null;
	}
}
