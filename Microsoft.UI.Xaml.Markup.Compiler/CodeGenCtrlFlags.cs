using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

[Flags]
internal enum CodeGenCtrlFlags
{
	Nothing = 0,
	NoPageCodeGen = 1,
	NoTypeInfoCodeGen = 2,
	IncrementalTypeInfoCodeGen = 4,
	DoNotGenerateOtherProviders = 8,
	FullXamlMetadataProvider = 0x10,
	DoNotGenerateCppWinRTStaticAsserts = 0x20
}
