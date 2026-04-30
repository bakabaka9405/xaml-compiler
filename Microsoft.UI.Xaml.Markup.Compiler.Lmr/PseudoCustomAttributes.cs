using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Adds;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal static class PseudoCustomAttributes
{
	public const string TypeForwardedToAttributeName = "System.Runtime.CompilerServices.TypeForwardedToAttribute";

	public const string SerializableAttributeName = "System.SerializableAttribute";

	public static IEnumerable<CustomAttributeData> GetTypeForwardedToAttributes(MetadataOnlyAssembly assembly)
	{
		return GetTypeForwardedToAttributes(assembly.ManifestModuleInternal);
	}

	public static IEnumerable<CustomAttributeData> GetTypeForwardedToAttributes(MetadataOnlyModule manifestModule, Token token)
	{
		if (token.TokenType != TokenType.Assembly)
		{
			return new CustomAttributeData[0];
		}
		return GetTypeForwardedToAttributes(manifestModule);
	}

	public static IEnumerable<CustomAttributeData> GetTypeForwardedToAttributes(MetadataOnlyModule manifestModule)
	{
		ITypeUniverse itu = manifestModule.AssemblyResolver;
		Type argumentType = itu.GetBuiltInType(CorElementType.Type);
		Assembly systemAssembly = itu.GetSystemAssembly();
		Type attributeType = systemAssembly.GetType("System.Runtime.CompilerServices.TypeForwardedToAttribute", throwOnError: false, ignoreCase: false);
		if (attributeType == null)
		{
			yield break;
		}
		IEnumerable<UnresolvedTypeName> rawTypeForwardedToAttributes = GetRawTypeForwardedToAttributes(manifestModule);
		foreach (UnresolvedTypeName item2 in rawTypeForwardedToAttributes)
		{
			ConstructorInfo[] constructors = attributeType.GetConstructors();
			Type type = null;
			try
			{
				type = item2.ConvertToType(itu, manifestModule);
			}
			catch (UnresolvedAssemblyException)
			{
			}
			catch (TypeLoadException)
			{
			}
			if (type != null)
			{
				CustomAttributeTypedArgument item = new CustomAttributeTypedArgument(argumentType, type);
				List<CustomAttributeTypedArgument> list = new List<CustomAttributeTypedArgument>(1);
				list.Add(item);
				List<CustomAttributeNamedArgument> namedArguments = new List<CustomAttributeNamedArgument>(0);
				yield return new MetadataOnlyCustomAttributeData(constructors[0], list, namedArguments);
			}
		}
	}

	internal static IEnumerable<UnresolvedTypeName> GetRawTypeForwardedToAttributes(MetadataOnlyAssembly assembly)
	{
		return GetRawTypeForwardedToAttributes(assembly.ManifestModuleInternal);
	}

	internal static bool GetNextExportedType(ref HCORENUM hEnum, IMetadataAssemblyImport assemblyImport, StringBuilder typeName, out Token implementationToken)
	{
		implementationToken = Token.Nil;
		assemblyImport.EnumExportedTypes(ref hEnum, out var rExportedTypes, 1, out var cTokens);
		if (cTokens == 0)
		{
			return false;
		}
		assemblyImport.GetExportedTypeProps(rExportedTypes, null, 0, out var pchName, out var ptkImplementation, out var ptkTypeDef, out var pdwExportedTypeFlags);
		implementationToken = new Token(ptkImplementation);
		if (implementationToken.TokenType == TokenType.AssemblyRef)
		{
			typeName.Length = 0;
			typeName.EnsureCapacity(pchName);
			assemblyImport.GetExportedTypeProps(rExportedTypes, typeName, typeName.Capacity, out pchName, out ptkImplementation, out ptkTypeDef, out pdwExportedTypeFlags);
		}
		return true;
	}

	internal static IEnumerable<UnresolvedTypeName> GetRawTypeForwardedToAttributes(MetadataOnlyModule manifestModule)
	{
		HCORENUM hEnum = default(HCORENUM);
		IMetadataAssemblyImport assemblyImport = (IMetadataAssemblyImport)manifestModule.RawImport;
		try
		{
			StringBuilder typeName = StringBuilderPool.Get();
			Token implementationToken;
			while (GetNextExportedType(ref hEnum, assemblyImport, typeName, out implementationToken))
			{
				if (implementationToken.TokenType == TokenType.AssemblyRef)
				{
					AssemblyName assemblyNameFromRef = AssemblyNameHelper.GetAssemblyNameFromRef(implementationToken, manifestModule, assemblyImport);
					yield return new UnresolvedTypeName(typeName.ToString(), assemblyNameFromRef);
				}
			}
			StringBuilderPool.Release(ref typeName);
		}
		finally
		{
			hEnum.Close(assemblyImport);
		}
	}

	internal static UnresolvedTypeName GetRawTypeForwardedToAttribute(MetadataOnlyAssembly assembly, string fullname, bool ignoreCase)
	{
		return GetRawTypeForwardedToAttribute(assembly.ManifestModuleInternal, fullname, ignoreCase);
	}

	internal static UnresolvedTypeName GetRawTypeForwardedToAttribute(MetadataOnlyModule manifestModule, string fullname, bool ignoreCase)
	{
		HCORENUM hEnum = default(HCORENUM);
		IMetadataAssemblyImport metadataAssemblyImport = (IMetadataAssemblyImport)manifestModule.RawImport;
		if (string.IsNullOrEmpty(fullname))
		{
			return null;
		}
		UnresolvedTypeName result = null;
		try
		{
			StringBuilder builder = StringBuilderPool.Get();
			Token implementationToken;
			while (GetNextExportedType(ref hEnum, metadataAssemblyImport, builder, out implementationToken))
			{
				if (implementationToken.TokenType == TokenType.AssemblyRef && fullname.Length == builder.Length)
				{
					string text = builder.ToString();
					if (Utility.Compare(text, fullname, ignoreCase))
					{
						AssemblyName assemblyNameFromRef = AssemblyNameHelper.GetAssemblyNameFromRef(implementationToken, manifestModule, metadataAssemblyImport);
						result = new UnresolvedTypeName(text, assemblyNameFromRef);
						break;
					}
				}
			}
			StringBuilderPool.Release(ref builder);
			return result;
		}
		finally
		{
			hEnum.Close(metadataAssemblyImport);
		}
	}

	public static Type GetTypeFromTypeForwardToAttribute(CustomAttributeData data)
	{
		return (Type)data.ConstructorArguments[0].Value;
	}

	public static CustomAttributeData GetSerializableAttribute(MetadataOnlyModule module, Token token)
	{
		if (token.TokenType != TokenType.TypeDef)
		{
			return null;
		}
		module.RawImport.GetTypeDefProps(token.Value, null, 0, out var _, out var pdwTypeDefFlags, out var _);
		if ((pdwTypeDefFlags & TypeAttributes.Serializable) == 0)
		{
			return null;
		}
		return GetSerializableAttribute(module, isRequired: false);
	}

	internal static CustomAttributeData GetSerializableAttribute(MetadataOnlyModule module, bool isRequired)
	{
		Assembly systemAssembly = module.AssemblyResolver.GetSystemAssembly();
		Type type = systemAssembly.GetType("System.SerializableAttribute", isRequired, ignoreCase: false);
		if (type == null)
		{
			return null;
		}
		ConstructorInfo[] constructors = type.GetConstructors();
		List<CustomAttributeTypedArgument> typedArguments = new List<CustomAttributeTypedArgument>(0);
		List<CustomAttributeNamedArgument> namedArguments = new List<CustomAttributeNamedArgument>(0);
		return new MetadataOnlyCustomAttributeData(constructors[0], typedArguments, namedArguments);
	}
}
