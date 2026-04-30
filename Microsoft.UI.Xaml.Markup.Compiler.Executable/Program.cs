using System;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;

namespace Microsoft.UI.Xaml.Markup.Compiler.Executable;

internal class Program
{
	internal static int Main(string[] args)
	{
		try
		{
			CompileXaml compileXaml = new CompileXaml();
			return compileXaml.Run(args);
		}
		catch (Exception ex)
		{
			Console.Error.Write(XamlCompilerResources.XamlInternlError + ": " + ex.Message);
			return 1;
		}
	}
}
