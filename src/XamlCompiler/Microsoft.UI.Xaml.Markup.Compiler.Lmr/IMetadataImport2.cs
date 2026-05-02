using System;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[Guid("FCE5EFA0-8BBA-4f8e-A036-8F2022B08466")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMetadataImport2 : IMetadataImport
{
	[PreserveSig]
	new void CloseEnum(IntPtr hEnum);

	new void CountEnum(HCORENUM hEnum, [ComAliasName("ULONG*")] out int pulCount);

	new void ResetEnum(HCORENUM hEnum, int ulPos);

	new void EnumTypeDefs(ref HCORENUM phEnum, [ComAliasName("mdTypeDef*")] out int rTypeDefs, uint cMax, [ComAliasName("ULONG*")] out uint pcTypeDefs);

	new void EnumInterfaceImpls(ref HCORENUM phEnum, int td, out int rImpls, int cMax, ref int pcImpls);

	new void EnumTypeRefs_();

	new void FindTypeDefByName([In][MarshalAs(UnmanagedType.LPWStr)] string szTypeDef, [In] int tkEnclosingClass, [ComAliasName("mdTypeDef*")] out int token);

	new void GetScopeProps([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName, [In] int cchName, [ComAliasName("ULONG*")] out int pchName, out Guid mvid);

	new void GetModuleFromScope(out int mdModule);

	new void GetTypeDefProps([In] int td, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szTypeDef, [In] int cchTypeDef, [ComAliasName("ULONG*")] out int pchTypeDef, [MarshalAs(UnmanagedType.U4)] out TypeAttributes pdwTypeDefFlags, [ComAliasName("mdToken*")] out int ptkExtends);

	new void GetInterfaceImplProps(int iiImpl, out int pClass, out int ptkIface);

	new void GetTypeRefProps(int tr, [ComAliasName("mdToken*")] out int ptkResolutionScope, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName, [In] int cchName, [ComAliasName("ULONG*")] out int pchName);

	new void ResolveTypeRef_();

	new void EnumMembers_();

	new void EnumMembersWithName_();

	new void EnumMethods(ref HCORENUM phEnum, int cl, [ComAliasName("mdMethodDef*")] out int mdMethodDef, int cMax, [ComAliasName("ULONG*")] out int pcTokens);

	new void EnumMethodsWithName(ref HCORENUM phEnum, int cl, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [ComAliasName("mdMethodDef*")] out int mdMethodDef, int cMax, [ComAliasName("ULONG*")] out int pcTokens);

	new void EnumFields(ref HCORENUM phEnum, int cl, [ComAliasName("mdFieldDef*")] out int mdFieldDef, int cMax, [ComAliasName("ULONG*")] out uint pcTokens);

	new void EnumFieldsWithName_();

	[PreserveSig]
	new int EnumParams(ref HCORENUM phEnum, int mdMethodDef, [MarshalAs(UnmanagedType.LPArray)] int[] rParams, int cMax, [ComAliasName("ULONG*")] out uint pcTokens);

	new void EnumMemberRefs_();

	new void EnumMethodImpls(ref HCORENUM hEnum, Token typeDef, out Token methodBody, out Token methodDecl, int cMax, out int cTokens);

	new void EnumPermissionSets_();

	new void FindMember([In] int typeDefToken, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [In] byte[] pvSigBlob, [In] int cbSigBlob, out int memberDefToken);

	new void FindMethod([In] int typeDef, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [In] EmbeddedBlobPointer pvSigBlob, [In] int cbSigBlob, out int methodDef);

	new void FindField([In] int typeDef, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [In] byte[] pvSigBlob, [In] int cbSigBlob, out int fieldDef);

	new void FindMemberRef([In] int typeRef, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [In] byte[] pvSigBlob, [In] int cbSigBlob, out int result);

	new void GetMethodProps([In] uint md, [ComAliasName("mdTypeDef*")] out int pClass, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szMethod, [In] int cchMethod, [ComAliasName("ULONG*")] out uint pchMethod, [ComAliasName("DWORD*")] out MethodAttributes pdwAttr, [ComAliasName("PCCOR_SIGNATURE*")] out EmbeddedBlobPointer ppvSigBlob, [ComAliasName("ULONG*")] out uint pcbSigBlob, [ComAliasName("ULONG*")] out uint pulCodeRVA, [ComAliasName("DWORD*")] out uint pdwImplFlags);

	new void GetMemberRefProps([In] Token mr, [ComAliasName("mdMemberRef*")] out Token ptk, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szMember, [In] int cchMember, [ComAliasName("ULONG*")] out uint pchMember, [ComAliasName("PCCOR_SIGNATURE*")] out EmbeddedBlobPointer ppvSigBlob, [ComAliasName("ULONG*")] out uint pbSig);

	new void EnumProperties(ref HCORENUM phEnum, int td, [ComAliasName("mdProperty*")] out int mdFieldDef, int cMax, [ComAliasName("ULONG*")] out uint pcTokens);

	new void EnumEvents(ref HCORENUM phEnum, int td, [ComAliasName("mdEvent*")] out int mdFieldDef, int cMax, [ComAliasName("ULONG*")] out uint pcEvents);

	new void GetEventProps(int ev, [ComAliasName("mdTypeDef*")] out int pClass, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szEvent, int cchEvent, [ComAliasName("ULONG*")] out int pchEvent, [ComAliasName("DWORD*")] out int pdwEventFlags, [ComAliasName("mdToken*")] out int ptkEventType, [ComAliasName("mdMethodDef*")] out int pmdAddOn, [ComAliasName("mdMethodDef*")] out int pmdRemoveOn, [ComAliasName("mdMethodDef*")] out int pmdFire, [ComAliasName("mdMethodDef*")] out int rmdOtherMethod, uint cMax, [ComAliasName("ULONG*")] out uint pcOtherMethod);

	new void EnumMethodSemantics_();

	new void GetMethodSemantics_();

	[PreserveSig]
	new uint GetClassLayout(int typeDef, out uint dwPackSize, UnusedIntPtr zeroPtr, uint zeroCount, UnusedIntPtr zeroPtr2, ref uint ulClassSize);

	new void GetFieldMarshal(int token, out EmbeddedBlobPointer pNativeType, out int cbNativeType);

	new void GetRVA(int token, out uint rva, out uint flags);

	new void GetPermissionSetProps_();

	new void GetSigFromToken(int token, out EmbeddedBlobPointer pSig, out int cbSig);

	new void GetModuleRefProps(int mur, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName, int cchName, [ComAliasName("ULONG*")] out int pchName);

	new void EnumModuleRefs(ref HCORENUM phEnum, [ComAliasName("mdModuleRef*")] out int mdModuleRef, int cMax, [ComAliasName("ULONG*")] out uint pcModuleRefs);

	[PreserveSig]
	new int GetTypeSpecFromToken(Token typeSpec, out EmbeddedBlobPointer pSig, out int cbSig);

	new void GetNameFromToken_();

	new void EnumUnresolvedMethods_();

	new void GetUserString([In] int stk, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] szString, [In] int cchString, [ComAliasName("ULONG*")] out int pchString);

	new void GetPinvokeMap_();

	new void EnumSignatures(ref HCORENUM hEnum, [ComAliasName("rSignatures*")] out int rSignature, uint cMax, [ComAliasName("ULONG*")] out uint pcSignatures);

	new void EnumTypeSpecs_();

	new void EnumUserStrings_();

	new void GetParamForMethodIndex_();

	new void EnumCustomAttributes(ref HCORENUM phEnum, int tk, int tkType, [ComAliasName("mdCustomAttribute*")] out Token mdCustomAttribute, uint cMax, [ComAliasName("ULONG*")] out uint pcTokens);

	new void GetCustomAttributeProps([In] Token cv, out Token tkObj, out Token tkType, out EmbeddedBlobPointer blob, out int cbSize);

	new void FindTypeRef([In] int tkResolutionScope, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, out int typeRef);

	new void GetMemberProps_();

	new void GetFieldProps(int mb, [ComAliasName("mdTypeDef*")] out int mdTypeDef, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szField, int cchField, [ComAliasName("ULONG*")] out int pchField, [ComAliasName("DWORD*")] out FieldAttributes pdwAttr, [ComAliasName("PCCOR_SIGNATURE*")] out EmbeddedBlobPointer ppvSigBlob, [ComAliasName("ULONG*")] out int pcbSigBlob, [ComAliasName("DWORD*")] out int pdwCPlusTypeFlab, [ComAliasName("UVCP_CONSTANT*")] out IntPtr ppValue, [ComAliasName("ULONG*")] out int pcchValue);

	new void GetPropertyProps(Token prop, [ComAliasName("mdTypeDef*")] out Token pClass, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szProperty, int cchProperty, [ComAliasName("ULONG*")] out int pchProperty, [ComAliasName("DWORD*")] out PropertyAttributes pdwPropFlags, [ComAliasName("PCCOR_SIGNATURE*")] out EmbeddedBlobPointer ppvSig, [ComAliasName("ULONG*")] out int pbSig, [ComAliasName("DWORD*")] out int pdwCPlusTypeFlag, [ComAliasName("UVCP_CONSTANT*")] out UnusedIntPtr ppDefaultValue, [ComAliasName("ULONG*")] out int pcchDefaultValue, [ComAliasName("mdMethodDef*")] out Token pmdSetter, [ComAliasName("mdMethodDef*")] out Token pmdGetter, [ComAliasName("mdMethodDef*")] out Token rmdOtherMethod, uint cMax, [ComAliasName("ULONG*")] out uint pcOtherMethod);

	new void GetParamProps(int tk, [ComAliasName("mdMethodDef*")] out int pmd, [ComAliasName("ULONG*")] out uint pulSequence, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName, uint cchName, [ComAliasName("ULONG*")] out uint pchName, [ComAliasName("DWORD*")] out uint pdwAttr, [ComAliasName("DWORD*")] out uint pdwCPlusTypeFlag, [ComAliasName("UVCP_CONSTANT*")] out UnusedIntPtr ppValue, [ComAliasName("ULONG*")] out uint pcchValue);

	[PreserveSig]
	new int GetCustomAttributeByName(int tkObj, [MarshalAs(UnmanagedType.LPWStr)] string szName, out EmbeddedBlobPointer ppData, out uint pcbData);

	[PreserveSig]
	new bool IsValidToken([In][MarshalAs(UnmanagedType.U4)] uint tk);

	new void GetNestedClassProps(int tdNestedClass, [ComAliasName("mdTypeDef*")] out int tdEnclosingClass);

	new void GetNativeCallConvFromSig_();

	new void IsGlobal_();

	void EnumGenericParams(ref HCORENUM hEnum, int tk, [ComAliasName("mdGenericParam*")] out int rGenericParams, uint cMax, [ComAliasName("ULONG*")] out uint pcGenericParams);

	void GetGenericParamProps(int gp, [ComAliasName("ULONG*")] out uint pulParamSeq, [ComAliasName("DWORD*")] out int pdwParamFlags, [ComAliasName("mdToken*")] out int ptOwner, [ComAliasName("mdToken*")] out int ptkKind, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzName, [ComAliasName("ULONG*")] uint cchName, [ComAliasName("ULONG*")] out uint pchName);

	void GetMethodSpecProps([ComAliasName("mdMethodSpec")] Token mi, [ComAliasName("mdToken*")] out Token tkParent, [ComAliasName("PCCOR_SIGNATURE*")] out EmbeddedBlobPointer ppvSigBlob, [ComAliasName("ULONG*")] out int pcbSigBlob);

	void EnumGenericParamConstraints(ref HCORENUM hEnum, int tk, [ComAliasName("mdGenericParamConstraint*")] out int rGenericParamConstraints, uint cMax, [ComAliasName("ULONG*")] out uint pcGenericParams);

	void GetGenericParamConstraintProps(int gpc, [ComAliasName("mdGenericParam*")] out int ptGenericParam, [ComAliasName("mdToken*")] out int ptkConstraintType);

	void GetPEKind(out PortableExecutableKinds dwPEKind, out ImageFileMachine pdwMachine);

	void GetVersionString([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName, [In] int cchName, out int pchName);

	void EnumMethodSpecs(ref HCORENUM hEnum, int tk, [ComAliasName("rMethodSpecs*")] out int rMethodSpec, uint cMax, [ComAliasName("ULONG*")] out uint pcMethodSpecs);
}
