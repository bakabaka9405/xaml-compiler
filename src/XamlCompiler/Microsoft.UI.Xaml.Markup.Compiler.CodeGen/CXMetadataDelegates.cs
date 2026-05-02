using System.CodeDom.Compiler;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

[GeneratedCode("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
internal class CXMetadataDelegates : CppCX_CodeGenerator<TypeGenInfo>
{
	public override string TransformText()
	{
		bool flag = (bool)base.Arguments[0];
		Write("\r\n");
		if (base.Model.HasActivator)
		{
			if (flag)
			{
				Write("extern ::Platform::Object^ ");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.ActivatorName));
				Write("();\r\n");
			}
			else
			{
				Write("::Platform::Object^ ");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.ActivatorName));
				Write("()\r\n{\r\n    return ref new ");
				Write(base.ToStringHelper.ToStringWithCulture(base.Model.FullName));
				Write("();\r\n}\r\n");
			}
		}
		if (!base.ProjectInfo.EnableTypeInfoReflection)
		{
			if (base.Model.IsCollection)
			{
				if (flag)
				{
					Write("extern void ");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.CollectionAddName));
					Write("(::Platform::Object^ instance, ::Platform::Object^ item);\r\n");
				}
				else
				{
					Write("void ");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.CollectionAddName));
					Write("(::Platform::Object^ instance, ::Platform::Object^ item)\r\n{\r\n    safe_cast<");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.FullName));
					Write("^>(instance)->Append((");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.ItemType.FullName));
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.ItemType.RefHat));
					Write(")item);\r\n}\r\n");
				}
			}
			if (base.Model.IsDictionary)
			{
				if (flag)
				{
					Write("extern void ");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.DictionaryAddName));
					Write("(::Platform::Object^ instance, ::Platform::Object^ key, ::Platform::Object^ item);\r\n");
				}
				else
				{
					Write("void ");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.DictionaryAddName));
					Write("(::Platform::Object^ instance, ::Platform::Object^ key, ::Platform::Object^ item)\r\n{\r\n    safe_cast<");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.FullName));
					Write("^>(instance)->Insert((");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.KeyType.FullName));
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.KeyType.RefHat));
					Write(")key, (");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.ItemType.FullName));
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.ItemType.RefHat));
					Write(")item);\r\n}\r\n");
				}
			}
			if (base.Model.HasEnumValues)
			{
				if (flag)
				{
					Write("extern ::Platform::Object^ ");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.FromStringConverterName));
					Write("(::XamlTypeInfo::InfoProvider::XamlUserType^ userType, ::Platform::String^ input);\r\n");
				}
				else
				{
					Write("::Platform::Object^ ");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.FromStringConverterName));
					Write("(::XamlTypeInfo::InfoProvider::XamlUserType^ userType, ::Platform::String^ input)\r\n{\r\n    return ref new ::Platform::Box<");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.FullName));
					Write(">((");
					Write(base.ToStringHelper.ToStringWithCulture(base.Model.FullName));
					Write(")userType->CreateEnumUIntFromString(input));\r\n}\r\n");
				}
			}
			foreach (MemberGenInfo member in base.Model.Members)
			{
				Write("\r\n");
				if (member.HasGetAttachableMember)
				{
					if (flag)
					{
						Write("extern ::Platform::Object^ ");
						Write(base.ToStringHelper.ToStringWithCulture(member.GetAttachableMemberName));
						Write("(::Platform::Object^ instance);\r\n");
					}
					else
					{
						Write("::Platform::Object^ ");
						Write(base.ToStringHelper.ToStringWithCulture(member.GetAttachableMemberName));
						Write("(::Platform::Object^ instance)\r\n{\r\n    return ");
						Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
						Write("::Get");
						Write(base.ToStringHelper.ToStringWithCulture(member.Name));
						Write("(safe_cast<");
						Write(base.ToStringHelper.ToStringWithCulture(member.TargetType.FullName));
						Write("^>(instance));\r\n}\r\n");
					}
				}
				if (member.HasSetAttachableMember)
				{
					if (flag)
					{
						Write("extern void ");
						Write(base.ToStringHelper.ToStringWithCulture(member.SetAttachableMemberName));
						Write("(::Platform::Object^ instance, ::Platform::Object^ value);\r\n");
					}
					else
					{
						Write("void ");
						Write(base.ToStringHelper.ToStringWithCulture(member.SetAttachableMemberName));
						Write("(::Platform::Object^ instance, ::Platform::Object^ value)\r\n{\r\n    ");
						Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
						Write("::Set");
						Write(base.ToStringHelper.ToStringWithCulture(member.Name));
						Write("(safe_cast<");
						Write(base.ToStringHelper.ToStringWithCulture(member.TargetType.FullName));
						Write("^>(instance), (");
						Write(base.ToStringHelper.ToStringWithCulture(member.Type.FullName));
						Write(base.ToStringHelper.ToStringWithCulture(member.IsValueType ? string.Empty : "^"));
						Write(")value);\r\n}\r\n");
					}
				}
				if (member.HasGetValueTypeMember)
				{
					if (flag)
					{
						Write("extern ::Platform::Object^ ");
						Write(base.ToStringHelper.ToStringWithCulture(member.GetValueTypeMemberName));
						Write("(::Platform::Object^ instance);\r\n");
					}
					else
					{
						Write("::Platform::Object^ ");
						Write(base.ToStringHelper.ToStringWithCulture(member.GetValueTypeMemberName));
						Write("(::Platform::Object^ instance)\r\n{\r\n    return ref new ::Platform::Box<");
						Write(base.ToStringHelper.ToStringWithCulture(member.Type.FullName));
						Write(">(safe_cast<");
						Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
						Write("^>(instance)->");
						Write(base.ToStringHelper.ToStringWithCulture(member.Name));
						Write(");\r\n}\r\n");
					}
				}
				if (member.HasSetValueTypeMember)
				{
					if (flag)
					{
						Write("extern void ");
						Write(base.ToStringHelper.ToStringWithCulture(member.SetValueTypeMemberName));
						Write("(::Platform::Object^ instance, ::Platform::Object^ value);\r\n");
					}
					else
					{
						Write("void ");
						Write(base.ToStringHelper.ToStringWithCulture(member.SetValueTypeMemberName));
						Write("(::Platform::Object^ instance, ::Platform::Object^ value)\r\n{\r\n    safe_cast<");
						Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
						Write("^>(instance)->");
						Write(base.ToStringHelper.ToStringWithCulture(member.Name));
						Write(" = safe_cast<::Platform::IBox<");
						Write(base.ToStringHelper.ToStringWithCulture(member.Type.FullName));
						Write(">^>(value)->Value;\r\n}\r\n");
					}
				}
				if (member.HasSetEnumMember)
				{
					if (flag)
					{
						Write("extern void ");
						Write(base.ToStringHelper.ToStringWithCulture(member.SetEnumMemberName));
						Write("(::Platform::Object^ instance, ::Platform::Object^ value);\r\n");
					}
					else
					{
						Write("void ");
						Write(base.ToStringHelper.ToStringWithCulture(member.SetEnumMemberName));
						Write("(::Platform::Object^ instance, ::Platform::Object^ value)\r\n{\r\n    safe_cast<");
						Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
						Write("^>(instance)->");
						Write(base.ToStringHelper.ToStringWithCulture(member.Name));
						Write(" = safe_cast<::Platform::IBox<");
						Write(base.ToStringHelper.ToStringWithCulture(member.Type.FullName));
						Write(">^>(value)->Value;\r\n}\r\n");
					}
				}
				if (member.HasGetReferenceTypeMember)
				{
					if (flag)
					{
						Write("extern ::Platform::Object^ ");
						Write(base.ToStringHelper.ToStringWithCulture(member.GetReferenceTypeMemberName));
						Write("(::Platform::Object^ instance);\r\n");
					}
					else
					{
						Write("::Platform::Object^ ");
						Write(base.ToStringHelper.ToStringWithCulture(member.GetReferenceTypeMemberName));
						Write("(::Platform::Object^ instance)\r\n{\r\n    return safe_cast<");
						Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
						Write("^>(instance)->");
						Write(base.ToStringHelper.ToStringWithCulture(member.Name));
						Write(";\r\n}\r\n");
					}
				}
				if (member.HasSetReferenceTypeMember)
				{
					if (flag)
					{
						Write("extern void ");
						Write(base.ToStringHelper.ToStringWithCulture(member.SetReferenceTypeMemberName));
						Write("(::Platform::Object^ instance, ::Platform::Object^ value);\r\n");
						continue;
					}
					Write("void ");
					Write(base.ToStringHelper.ToStringWithCulture(member.SetReferenceTypeMemberName));
					Write("(::Platform::Object^ instance, ::Platform::Object^ value)\r\n{\r\n    safe_cast<");
					Write(base.ToStringHelper.ToStringWithCulture(member.DeclaringType.FullName));
					Write("^>(instance)->");
					Write(base.ToStringHelper.ToStringWithCulture(member.Name));
					Write(" = safe_cast<");
					Write(base.ToStringHelper.ToStringWithCulture(member.Type.FullName));
					Write("^>(value);\r\n}\r\n");
				}
			}
		}
		return base.GenerationEnvironment.ToString();
	}
}
