using System;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[Guid("EE62470B-E94B-424e-9B7C-2F00C9249F93")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMetadataAssemblyImport
{
	void GetAssemblyProps([In] Token assemblyToken, out EmbeddedBlobPointer pPublicKey, out int cbPublicKey, out int hashAlgId, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName, [In] int cchName, out int pchName, [In][Out] ref AssemblyMetaData pMetaData, out AssemblyNameFlags flags);

	void GetAssemblyRefProps([In] Token token, out EmbeddedBlobPointer pPublicKey, out int cbPublicKey, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName, [In] int cchName, out int pchName, [In][Out] ref AssemblyMetaData pMetaData, out UnusedIntPtr ppbHashValue, out uint pcbHashValue, out AssemblyNameFlags dwAssemblyRefFlags);

	void GetFileProps([In] int token, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName, [In] int cchName, out int pchName, out UnusedIntPtr ppbHashValue, out uint pcbHashValue, out CorFileFlags dwFileFlags);

	void GetExportedTypeProps(int mdct, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName, int cchName, out int pchName, out int ptkImplementation, out int ptkTypeDef, out CorTypeAttr pdwExportedTypeFlags);

	void GetManifestResourceProps([In] int mdmr, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName, [In] int cchName, out int pchName, [ComAliasName("mdToken*")] out int ptkImplementation, [ComAliasName("DWORD*")] out uint pdwOffset, out CorManifestResourceFlags pdwResourceFlags);

	[PreserveSig]
	int EnumAssemblyRefs(ref HCORENUM phEnum, out Token assemblyRefs, int cMax, out int cTokens);

	void EnumFiles(ref HCORENUM phEnum, out int files, int cMax, out int cTokens);

	void EnumExportedTypes(ref HCORENUM phEnum, out int rExportedTypes, int cMax, out uint cTokens);

	void EnumManifestResources(ref HCORENUM phEnum, out int rManifestResources, int cMax, out int cTokens);

	[PreserveSig]
	int GetAssemblyFromScope(out int assemblyToken);

	void FindExportedTypeByName_();

	[PreserveSig]
	int FindManifestResourceByName([MarshalAs(UnmanagedType.LPWStr)] string szName, out int ptkManifestResource);

	[PreserveSig]
	int CloseEnum(IntPtr hEnum);
}
