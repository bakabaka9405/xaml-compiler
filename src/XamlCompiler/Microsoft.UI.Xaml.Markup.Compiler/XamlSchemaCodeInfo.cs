using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xaml;
using System.Xaml.Schema;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlSchemaCodeInfo
{
	private class ProjectionDefinition
	{
		public string CppCXName;

		public string CppWinRTName;

		public bool StripSystemPrefix;

		public ProjectionDefinition(string cppCXName, string cppWinRTName, bool stripSystemPrefix = true)
		{
			CppCXName = cppCXName;
			CppWinRTName = cppWinRTName;
			StripSystemPrefix = stripSystemPrefix;
		}

		public ProjectionDefinition(string all)
			: this(all, all)
		{
		}
	}

	private static Dictionary<string, ProjectionDefinition> _winRtPrimitiveTypeList;

	private List<InternalTypeEntry> _typeTable;

	private List<InternalXamlUserTypeInfo> _userTypeInfo;

	private List<InternalXamlUserMemberInfo> _userMemberInfo;

	private bool typeInfoReflectionEnabled;

	private IReadOnlyCollection<TypeForCodeGen> _otherMetadataProviders;

	public static bool SetAirityOnGenericTypeNames = true;

	public IReadOnlyCollection<TypeForCodeGen> OtherMetadataProviders
	{
		get
		{
			if (_otherMetadataProviders == null)
			{
				_otherMetadataProviders = new List<TypeForCodeGen>();
			}
			return _otherMetadataProviders;
		}
		set
		{
			if (value == _otherMetadataProviders)
			{
				return;
			}
			_otherMetadataProviders = value;
			foreach (InternalTypeEntry item in _typeTable)
			{
				item.IsHandledByOtherProviders = IsTypeHandledByOtherProviders(item.UnderlyingType, _otherMetadataProviders);
			}
		}
	}

	public bool TypeInfoReflectionEnabled
	{
		get
		{
			return typeInfoReflectionEnabled;
		}
		set
		{
			typeInfoReflectionEnabled = value;
		}
	}

	public List<InternalTypeEntry> TypeTableFromAllAssemblies => _typeTable;

	public IReadOnlyList<InternalTypeEntry> TypeTable => _typeTable.Where((InternalTypeEntry t) => ShouldIncludeTypeInTypeTable(t)).ToList();

	public List<InternalXamlUserTypeInfo> UserTypeInfo => _userTypeInfo.Where((InternalXamlUserTypeInfo t) => ShouldIncludeTypeInTypeTable(t.TypeEntry)).ToList();

	public List<InternalXamlUserMemberInfo> UserMemberInfo => _userMemberInfo;

	private static IDictionary<string, ProjectionDefinition> WinRtPrimitiveTypesForProjection
	{
		get
		{
			if (_winRtPrimitiveTypeList == null)
			{
				_winRtPrimitiveTypeList = new Dictionary<string, ProjectionDefinition>();
				_winRtPrimitiveTypeList.Add("System.Byte", new ProjectionDefinition("default::uint8", "uint8_t"));
				_winRtPrimitiveTypeList.Add("System.UInt8", new ProjectionDefinition("default::uint8", "uint8_t"));
				_winRtPrimitiveTypeList.Add("System.SByte", new ProjectionDefinition("default::int8", "int8_t"));
				_winRtPrimitiveTypeList.Add("System.Int8", new ProjectionDefinition("default::int8", "int8_t"));
				_winRtPrimitiveTypeList.Add("System.Char", new ProjectionDefinition("default::char16", "wchar_t"));
				_winRtPrimitiveTypeList.Add("System.Char16", new ProjectionDefinition("default::char16", "wchar_t"));
				_winRtPrimitiveTypeList.Add("System.Single", new ProjectionDefinition("default::float32", "float"));
				_winRtPrimitiveTypeList.Add("System.Double", new ProjectionDefinition("default::float64", "double"));
				_winRtPrimitiveTypeList.Add("System.Int16", new ProjectionDefinition("default::int16", "int16_t"));
				_winRtPrimitiveTypeList.Add("System.Int32", new ProjectionDefinition("default::int32", "int32_t"));
				_winRtPrimitiveTypeList.Add("System.Int64", new ProjectionDefinition("default::int64", "int64_t"));
				_winRtPrimitiveTypeList.Add("System.UInt16", new ProjectionDefinition("default::uint16", "uint16_t"));
				_winRtPrimitiveTypeList.Add("System.UInt32", new ProjectionDefinition("default::uint32", "uint32_t"));
				_winRtPrimitiveTypeList.Add("System.UInt64", new ProjectionDefinition("default::uint64", "uint64_t"));
				_winRtPrimitiveTypeList.Add("System.Boolean", new ProjectionDefinition("Platform::Boolean", "bool"));
				_winRtPrimitiveTypeList.Add("System.String", new ProjectionDefinition("Platform::String", "::winrt::hstring"));
				_winRtPrimitiveTypeList.Add("System.Object", new ProjectionDefinition("Platform::Object", "::winrt::Windows::Foundation::IInspectable"));
				_winRtPrimitiveTypeList.Add("System.Guid", new ProjectionDefinition("Platform::Guid", "GUID"));
				_winRtPrimitiveTypeList.Add("Windows.Foundation.IReference`1", new ProjectionDefinition("Platform::IBox`1", "::winrt::Windows::Foundation::IReference`1"));
				_winRtPrimitiveTypeList.Add("System.TimeSpan", new ProjectionDefinition("Windows::Foundation::TimeSpan", "::winrt::Windows::Foundation::TimeSpan"));
				_winRtPrimitiveTypeList.Add("System.Type", new ProjectionDefinition("Platform::Type", null, stripSystemPrefix: false));
				_winRtPrimitiveTypeList.Add("System.Enum", new ProjectionDefinition("Platform::Enum", null, stripSystemPrefix: false));
				_winRtPrimitiveTypeList.Add("System.ValueType", new ProjectionDefinition("Platform::ValueType", null, stripSystemPrefix: false));
				_winRtPrimitiveTypeList.Add("System.UIntPtr", new ProjectionDefinition("Platform::UIntPtr", null, stripSystemPrefix: false));
				_winRtPrimitiveTypeList.Add("Windows.Foundation.Numerics.Vector2", new ProjectionDefinition("Windows::Foundation::Numerics::float2", "::winrt::Windows::Foundation::Numerics::float2"));
				_winRtPrimitiveTypeList.Add("Windows.Foundation.Numerics.Vector3", new ProjectionDefinition("Windows::Foundation::Numerics::float3", "::winrt::Windows::Foundation::Numerics::float3"));
				_winRtPrimitiveTypeList.Add("Windows.Foundation.Numerics.Vector4", new ProjectionDefinition("Windows::Foundation::Numerics::float4", "::winrt::Windows::Foundation::Numerics::float4"));
				_winRtPrimitiveTypeList.Add("Windows.Foundation.Numerics.Plane", new ProjectionDefinition("Windows::Foundation::Numerics::Plane", "::winrt::Windows::Foundation::Numerics::Plane"));
				_winRtPrimitiveTypeList.Add("Windows.Foundation.Numerics.Quaternion", new ProjectionDefinition("Windows::Foundation::Numerics::quaternion", "::winrt::Windows::Foundation::Numerics::quaternion"));
				_winRtPrimitiveTypeList.Add("Windows.Foundation.Numerics.Matrix3x2", new ProjectionDefinition("Windows::Foundation::Numerics::float3x2", "::winrt::Windows::Foundation::Numerics::float3x2"));
				_winRtPrimitiveTypeList.Add("Windows.Foundation.Numerics.Matrix4x4", new ProjectionDefinition("Windows::Foundation::Numerics::float4x4", "::winrt::Windows::Foundation::Numerics::float4x4"));
			}
			return _winRtPrimitiveTypeList;
		}
	}

	public XamlSchemaCodeInfo()
	{
		_typeTable = new List<InternalTypeEntry>();
		_userTypeInfo = new List<InternalXamlUserTypeInfo>();
		_userMemberInfo = new List<InternalXamlUserMemberInfo>();
		typeInfoReflectionEnabled = false;
	}

	private bool ShouldIncludeTypeInTypeTable(InternalTypeEntry type)
	{
		if (!type.IsHandledByOtherProviders)
		{
			return true;
		}
		if (type.UserTypeInfo != null && type.UserTypeInfo.IsReturnTypeStub)
		{
			return true;
		}
		return false;
	}

	public bool TryFindType(string systemName, out InternalTypeEntry type)
	{
		foreach (InternalTypeEntry item in _typeTable)
		{
			if (systemName == item.SystemName)
			{
				type = item;
				return true;
			}
		}
		type = null;
		return false;
	}

	public InternalTypeEntry AddTypeAndProperties(XamlType xamlType)
	{
		InternalTypeEntry result = AddType(xamlType);
		AddAllCodeGenProperties(xamlType);
		return result;
	}

	public InternalTypeEntry AddReturnTypeStub(XamlType xamlType)
	{
		return AddType(xamlType, isReturnTypeStub: true);
	}

	public InternalTypeEntry AddType(XamlType xamlType, bool isReturnTypeStub = false)
	{
		if (xamlType == null)
		{
			throw new ArgumentNullException("xamlType");
		}
		if (TryFindType(xamlType.UnderlyingType.FullName, out var type))
		{
			if (type.UserTypeInfo != null && !isReturnTypeStub)
			{
				type.UserTypeInfo.IsReturnTypeStub = false;
			}
			return type;
		}
		DirectUIXamlType directUIXamlType = xamlType as DirectUIXamlType;
		if (directUIXamlType.IsCodeGenType)
		{
			if (xamlType.UnderlyingType.IsEnum)
			{
				isReturnTypeStub = false;
			}
			return AddNewUserTypeInfo(xamlType, isReturnTypeStub);
		}
		return AddNewTypeEntry(xamlType);
	}

	public InternalTypeEntry AddBindableType(XamlType type)
	{
		InternalTypeEntry internalTypeEntry = AddTypeAndProperties(type);
		if (internalTypeEntry.IsSystemType)
		{
			return internalTypeEntry;
		}
		internalTypeEntry.UserTypeInfo.IsBindable = true;
		return internalTypeEntry;
	}

	private void AddAllCodeGenProperties(XamlType type)
	{
		if (type.UnderlyingType.IsEnum)
		{
			return;
		}
		foreach (XamlMember allMember in type.GetAllMembers())
		{
			AddCodeGenProperty(allMember);
		}
		foreach (XamlMember allAttachableMember in type.GetAllAttachableMembers())
		{
			AddCodeGenProperty(allAttachableMember);
		}
	}

	public bool TryFindIndexOfMember(string name, InternalTypeEntry declaringType, out int idx)
	{
		for (int i = 0; i < _userMemberInfo.Count; i++)
		{
			InternalXamlUserMemberInfo internalXamlUserMemberInfo = UserMemberInfo[i];
			if (internalXamlUserMemberInfo.Name == name && internalXamlUserMemberInfo.DeclaringType == declaringType)
			{
				idx = i;
				return true;
			}
		}
		idx = -1;
		return false;
	}

	private bool TryFindMember(string name, InternalTypeEntry declaringType, out InternalXamlUserMemberInfo member)
	{
		if (TryFindIndexOfMember(name, declaringType, out var idx))
		{
			member = UserMemberInfo[idx];
			return true;
		}
		member = null;
		return false;
	}

	private void AddCodeGenProperty(XamlMember xamlMember)
	{
		if (xamlMember.IsEvent)
		{
			return;
		}
		DirectUIXamlType directUIXamlType = xamlMember.DeclaringType as DirectUIXamlType;
		if (directUIXamlType.IsCodeGenType)
		{
			DirectUIXamlMember directUIXamlMember = xamlMember as DirectUIXamlMember;
			if (!(directUIXamlMember != null) || !directUIXamlMember.IsIndexer)
			{
				InternalTypeEntry usingType = AddType(directUIXamlType);
				AddMember(usingType, xamlMember);
			}
		}
	}

	public InternalXamlUserMemberInfo AddMember(InternalTypeEntry usingType, XamlMember xamlMember, bool declaringTypeAsStub = false)
	{
		if (xamlMember.IsDirective)
		{
			return null;
		}
		InternalTypeEntry internalTypeEntry = AddType(xamlMember.DeclaringType, declaringTypeAsStub);
		if (internalTypeEntry == null)
		{
			throw new InvalidOperationException("declaring type is null on a non-directive property");
		}
		if (internalTypeEntry.IsSystemType)
		{
			return null;
		}
		DirectUIXamlMember directUIXamlMember = (DirectUIXamlMember)xamlMember;
		if (directUIXamlMember.IsEvent || directUIXamlMember.IsHardDeprecated)
		{
			return null;
		}
		DirectUIXamlType directUIXamlType = (DirectUIXamlType)xamlMember.Type;
		if (directUIXamlType.IsHardDeprecated)
		{
			return null;
		}
		if (TryFindMember(xamlMember.Name, internalTypeEntry, out var member))
		{
			return member;
		}
		member = new InternalXamlUserMemberInfo();
		member.Name = xamlMember.Name;
		member.DeclaringType = internalTypeEntry;
		UserMemberInfo.Add(member);
		internalTypeEntry.UserTypeInfo.AddMember(member, internalTypeEntry, this);
		member.Init(xamlMember, this);
		return member;
	}

	private InternalTypeEntry AddNewTypeEntry(XamlType xamlType)
	{
		InternalTypeEntry internalTypeEntry = new InternalTypeEntry(xamlType);
		internalTypeEntry.TypeIndex = _typeTable.Count;
		internalTypeEntry.IsHandledByOtherProviders = IsTypeHandledByOtherProviders(xamlType.UnderlyingType, OtherMetadataProviders);
		_typeTable.Add(internalTypeEntry);
		return internalTypeEntry;
	}

	private InternalTypeEntry AddNewUserTypeInfo(XamlType xamlType, bool isReturnTypeStub)
	{
		InternalTypeEntry internalTypeEntry = AddNewTypeEntry(xamlType);
		internalTypeEntry.UserTypeInfo = new InternalXamlUserTypeInfo(internalTypeEntry, this);
		internalTypeEntry.UserTypeInfo.IsReturnTypeStub = isReturnTypeStub;
		_userTypeInfo.Add(internalTypeEntry.UserTypeInfo);
		internalTypeEntry.UserTypeInfo.Init(xamlType);
		return internalTypeEntry;
	}

	internal static string GetFullGenericNestedName(Type type, bool setAirity)
	{
		return GetFullGenericNestedName(type, "WinRT", globalized: false, setAirity);
	}

	public static string GetFullGenericNestedName(Type type, string programmingLanguage, bool globalized)
	{
		return GetFullGenericNestedName(type, programmingLanguage, globalized, setAirity: true);
	}

	public static bool GetGlobalizedFullNameForCppRefType(Type type, out string fullName)
	{
		bool flag = false;
		fullName = null;
		if (!type.IsByRef)
		{
			return false;
		}
		string text = type.FullName.Substring(0, type.FullName.Length - 1);
		if (text.EndsWith("[]"))
		{
			flag = true;
			text = text.Substring(0, text.Length - 2);
		}
		text = ProjectionNameTranslation(text, "C++");
		fullName = (flag ? ("::Platform::Array<" + text + ">") : ("::" + text));
		return true;
	}

	public static string GetFullGenericNestedName(Type type, string programmingLanguage, bool globalized, bool setAirity)
	{
		type = GetArrayElementType(type, out var suffix, programmingLanguage);
		string text = FixNestedTypeName(type);
		string text2 = "";
		string text3 = ProjectionNameTranslation(text, programmingLanguage);
		if (globalized)
		{
			switch (programmingLanguage)
			{
			case "WinRT":
				text2 = "";
				break;
			case "C#":
				text2 = "global::";
				break;
			case "VB":
				text2 = "Global.";
				break;
			case "C++":
				text2 = "::";
				break;
			case "CppWinRT":
				if (!WinRtPrimitiveTypesForProjection.ContainsKey(text))
				{
					text2 = "::winrt::";
				}
				break;
			default:
				throw new ArgumentException("programmingLanguage");
			}
		}
		if (!type.IsGenericType)
		{
			return text2 + text3 + suffix;
		}
		string text4 = "<";
		string text5 = ">";
		string text6 = string.Empty;
		string text7 = string.Empty;
		switch (programmingLanguage)
		{
		case "WinRT":
			text6 = "`{0}";
			break;
		case "VB":
			text4 = "(Of ";
			text5 = ")";
			break;
		case "C++":
			text7 = "^";
			break;
		default:
			throw new ArgumentException("programmingLanguage");
		case "C#":
		case "CppWinRT":
			break;
		}
		if (!setAirity)
		{
			text6 = string.Empty;
		}
		Type[] genericArguments = type.GetGenericArguments();
		int num = 0;
		string[] array = text3.Split('`');
		string text8 = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			string namePart = array[i];
			string remainder;
			int num2 = CountTypeArgs(namePart, out remainder);
			if (!string.IsNullOrEmpty(text6))
			{
				text8 += string.Format(CultureInfo.InvariantCulture, text6, num2);
			}
			text8 += text4;
			for (int j = 0; j < num2; j++)
			{
				Type type2 = genericArguments[num + j];
				string text9 = GetFullGenericNestedName(type2, programmingLanguage, globalized: true);
				if (!type2.IsValueType)
				{
					text9 += text7;
				}
				text8 += text9;
				if (j + 1 < num2)
				{
					text8 += ", ";
				}
			}
			text8 += text5;
			text8 += remainder;
			num += num2;
		}
		return text2 + text8 + suffix;
	}

	internal static string FixNestedTypeName(Type type)
	{
		string empty = string.Empty;
		if (type.IsGenericType)
		{
			type = type.GetGenericTypeDefinition();
		}
		if (type.IsNested)
		{
			string text = FixNestedTypeName(type.DeclaringType);
			return text + "." + type.Name;
		}
		return type.FullName;
	}

	internal static Type GetArrayElementType(Type type, out string suffix, string programmingLanguage)
	{
		suffix = string.Empty;
		if (!type.IsArray)
		{
			return type;
		}
		suffix += ((programmingLanguage == "VB") ? "(" : "[");
		for (int i = 1; i < type.GetArrayRank(); i++)
		{
			suffix += ",";
		}
		suffix += ((programmingLanguage == "VB") ? ")" : "]");
		Type elementType = type.GetElementType();
		string suffix2;
		Type arrayElementType = GetArrayElementType(elementType, out suffix2, programmingLanguage);
		suffix += suffix2;
		return arrayElementType;
	}

	public static XamlTypeName GetXamlTypeNameFromFullName(string typeFullName)
	{
		ClassName className = new ClassName(typeFullName);
		return new XamlTypeName("using:" + className.Namespace, className.ShortName);
	}

	private static int CountTypeArgs(string namePart, out string remainder)
	{
		char[] separator = new char[2] { '.', ':' };
		string[] array = namePart.Split(separator, 2);
		if (array.Length == 1)
		{
			remainder = string.Empty;
		}
		else if (array[1][0] == ':')
		{
			remainder = ":" + array[1];
		}
		else
		{
			remainder = "." + array[1];
		}
		return int.Parse(array[0]);
	}

	public static bool IsProjectedPrimitiveCppType(string typeFullName)
	{
		return WinRtPrimitiveTypesForProjection.ContainsKey(typeFullName);
	}

	private static string ProjectionNameTranslation(string simpleTypeName, string programmingLanguage)
	{
		WinRtPrimitiveTypesForProjection.TryGetValue(simpleTypeName, out var value);
		switch (programmingLanguage)
		{
		case "WinRT":
			if (value != null && value.StripSystemPrefix && simpleTypeName.StartsWith("System."))
			{
				return simpleTypeName.Remove(0, "System.".Length);
			}
			return simpleTypeName;
		case "C++":
			if (value == null)
			{
				return KnownStrings.Colonize(simpleTypeName);
			}
			return value.CppCXName;
		case "CppWinRT":
			if (value == null)
			{
				return KnownStrings.Colonize(simpleTypeName);
			}
			return value.CppWinRTName;
		default:
			return simpleTypeName;
		}
	}

	private static bool IsTypeHandledByOtherProviders(Type type, IEnumerable<TypeForCodeGen> otherProviders)
	{
		if (otherProviders != null)
		{
			foreach (TypeForCodeGen otherProvider in otherProviders)
			{
				if (otherProvider.UnderlyingType.Assembly.FullName == type.Assembly.FullName)
				{
					return otherProvider.UnderlyingType.HasFullXamlMetadataProviderAttribute();
				}
			}
		}
		return false;
	}
}
