using System;
using System.Runtime.InteropServices;

namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

[ComImport]
[Guid("ef46679c-4ec5-447a-bd26-e04f1d2c2551")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IXbfMetadataProvider
{
	IXbfType GetXamlType(Type type);

	IXbfType GetXamlType([MarshalAs(UnmanagedType.BStr)] string fullName);

	object[] GetXmlnsDefinitions();
}
