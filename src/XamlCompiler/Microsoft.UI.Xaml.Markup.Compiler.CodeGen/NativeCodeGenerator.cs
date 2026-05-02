namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal abstract class NativeCodeGenerator<T> : CodeGenerator<T>
{
	public static string Colonize(string typeName)
	{
		return KnownStrings.Colonize(typeName);
	}

	public static string Globalize(string fullType)
	{
		return "::" + Colonize(fullType).TrimStart(':');
	}
}
