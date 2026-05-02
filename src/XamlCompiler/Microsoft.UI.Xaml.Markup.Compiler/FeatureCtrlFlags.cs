using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

[Flags]
internal enum FeatureCtrlFlags
{
	Nothing = 0,
	EnableTypeInfoReflection = 1,
	EnableXBindDiagnostics = 2,
	EnableDefaultValidationContextGeneration = 4,
	EnableWin32Codegen = 8,
	UsingCSWinRT = 0x10,
	EnableBindingDiagnostics = 0x20
}
