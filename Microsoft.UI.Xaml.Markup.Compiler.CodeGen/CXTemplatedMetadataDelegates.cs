using System.CodeDom.Compiler;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

[GeneratedCode("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
internal class CXTemplatedMetadataDelegates : CppCX_CodeGenerator<TypeInfoDefinition>
{
	public override string TransformText()
	{
		Write("template<typename T>\r\n::Platform::Object^ ActivateType()\r\n{\r\n    return ref new T;\r\n}\r\n\r\n");
		if (!base.ProjectInfo.EnableTypeInfoReflection)
		{
			Write("template<typename TInstance, typename TItem>\r\nvoid CollectionAdd(::Platform::Object^ instance, ::Platform::Object^ item)\r\n{\r\n    safe_cast<TInstance^>(instance)->Append((TItem)item);\r\n}\r\n\r\ntemplate<typename TInstance, typename TKey, typename TItem>\r\nvoid DictionaryAdd(::Platform::Object^ instance, ::Platform::Object^ key, ::Platform::Object^ item)\r\n{\r\n    safe_cast<TInstance^>(instance)->Insert((TKey)key, (TItem)item);\r\n}\r\n\r\ntemplate<typename T>\r\n::Platform::Object^ FromStringConverter(::XamlTypeInfo::InfoProvider::XamlUserType^ userType, ::Platform::String^ input)\r\n{\r\n    return ref new ::Platform::Box<T>((T)userType->CreateEnumUIntFromString(input));\r\n}\r\n");
			foreach (string attachableMemberGetterUniqueName in base.Model.AttachableMemberGetterUniqueNames)
			{
				Write("\r\ntemplate<typename TDeclaringType, typename TargetType>\r\n::Platform::Object^ GetAttachableMember_");
				Write(base.ToStringHelper.ToStringWithCulture(attachableMemberGetterUniqueName));
				Write("(::Platform::Object^ instance)\r\n{\r\n    return TDeclaringType::Get");
				Write(base.ToStringHelper.ToStringWithCulture(attachableMemberGetterUniqueName));
				Write("(safe_cast<TargetType^>(instance));\r\n}\r\n");
			}
			foreach (string valueTypeMemberGetterUniqueName in base.Model.ValueTypeMemberGetterUniqueNames)
			{
				Write("\r\ntemplate<typename TDeclaringType, typename TValue>\r\n::Platform::Object^ GetValueTypeMember_");
				Write(base.ToStringHelper.ToStringWithCulture(valueTypeMemberGetterUniqueName));
				Write("(::Platform::Object^ instance)\r\n{\r\n    return ref new ::Platform::Box<TValue>(safe_cast<TDeclaringType^>(instance)->");
				Write(base.ToStringHelper.ToStringWithCulture(valueTypeMemberGetterUniqueName));
				Write(");\r\n}\r\n");
			}
			foreach (string referenceTypeMemberGetterUniqueName in base.Model.ReferenceTypeMemberGetterUniqueNames)
			{
				Write("\r\ntemplate<typename TDeclaringType>\r\n::Platform::Object^ GetReferenceTypeMember_");
				Write(base.ToStringHelper.ToStringWithCulture(referenceTypeMemberGetterUniqueName));
				Write("(::Platform::Object^ instance)\r\n{\r\n    return safe_cast<TDeclaringType^>(instance)->");
				Write(base.ToStringHelper.ToStringWithCulture(referenceTypeMemberGetterUniqueName));
				Write(";\r\n}\r\n");
			}
			foreach (string attachableMemberSetterUniqueName in base.Model.AttachableMemberSetterUniqueNames)
			{
				Write("\r\ntemplate<typename TDeclaringType, typename TTargetType, typename TValue>\r\nvoid SetAttachableMember_");
				Write(base.ToStringHelper.ToStringWithCulture(attachableMemberSetterUniqueName));
				Write("(::Platform::Object^ instance, ::Platform::Object^ value)\r\n{\r\n    TDeclaringType::Set");
				Write(base.ToStringHelper.ToStringWithCulture(attachableMemberSetterUniqueName));
				Write("(safe_cast<TTargetType^>(instance), (TValue)value);\r\n}\r\n");
			}
			foreach (string enumTypeMemberSetterUniqueName in base.Model.EnumTypeMemberSetterUniqueNames)
			{
				Write("\r\ntemplate<typename TDeclaringType, typename TValue>\r\nvoid SetEnumMember_");
				Write(base.ToStringHelper.ToStringWithCulture(enumTypeMemberSetterUniqueName));
				Write("(::Platform::Object^ instance, ::Platform::Object^ value)\r\n{\r\n    safe_cast<TDeclaringType^>(instance)->");
				Write(base.ToStringHelper.ToStringWithCulture(enumTypeMemberSetterUniqueName));
				Write(" = safe_cast<::Platform::IBox<TValue>^>(value)->Value;\r\n}\r\n");
			}
			foreach (string valueTypeMemberSetterUniqueName in base.Model.ValueTypeMemberSetterUniqueNames)
			{
				Write("\r\ntemplate<typename TDeclaringType, typename TValue>\r\nvoid SetValueTypeMember_");
				Write(base.ToStringHelper.ToStringWithCulture(valueTypeMemberSetterUniqueName));
				Write("(::Platform::Object^ instance, ::Platform::Object^ value)\r\n{\r\n    safe_cast<TDeclaringType^>(instance)->");
				Write(base.ToStringHelper.ToStringWithCulture(valueTypeMemberSetterUniqueName));
				Write(" = safe_cast<::Platform::IBox<TValue>^>(value)->Value;\r\n}\r\n");
			}
			foreach (string referenceTypeMemberSetterUniqueName in base.Model.ReferenceTypeMemberSetterUniqueNames)
			{
				Write("\r\ntemplate<typename TDeclaringType, typename TValue>\r\nvoid SetReferenceTypeMember_");
				Write(base.ToStringHelper.ToStringWithCulture(referenceTypeMemberSetterUniqueName));
				Write("(::Platform::Object^ instance, ::Platform::Object^ value)\r\n{\r\n    safe_cast<TDeclaringType^>(instance)->");
				Write(base.ToStringHelper.ToStringWithCulture(referenceTypeMemberSetterUniqueName));
				Write(" = safe_cast<TValue^>(value);\r\n}\r\n");
			}
		}
		return base.GenerationEnvironment.ToString();
	}
}
