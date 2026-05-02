using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class TypeInfoDefinition(XamlProjectInfo projectInfo, XamlSchemaCodeInfo schemaInfo) : Definition(projectInfo, schemaInfo)
{
	private List<MemberGenInfo> _memberInfos = new List<MemberGenInfo>();

	private Dictionary<string, int> _typeInfoIndexes;

	private uint[] _typeInfoLookup;

	private List<EnumGenInfo> _enumValues = new List<EnumGenInfo>();

	private List<string> _neededCppWinRTProjectionHeaderFiles;

	internal ClassName AppXamlInfo { get; set; }

	public bool GenerateTypeInfo
	{
		get
		{
			if (base.SchemaInfo.UserTypeInfo.Count == 0)
			{
				return base.ProjectInfo.EnableTypeInfoReflection;
			}
			return true;
		}
	}

	public IEnumerable<string> AllLocalXamlHeaderFiles
	{
		get
		{
			foreach (IFileItem additionalXamlTypeInfoInclude in base.ProjectInfo.AdditionalXamlTypeInfoIncludes)
			{
				yield return additionalXamlTypeInfoInclude.ItemSpec;
			}
			foreach (string value in base.ProjectInfo.ClassToHeaderFileMap.Values)
			{
				yield return value;
			}
		}
	}

	public IEnumerable<string> AllLocalHppGeneratedFiles
	{
		get
		{
			foreach (string value in base.ProjectInfo.ClassToHeaderFileMap.Values)
			{
				string text = value;
				string extension = Path.GetExtension(text);
				if (!string.IsNullOrEmpty(extension))
				{
					text = text.Remove(text.Length - extension.Length);
				}
				if (text.EndsWith(".xaml"))
				{
					text = text.Remove(text.Length - ".xaml".Length);
				}
				yield return text + ".g.hpp";
			}
		}
	}

	public string AppMetadataProviderNamespace => AppXamlInfo?.Namespace;

	public Dictionary<string, int> TypeInfoIndexes
	{
		get
		{
			if (_typeInfoIndexes == null)
			{
				int num = 0;
				_typeInfoIndexes = new Dictionary<string, int>();
				foreach (TypeGenInfo typeInfo in base.TypeInfos)
				{
					_typeInfoIndexes.Add(typeInfo.StandardName, num++);
				}
			}
			return _typeInfoIndexes;
		}
	}

	public IEnumerable<uint> TypeInfoLookup
	{
		get
		{
			if (_typeInfoLookup == null)
			{
				int num = base.TypeInfos.Min((TypeGenInfo x) => x.StandardName.Length);
				int num2 = base.TypeInfos.Max((TypeGenInfo x) => x.StandardName.Length) + 1;
				_typeInfoLookup = new uint[num2 + 1];
				uint num3 = 0u;
				foreach (TypeGenInfo typeInfo in base.TypeInfos)
				{
					int length = typeInfo.StandardName.Length;
					if (_typeInfoLookup[length] == 0 && length != num)
					{
						_typeInfoLookup[length] = num3;
					}
					num3++;
				}
				_typeInfoLookup[num2] = num3;
				for (int num4 = num2 - 1; num4 > num; num4--)
				{
					if (_typeInfoLookup[num4] == 0)
					{
						_typeInfoLookup[num4] = num3;
					}
					else
					{
						num3 = _typeInfoLookup[num4];
					}
				}
			}
			return _typeInfoLookup;
		}
	}

	public IEnumerable<MemberGenInfo> MemberInfos => _memberInfos;

	public IEnumerable<EnumGenInfo> EnumValues => _enumValues;

	public IEnumerable<string> AttachableMemberGetterUniqueNames => (from x in base.SchemaInfo.UserMemberInfo
		where x.HasPublicGetter && x.IsAttachable
		select x.Name).Distinct();

	public IEnumerable<string> ValueTypeMemberGetterUniqueNames => (from x in base.SchemaInfo.UserMemberInfo
		where x.HasPublicGetter && !x.IsAttachable && x.IsValueType
		select x.Name).Distinct();

	public IEnumerable<string> StringGetterUniqueNames => (from x in base.SchemaInfo.UserMemberInfo
		where x.HasPublicGetter && !x.IsAttachable && !x.IsValueType && x.IsString
		select x.Name).Distinct();

	public IEnumerable<string> ReferenceTypeMemberGetterUniqueNamesNoStrings => (from x in base.SchemaInfo.UserMemberInfo
		where x.HasPublicGetter && !x.IsAttachable && !x.IsValueType && !x.IsString
		select x.Name).Distinct();

	public IEnumerable<string> ReferenceTypeMemberGetterUniqueNames => (from x in base.SchemaInfo.UserMemberInfo
		where x.HasPublicGetter && !x.IsAttachable && !x.IsValueType
		select x.Name).Distinct();

	public IEnumerable<string> AttachableMemberSetterUniqueNames => (from x in base.SchemaInfo.UserMemberInfo
		where x.HasPublicSetter && x.IsAttachable
		select x.Name).Distinct();

	public IEnumerable<string> EnumTypeMemberSetterUniqueNames => (from x in base.SchemaInfo.UserMemberInfo
		where x.HasPublicSetter && !x.IsAttachable && x.IsValueType && x.IsEnum
		select x.Name).Distinct();

	public IEnumerable<string> ValueTypeMemberSetterUniqueNames => (from x in base.SchemaInfo.UserMemberInfo
		where x.HasPublicSetter && !x.IsAttachable && x.IsValueType && !x.IsEnum
		select x.Name).Distinct();

	public IEnumerable<string> StringSetterUniqueNames => (from x in base.SchemaInfo.UserMemberInfo
		where x.HasPublicSetter && !x.IsAttachable && !x.IsValueType && x.IsString
		select x.Name).Distinct();

	public IEnumerable<string> ReferenceTypeMemberSetterUniqueNames => (from x in base.SchemaInfo.UserMemberInfo
		where x.HasPublicSetter && !x.IsAttachable && !x.IsValueType
		select x.Name).Distinct();

	public IEnumerable<string> ReferenceTypeMemberSetterUniqueNamesNoStrings => (from x in base.SchemaInfo.UserMemberInfo
		where x.HasPublicSetter && !x.IsAttachable && !x.IsValueType && !x.IsString
		select x.Name).Distinct();

	public IEnumerable<string> NeededCppWinRTProjectionHeaderFiles
	{
		get
		{
			if (_neededCppWinRTProjectionHeaderFiles == null)
			{
				_neededCppWinRTProjectionHeaderFiles = LookupNeededCppWinRTProjectionHeaderFiles();
			}
			return _neededCppWinRTProjectionHeaderFiles;
		}
	}

	public void TrackTypeMembers(TypeGenInfo entry, out int startIndex)
	{
		startIndex = _memberInfos.Count;
		foreach (MemberGenInfo member in entry.Members)
		{
			_memberInfos.Add(member);
		}
	}

	public void TrackTypeEnumValues(TypeGenInfo entry, out int startIndex)
	{
		startIndex = _enumValues.Count;
		foreach (string enumValue in entry.EnumValues)
		{
			_enumValues.Add(new EnumGenInfo(entry, enumValue));
		}
	}

	private List<string> LookupNeededCppWinRTProjectionHeaderFiles()
	{
		HashSet<string> headers = new HashSet<string>();
		foreach (TypeGenInfo typeInfo in base.TypeInfos)
		{
			if (typeInfo.HasActivator || typeInfo.IsCollection || typeInfo.IsDictionary || typeInfo.HasEnumValues)
			{
				addCppWinRTHeaderForTypeIfNecessary(typeInfo.TypeEntry?.UnderlyingType);
			}
		}
		return headers.OrderBy((string value) => value.EndsWith(".h") ? value.Substring(0, value.Length - 2) : value).ToList();
		void addCppWinRTHeaderForTypeIfNecessary(Type type)
		{
			if (type != null && !XamlSchemaCodeInfo.IsProjectedPrimitiveCppType(type.FullName))
			{
				headers.Add("winrt/" + type.Namespace + ".h");
			}
		}
	}

	public string GetterName(int i)
	{
		InternalXamlUserMemberInfo internalXamlUserMemberInfo = base.SchemaInfo.UserMemberInfo[i];
		return "get_" + i + "_" + internalXamlUserMemberInfo.DeclaringType.Name + "_" + internalXamlUserMemberInfo.Name;
	}

	public string SetterName(int i)
	{
		InternalXamlUserMemberInfo internalXamlUserMemberInfo = base.SchemaInfo.UserMemberInfo[i];
		return "set_" + i + "_" + internalXamlUserMemberInfo.DeclaringType.Name + "_" + internalXamlUserMemberInfo.Name;
	}

	public string ActivatorName(InternalXamlUserTypeInfo entry)
	{
		return "Activate_" + entry.TypeIndex + "_" + entry.Name;
	}

	public string StaticInitializerName(InternalXamlUserTypeInfo entry)
	{
		return "StaticInitializer_" + entry.TypeIndex + "_" + entry.Name;
	}

	public string VectorAddName(InternalXamlUserTypeInfo entry)
	{
		if (entry.IsCollection)
		{
			return "VectorAdd_" + entry.TypeIndex + "_" + entry.Name;
		}
		return string.Empty;
	}

	public string MapAddName(InternalXamlUserTypeInfo entry)
	{
		if (entry.IsDictionary)
		{
			return "MapAdd_" + entry.TypeIndex + "_" + entry.Name;
		}
		return string.Empty;
	}
}
