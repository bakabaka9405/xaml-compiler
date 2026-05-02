namespace Microsoft.UI.Xaml.Markup.Compiler;

internal static class KnownStrings
{
	public const string UriClrNamespace = "clr-namespace";

	public const string UriAssembly = "assembly";

	public const string DefaultHeaderExtension = ".h";

	public const string GeneratedHppExtension = ".g.hpp";

	public const string XamlExtension = ".xaml";

	public const string XbfExtension = ".xbf";

	public const string FrameworkTemplate = "FrameworkTemplate";

	public const string ResourceDictionary = "ResourceDictionary";

	public const string Source = "Source";

	public const string Library = "Library";

	public const string WinMdObj = "WinMdObj";

	public const string XAMLBuildTaskAsmName = "Microsoft.UI.Xaml.Markup.Compiler";

	public const string BackupSuffix = ".backup";

	public const string XamlBindingInfo = "XamlBindingInfo";

	public const string XamlTypeInfo = "XamlTypeInfo";

	public const string Get = "Get";

	public const string Set = "Set";

	public const string Debug = "Debug";

	public const string Converter = "Converter";

	public const string ConverterLanguage = "ConverterLanguage";

	public const string ConverterParameter = "ConverterParameter";

	public const string FallbackValue = "FallbackValue";

	public const string LostFocus = "LostFocus";

	public const string Mode = "Mode";

	public const string OneTime = "OneTime";

	public const string OneWay = "OneWay";

	public const string TwoWay = "TwoWay";

	public const string TargetNullValue = "TargetNullValue";

	public const string BindBack = "BindBack";

	public const string Path = "Path";

	public const string UpdateSourceTrigger = "UpdateSourceTrigger";

	public const string XColon = "x:";

	public const string op_Implicit = "op_Implicit";

	public const string op_Explicit = "op_Explicit";

	public const string UpdateParamName = "obj";

	public const string UpdateParamBindingsName = "bindings";

	public const string DataChanged = "DATA_CHANGED";

	public const string NotPhased = "NOT_PHASED";

	public const string DirectCast = "DirectCast";

	public const string CType = "CType";

	public const string Platforms = "Platforms";

	public const string UAP = "UAP";

	public const string UsingPrefix = "using:";

	public const string ClrNamespaceColon = "clr-namespace:";

	public const string SemiColonAssemblyEquals = ";assembly=";

	public const string DeprecatedAttributeDefaultMessage = "Deprecated";

	public const string ObsoleteAttributeDefaultMessage = "Obsolete";

	public const string PlatformAssemblySentinelType = "Windows.Foundation.HResult";

	public const string WinUIAssemblySentinelType = "Microsoft.UI.Xaml.DependencyObject";

	public const string MsCorLib = "mscorlib";

	public static string Colonize(string name)
	{
		return name.Replace(".", "::");
	}
}
