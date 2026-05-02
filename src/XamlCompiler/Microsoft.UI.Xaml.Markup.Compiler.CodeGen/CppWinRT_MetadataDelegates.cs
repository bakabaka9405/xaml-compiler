using System.CodeDom.Compiler;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

[GeneratedCode("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
internal class CppWinRT_MetadataDelegates : CppWinRT_CodeGenerator<TypeGenInfo>
{
	public override string TransformText()
	{
		bool flag = (bool)base.Arguments[0];
		Write("\r\n");
		if (base.Model.HasActivator)
		{
			if (flag)
			{
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable ");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.ActivatorName));
				Write("();\r\n");
			}
			else
			{
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable ");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.ActivatorName));
				Write("()\r\n{\r\n");
				if (base.Model.IsLocalType)
				{
					Write("    return ::winrt::make<");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.FullName.CppWinRTName().ToLocalCppWinRTTypeName()));
					Write(">();\r\n");
				}
				else
				{
					Write("    return ");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.FullName));
					Write("();\r\n");
				}
				Write("}\r\n");
			}
		}
		if (base.Model.IsCollection)
		{
			if (flag)
			{
				Write("extern void ");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.CollectionAddName));
				Write("(");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable const& instance, ");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable const& item);\r\n");
			}
			else
			{
				Write("void ");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.CollectionAddName));
				Write("(");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable const& instance, ");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable const& item)\r\n{\r\n    instance.as<");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.FullName));
				Write(">().Append(::winrt::unbox_value<");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.ItemType.FullName));
				Write(">(item));\r\n}\r\n");
			}
		}
		if (base.Model.IsDictionary)
		{
			if (flag)
			{
				Write("extern void ");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.DictionaryAddName));
				Write("(");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable const& instance, ");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable const& key, ");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable const& item);\r\n");
			}
			else
			{
				Write("void ");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.DictionaryAddName));
				Write("(");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable const& instance, ");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable const& key, ");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable const& item)\r\n{\r\n    instance.as<");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.FullName));
				Write(">().Insert(::winrt::unbox_value<");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.KeyType.FullName));
				Write(">(key), ::winrt::unbox_value<");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.ItemType.FullName));
				Write(">(item));\r\n}\r\n");
			}
		}
		if (base.Model.HasEnumValues)
		{
			if (flag)
			{
				Write("extern ");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable ");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.FromStringConverterName));
				Write("(XamlUserType const& userType, ::winrt::hstring const& input);\r\n");
			}
			else
			{
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
				Write("::IInspectable ");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.FromStringConverterName));
				Write("(");
				Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection(base.ProjectInfo.RootNamespace)));
				Write("::implementation::XamlUserType const& userType, ::winrt::hstring const& input)\r\n{\r\n    return ::winrt::box_value((");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.FullName));
				Write(")userType.CreateEnumUIntFromString(input));\r\n}\r\n");
			}
		}
		foreach (MemberGenInfo member in base.Model.Members)
		{
			Write("\r\n");
			if (member.HasGetAttachableMember)
			{
				if (flag)
				{
					Write("extern ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable ");
					Write(base.ToStringHelper.ToStringWithCulture(member.GetAttachableMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance);\r\n");
				}
				else
				{
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable ");
					Write(base.ToStringHelper.ToStringWithCulture(member.GetAttachableMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance)\r\n{\r\n    return ::winrt::box_value(");
					Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
					Write("::Get");
					Write(base.ToStringHelper.ToStringWithCulture(member.Name));
					Write("(instance.as<");
					Write(base.ToStringHelper.ToStringWithCulture(member.TargetType.FullName));
					Write(">()));\r\n}\r\n");
				}
			}
			if (member.HasSetAttachableMember)
			{
				if (flag)
				{
					Write("extern void ");
					Write(base.ToStringHelper.ToStringWithCulture(member.SetAttachableMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance, ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& value);\r\n");
				}
				else
				{
					Write("void ");
					Write(base.ToStringHelper.ToStringWithCulture(member.SetAttachableMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance, ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& value)\r\n{\r\n    ");
					Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
					Write("::Set");
					Write(base.ToStringHelper.ToStringWithCulture(member.Name));
					Write("(instance.as<");
					Write(base.ToStringHelper.ToStringWithCulture(member.TargetType.FullName));
					Write(">(), ::winrt::unbox_value<");
					Write(base.ToStringHelper.ToStringWithCulture(member.Type.FullName));
					Write(">(value));\r\n}\r\n");
				}
			}
			if (member.HasGetValueTypeMember)
			{
				if (flag)
				{
					Write("extern ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable ");
					Write(base.ToStringHelper.ToStringWithCulture(member.GetValueTypeMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance);\r\n");
				}
				else
				{
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable ");
					Write(base.ToStringHelper.ToStringWithCulture(member.GetValueTypeMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance)\r\n{\r\n    return ::winrt::box_value<");
					Write(base.ToStringHelper.ToStringWithCulture(member.Type.FullName));
					Write(">(instance.as<");
					Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
					Write(">().");
					Write(base.ToStringHelper.ToStringWithCulture(member.Name));
					Write("());\r\n}\r\n");
				}
			}
			if (member.HasSetValueTypeMember)
			{
				if (flag)
				{
					Write("extern void ");
					Write(base.ToStringHelper.ToStringWithCulture(member.SetValueTypeMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance, ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& value);\r\n");
				}
				else
				{
					Write("void ");
					Write(base.ToStringHelper.ToStringWithCulture(member.SetValueTypeMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance, ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& value)\r\n{\r\n    instance.as<");
					Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
					Write(">().");
					Write(base.ToStringHelper.ToStringWithCulture(member.Name));
					Write("(::winrt::unbox_value<");
					Write(base.ToStringHelper.ToStringWithCulture(member.Type.FullName));
					Write(">(value));\r\n}\r\n");
				}
			}
			if (member.HasSetEnumMember)
			{
				if (flag)
				{
					Write("extern void ");
					Write(base.ToStringHelper.ToStringWithCulture(member.SetEnumMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance, ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& value);\r\n");
				}
				else
				{
					Write("void ");
					Write(base.ToStringHelper.ToStringWithCulture(member.SetEnumMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance, ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& value)\r\n{\r\n    instance.as<");
					Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
					Write(">().");
					Write(base.ToStringHelper.ToStringWithCulture(member.Name));
					Write("(::winrt::unbox_value<");
					Write(base.ToStringHelper.ToStringWithCulture(member.Type.FullName));
					Write(">(value));\r\n}\r\n");
				}
			}
			if (member.HasGetReferenceTypeMember)
			{
				if (flag)
				{
					Write("extern ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable ");
					Write(base.ToStringHelper.ToStringWithCulture(member.GetReferenceTypeMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance);\r\n");
				}
				else
				{
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable ");
					Write(base.ToStringHelper.ToStringWithCulture(member.GetReferenceTypeMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance)\r\n{\r\n    return ::winrt::box_value(instance.as<");
					Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
					Write(">().");
					Write(base.ToStringHelper.ToStringWithCulture(member.Name));
					Write("());\r\n}\r\n");
				}
			}
			if (member.HasSetReferenceTypeMember)
			{
				if (flag)
				{
					Write("extern void ");
					Write(base.ToStringHelper.ToStringWithCulture(member.SetReferenceTypeMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance, ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& value);\r\n");
				}
				else
				{
					Write("void ");
					Write(base.ToStringHelper.ToStringWithCulture(member.SetReferenceTypeMemberName));
					Write("(");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& instance, ");
					Write(base.ToStringHelper.ToStringWithCulture(CppWinRT_CodeGenerator<TypeGenInfo>.Projection("Windows.Foundation")));
					Write("::IInspectable const& value)\r\n{\r\n    instance.as<");
					Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
					Write(">().");
					Write(base.ToStringHelper.ToStringWithCulture(member.Name));
					Write("(::winrt::unbox_value<");
					Write(base.ToStringHelper.ToStringWithCulture(member.Type.FullName));
					Write(">(value));\r\n}\r\n");
				}
			}
		}
		return base.GenerationEnvironment.ToString();
	}
}
