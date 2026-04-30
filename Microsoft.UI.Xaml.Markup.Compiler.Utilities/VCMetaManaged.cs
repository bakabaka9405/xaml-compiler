using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Utilities;

internal static class VCMetaManaged
{
	public static Guid HashForWinMD(string path)
	{
		using MD5 mD = MD5.Create();
		using PEReader peReader = new PEReader(File.OpenRead(path));
		MetadataReader metadataReader = peReader.GetMetadataReader();
		foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
		{
			TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
			Hash(mD, (int)typeDefinition.Attributes);
			Hash(mD, metadataReader.GetString(typeDefinition.Name));
			Hash(mD, metadataReader.GetString(typeDefinition.Namespace));
			Hash(mD, MetadataTokens.GetToken(typeDefinition.BaseType));
		}
		foreach (FieldDefinitionHandle fieldDefinition2 in metadataReader.FieldDefinitions)
		{
			System.Reflection.Metadata.FieldDefinition fieldDefinition = metadataReader.GetFieldDefinition(fieldDefinition2);
			FieldAttributes fieldAttributes = fieldDefinition.Attributes & FieldAttributes.FieldAccessMask;
			if (fieldAttributes == FieldAttributes.Public || fieldAttributes == FieldAttributes.Family)
			{
				Hash(mD, (int)fieldDefinition.Attributes);
				Hash(mD, metadataReader.GetString(fieldDefinition.Name));
				Hash(mD, metadataReader.GetBlobBytes(fieldDefinition.Signature));
			}
		}
		foreach (MethodDefinitionHandle methodDefinition2 in metadataReader.MethodDefinitions)
		{
			MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(methodDefinition2);
			MethodAttributes methodAttributes = methodDefinition.Attributes & MethodAttributes.MemberAccessMask;
			if (methodAttributes == MethodAttributes.Public || methodAttributes == MethodAttributes.Family)
			{
				Hash(mD, (int)methodDefinition.ImplAttributes);
				Hash(mD, (int)methodDefinition.Attributes);
				Hash(mD, metadataReader.GetString(methodDefinition.Name));
				Hash(mD, metadataReader.GetBlobBytes(methodDefinition.Signature));
			}
		}
		foreach (TypeDefinitionHandle typeDefinition3 in metadataReader.TypeDefinitions)
		{
			foreach (InterfaceImplementationHandle interfaceImplementation2 in metadataReader.GetTypeDefinition(typeDefinition3).GetInterfaceImplementations())
			{
				InterfaceImplementation interfaceImplementation = metadataReader.GetInterfaceImplementation(interfaceImplementation2);
				Hash(mD, MetadataTokens.GetToken(typeDefinition3));
				Hash(mD, MetadataTokens.GetToken(interfaceImplementation.Interface));
			}
		}
		foreach (MemberReferenceHandle memberReference2 in metadataReader.MemberReferences)
		{
			MemberReference memberReference = metadataReader.GetMemberReference(memberReference2);
			Hash(mD, metadataReader.GetString(memberReference.Name));
			Hash(mD, metadataReader.GetBlobBytes(memberReference.Signature));
		}
		foreach (EventDefinitionHandle eventDefinition2 in metadataReader.EventDefinitions)
		{
			EventDefinition eventDefinition = metadataReader.GetEventDefinition(eventDefinition2);
			Hash(mD, metadataReader.GetString(eventDefinition.Name));
			Hash(mD, MetadataTokens.GetToken(eventDefinition.Type));
		}
		foreach (PropertyDefinitionHandle propertyDefinition2 in metadataReader.PropertyDefinitions)
		{
			PropertyDefinition propertyDefinition = metadataReader.GetPropertyDefinition(propertyDefinition2);
			Hash(mD, metadataReader.GetString(propertyDefinition.Name));
			Hash(mD, metadataReader.GetBlobBytes(propertyDefinition.Signature));
		}
		TableIndex tableIndex = TableIndex.TypeSpec;
		int tableMetadataOffset = metadataReader.GetTableMetadataOffset(tableIndex);
		int tableRowSize = metadataReader.GetTableRowSize(tableIndex);
		int tableRowCount = metadataReader.GetTableRowCount(tableIndex);
		for (int i = 0; i < tableRowCount; i++)
		{
			TypeSpecificationHandle handle = MetadataTokens.TypeSpecificationHandle(i + 1);
			Hash(mD, metadataReader.GetBlobBytes(metadataReader.GetTypeSpecification(handle).Signature));
		}
		mD.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
		return new Guid(mD.Hash);
	}

	private static void Hash(MD5 md5, int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Hash(md5, bytes);
	}

	private static void Hash(MD5 md5, string value)
	{
		Encoding uTF = Encoding.UTF8;
		int byteCount = uTF.GetByteCount(value);
		byte[] array = ArrayPool<byte>.Shared.Rent(byteCount);
		try
		{
			uTF.GetBytes(value, 0, value.Length, array, 0);
			Hash(md5, array, byteCount);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	private static void Hash(MD5 md5, byte[] bytes)
	{
		md5.TransformBlock(bytes, 0, bytes.Length, null, 0);
	}

	private static void Hash(MD5 md5, byte[] bytes, int length)
	{
		md5.TransformBlock(bytes, 0, length, null, 0);
	}
}
