namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class EnumGenInfo
{
	public TypeGenInfo TypeInfo { get; private set; }

	public string ValueName { get; private set; }

	public EnumGenInfo(TypeGenInfo typeInfo, string valueName)
	{
		TypeInfo = typeInfo;
		ValueName = valueName;
	}
}
