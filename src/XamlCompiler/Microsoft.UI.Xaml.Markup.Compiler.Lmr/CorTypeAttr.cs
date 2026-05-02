using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[Flags]
internal enum CorTypeAttr
{
	tdVisibilityMask = 7,
	tdNotPublic = 0,
	tdPublic = 1,
	tdNestedPublic = 2,
	tdNestedPrivate = 3,
	tdNestedFamily = 4,
	tdNestedAssembly = 5,
	tdNestedFamANDAssem = 6,
	tdNestedFamORAssem = 7,
	tdLayoutMask = 0x18,
	tdAutoLayout = 0,
	tdSequentialLayout = 8,
	tdExplicitLayout = 0x10,
	tdClassSemanticsMask = 0x20,
	tdClass = 0,
	tdInterface = 0x20,
	tdAbstract = 0x80,
	tdSealed = 0x100,
	tdSpecialName = 0x400,
	tdImport = 0x1000,
	tdSerializable = 0x2000,
	tdStringFormatMask = 0x30000,
	tdAnsiClass = 0,
	tdUnicodeClass = 0x10000,
	tdAutoClass = 0x20000,
	tdCustomFormatClass = 0x30000,
	tdCustomFormatMask = 0xC00000,
	tdBeforeFieldInit = 0x100000,
	tdForwarder = 0x200000,
	tdReservedMask = 0x40800,
	tdRTSpecialName = 0x800,
	tdHasSecurity = 0x40000
}
