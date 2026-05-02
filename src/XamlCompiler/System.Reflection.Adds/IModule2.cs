namespace System.Reflection.Adds;

internal interface IModule2
{
	int RowCount(MetadataTable metadataTableIndex);

	AssemblyName GetAssemblyNameFromAssemblyRef(Token token);
}
