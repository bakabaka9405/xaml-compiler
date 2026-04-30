using System;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal abstract class ManagedCodeGenerator<T> : CodeGenerator<T>
{
	private static string xamlCompilerVersion;

	public static string XamlCompilerVersion
	{
		get
		{
			if (string.IsNullOrEmpty(xamlCompilerVersion))
			{
				Version version = Assembly.GetExecutingAssembly().GetName().Version;
				xamlCompilerVersion = version.ToString();
			}
			return xamlCompilerVersion;
		}
	}

	public string WeakReferenceTypeName => "System.WeakReference";
}
