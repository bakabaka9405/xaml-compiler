using System.Reflection.Adds;
using System.Runtime.InteropServices;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[Guid("D8F579AB-402D-4b8e-82D9-5D63B1065C68")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMetadataTables
{
	void GetStringHeapSize(out uint countBytesStrings);

	void GetBlobHeapSize(out uint countBytesBlobs);

	void GetGuidHeapSize(out uint countBytesGuids);

	void GetUserStringHeapSize(out uint countByteBlobs);

	void GetNumTables(out uint countTables);

	void GetTableIndex(uint token, out uint tableIndex);

	void GetTableInfo(MetadataTable tableIndex, out int countByteRows, out int countRows, out int countColumns, out int columnPrimaryKey, out UnusedIntPtr name);

	void GetColumnInfo_();

	void GetCodedTokenInfo_();

	void GetRow_();

	void GetColumn_();

	void GetString_();

	void GetBlob_();

	void GetGuid_();

	void GetUserString_();

	void GetNextString_();

	void GetNextBlob_();

	void GetNextGuid_();

	void GetNextUserString_();
}
