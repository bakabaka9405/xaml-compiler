using System.CodeDom.Compiler;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

[GeneratedCode("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
internal class CppWinRT_TemplatedMetadataDelegates : CppWinRT_CodeGenerator<TypeInfoDefinition>
{
	public override string TransformText()
	{
		Write("template <typename T>\r\n");
		Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
		Write("::IInspectable ActivateType()\r\n{\r\n    return T();\r\n}\r\n\r\ntemplate <typename T>\r\n");
		Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
		Write("::IInspectable ActivateLocalType()\r\n{\r\n    return ::winrt::make<T>();\r\n}\r\n\r\ntemplate<typename TInstance, typename TItem>\r\nvoid CollectionAdd(\r\n    ");
		Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
		Write("::IInspectable const& instance, \r\n    ");
		Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
		Write("::IInspectable const& item)\r\n{\r\n    instance.as<TInstance>().Append(::winrt::unbox_value<TItem>(item));\r\n}\r\n\r\ntemplate<typename TInstance, typename TKey, typename TItem>\r\nvoid DictionaryAdd(\r\n    ");
		Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
		Write("::IInspectable const& instance,\r\n    ");
		Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
		Write("::IInspectable const& key,\r\n    ");
		Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
		Write("::IInspectable const& item)\r\n{\r\n    instance.as<TInstance>().Insert(::winrt::unbox_value<TKey>(key), ::winrt::unbox_value<TItem>(item));\r\n}\r\n\r\ntemplate<typename T>\r\n");
		Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
		Write("::IInspectable FromStringConverter(\r\n    XamlUserType const& userType, \r\n    ::winrt::hstring const& input)\r\n{\r\n    return ::winrt::box_value(static_cast<T>(userType.CreateEnumUIntFromString(input)));\r\n}\r\n");
		foreach (string attachableMemberGetterUniqueName in base.Model.AttachableMemberGetterUniqueNames)
		{
			Write("\r\ntemplate<typename TDeclaringType, typename TargetType>\r\n");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable GetAttachableMember_");
			Write(base.ToStringHelper.ToStringWithCulture(attachableMemberGetterUniqueName));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& instance)\r\n{\r\n    return ::winrt::box_value(TDeclaringType::Get");
			Write(base.ToStringHelper.ToStringWithCulture(attachableMemberGetterUniqueName));
			Write("(instance.as<TargetType>()));\r\n}\r\n");
		}
		foreach (string valueTypeMemberGetterUniqueName in base.Model.ValueTypeMemberGetterUniqueNames)
		{
			Write("\r\ntemplate<typename TDeclaringType, typename TValue>\r\n");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable GetValueTypeMember_");
			Write(base.ToStringHelper.ToStringWithCulture(valueTypeMemberGetterUniqueName));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& instance)\r\n{\r\n    return ::winrt::box_value<TValue>(instance.as<TDeclaringType>().");
			Write(base.ToStringHelper.ToStringWithCulture(valueTypeMemberGetterUniqueName));
			Write("());\r\n}\r\n");
		}
		foreach (string stringGetterUniqueName in base.Model.StringGetterUniqueNames)
		{
			Write("\r\ntemplate <typename T>\r\n");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable GetReferenceTypeStringMember_");
			Write(base.ToStringHelper.ToStringWithCulture(stringGetterUniqueName));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& instance)\r\n{\r\n   return ::winrt::box_value(");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::PropertyValue::CreateString(instance.as<T>().");
			Write(base.ToStringHelper.ToStringWithCulture(stringGetterUniqueName));
			Write("()));\r\n}\r\n");
		}
		foreach (string referenceTypeMemberGetterUniqueNamesNoString in base.Model.ReferenceTypeMemberGetterUniqueNamesNoStrings)
		{
			Write("\r\ntemplate <typename T>\r\n");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable GetReferenceTypeMember_");
			Write(base.ToStringHelper.ToStringWithCulture(referenceTypeMemberGetterUniqueNamesNoString));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& instance)\r\n{\r\n    return ::winrt::box_value(instance.as<T>().");
			Write(base.ToStringHelper.ToStringWithCulture(referenceTypeMemberGetterUniqueNamesNoString));
			Write("());\r\n}\r\n");
		}
		foreach (string attachableMemberSetterUniqueName in base.Model.AttachableMemberSetterUniqueNames)
		{
			Write("\r\ntemplate<typename TDeclaringType, typename TTargetType, typename TValue>\r\nvoid SetAttachableMember_");
			Write(base.ToStringHelper.ToStringWithCulture(attachableMemberSetterUniqueName));
			Write("(");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& instance, ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& value)\r\n{\r\n    TDeclaringType::Set");
			Write(base.ToStringHelper.ToStringWithCulture(attachableMemberSetterUniqueName));
			Write("(instance.as<TTargetType>(), ::winrt::unbox_value<TValue>(value));\r\n}\r\n");
		}
		foreach (string enumTypeMemberSetterUniqueName in base.Model.EnumTypeMemberSetterUniqueNames)
		{
			Write("\r\ntemplate<typename TDeclaringType, typename TValue>\r\nvoid SetEnumMember_");
			Write(base.ToStringHelper.ToStringWithCulture(enumTypeMemberSetterUniqueName));
			Write("(\r\n    ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& instance, \r\n    ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& value)\r\n{\r\n    instance.as<TDeclaringType>().");
			Write(base.ToStringHelper.ToStringWithCulture(enumTypeMemberSetterUniqueName));
			Write("(::winrt::unbox_value<TValue>(value));\r\n}\r\n");
		}
		foreach (string valueTypeMemberSetterUniqueName in base.Model.ValueTypeMemberSetterUniqueNames)
		{
			Write("\r\ntemplate<typename TDeclaringType, typename TValue>\r\nvoid SetValueTypeMember_");
			Write(base.ToStringHelper.ToStringWithCulture(valueTypeMemberSetterUniqueName));
			Write("(\r\n    ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& instance, \r\n    ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& value)\r\n{\r\n    instance.as<TDeclaringType>().");
			Write(base.ToStringHelper.ToStringWithCulture(valueTypeMemberSetterUniqueName));
			Write("(::winrt::unbox_value<TValue>(value));\r\n}\r\n");
		}
		foreach (string stringSetterUniqueName in base.Model.StringSetterUniqueNames)
		{
			Write("\r\ntemplate<typename TDeclaringType, typename TValue>\r\nvoid SetReferenceTypeStringMember_");
			Write(base.ToStringHelper.ToStringWithCulture(stringSetterUniqueName));
			Write("(\r\n    ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& instance, \r\n    ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& value)\r\n{\r\n    return instance.as<TDeclaringType>().");
			Write(base.ToStringHelper.ToStringWithCulture(stringSetterUniqueName));
			Write("(::winrt::unbox_value<::winrt::hstring>(value));\r\n}\r\n");
		}
		foreach (string referenceTypeMemberSetterUniqueNamesNoString in base.Model.ReferenceTypeMemberSetterUniqueNamesNoStrings)
		{
			Write("\r\ntemplate<typename TDeclaringType, typename TValue>\r\nvoid SetReferenceTypeMember_");
			Write(base.ToStringHelper.ToStringWithCulture(referenceTypeMemberSetterUniqueNamesNoString));
			Write("(\r\n    ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& instance, \r\n    ");
			Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeInfoDefinition>.Projection("Windows.Foundation")));
			Write("::IInspectable const& value)\r\n{\r\n    instance.as<TDeclaringType>().");
			Write(base.ToStringHelper.ToStringWithCulture(referenceTypeMemberSetterUniqueNamesNoString));
			Write("(value.as<TValue>());\r\n}\r\n");
		}
		return base.GenerationEnvironment.ToString();
	}
}
