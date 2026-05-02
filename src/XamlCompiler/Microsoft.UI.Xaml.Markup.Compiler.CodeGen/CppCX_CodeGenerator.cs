using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class CppCX_CodeGenerator<T> : NativeCodeGenerator<T>
{
	public override string ToStringWithCulture(ICodeGenOutput codegenOutput)
	{
		return codegenOutput.CppCXName();
	}

	public override string ToStringWithCulture(XamlType type)
	{
		return type.CppCXName();
	}

	public new static string Colonize(string typeName)
	{
		return KnownStrings.Colonize(typeName).Replace("System::", "Platform::");
	}

	internal static string ColonizeRef(EventAssignment ev)
	{
		return XamlSchemaCodeInfo.GetFullGenericNestedName(ev.Event.Type.UnderlyingType, "C++", globalized: false);
	}

	internal static string ColonizeRef(BoundEventAssignment ev)
	{
		return XamlSchemaCodeInfo.GetFullGenericNestedName(ev.MemberType.UnderlyingType, "C++", globalized: false);
	}

	public static string Projection(string typeName)
	{
		return NativeCodeGenerator<T>.Globalize(typeName);
	}

	protected string GetBindingFullClassName(BindUniverse bindUniverse, XamlClassCodeInfo codeInfo)
	{
		return Colonize(codeInfo.ClassName.FullName + "." + bindUniverse.BindingsClassName);
	}

	protected string GetBindingTrackingFullClassName(BindUniverse bindUniverse, XamlClassCodeInfo codeInfo)
	{
		return Colonize(bindUniverse.NeedsCppBindingTrackingClass ? (codeInfo.ClassName.Namespace + "." + bindUniverse.BindingsTrackingClassName) : "::XamlBindingInfo::XamlBindingTrackingBase");
	}

	public void OutputNamespaceBegin(string name)
	{
		string[] array = name.Split('.');
		foreach (string text in array)
		{
			WriteLine("namespace " + text);
			WriteLine("{");
			PushIndent();
		}
	}

	public void OutputNamespaceEnd(string name)
	{
		string[] array = name.Split('.');
		foreach (string text in array)
		{
			PopIndent();
			WriteLine("}");
		}
	}
}
