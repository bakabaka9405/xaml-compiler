using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataExtensionsPolicy20 : IMetadataExtensionsPolicy
{
	protected ITypeUniverse m_universe;

	public MetadataExtensionsPolicy20(ITypeUniverse u)
	{
		m_universe = u;
	}

	public virtual Type[] GetExtraArrayInterfaces(Type elementType)
	{
		if (elementType.IsPointer)
		{
			return Type.EmptyTypes;
		}
		Type[] typeArguments = new Type[1] { elementType };
		return new Type[3]
		{
			m_universe.GetTypeXFromName("System.Collections.Generic.IList`1").MakeGenericType(typeArguments),
			m_universe.GetTypeXFromName("System.Collections.Generic.ICollection`1").MakeGenericType(typeArguments),
			m_universe.GetTypeXFromName("System.Collections.Generic.IEnumerable`1").MakeGenericType(typeArguments)
		};
	}

	public virtual MethodInfo[] GetExtraArrayMethods(Type arrayType)
	{
		return new MethodInfo[3]
		{
			new ArrayFabricatedGetMethodInfo(arrayType),
			new ArrayFabricatedSetMethodInfo(arrayType),
			new ArrayFabricatedAddressMethodInfo(arrayType)
		};
	}

	public virtual ConstructorInfo[] GetExtraArrayConstructors(Type arrayType)
	{
		int arrayRank = arrayType.GetArrayRank();
		return new ConstructorInfo[2]
		{
			new ArrayFabricatedConstructorInfo(arrayType, arrayRank),
			new ArrayFabricatedConstructorInfo(arrayType, arrayRank * 2)
		};
	}

	public virtual ParameterInfo GetFakeParameterInfo(MemberInfo member, Type paramType, int position)
	{
		return new SimpleParameterInfo(member, paramType, position);
	}

	public virtual IEnumerable<CustomAttributeData> GetPseudoCustomAttributes(MetadataOnlyModule module, Token token)
	{
		List<CustomAttributeData> list = new List<CustomAttributeData>();
		IEnumerable<CustomAttributeData> typeForwardedToAttributes = PseudoCustomAttributes.GetTypeForwardedToAttributes(module, token);
		if (typeForwardedToAttributes.Any())
		{
			list.AddRange(typeForwardedToAttributes);
		}
		CustomAttributeData serializableAttribute = PseudoCustomAttributes.GetSerializableAttribute(module, token);
		if (serializableAttribute != null)
		{
			list.Add(serializableAttribute);
		}
		return list;
	}

	public virtual Type TryTypeForwardResolution(MetadataOnlyAssembly assembly, string fullname, bool ignoreCase)
	{
		UnresolvedTypeName rawTypeForwardedToAttribute = PseudoCustomAttributes.GetRawTypeForwardedToAttribute(assembly, fullname, ignoreCase);
		if (rawTypeForwardedToAttribute != null)
		{
			Type result = null;
			try
			{
				result = rawTypeForwardedToAttribute.ConvertToType(assembly.TypeUniverse, assembly.ManifestModuleInternal);
			}
			catch (UnresolvedAssemblyException)
			{
			}
			catch (TypeLoadException)
			{
			}
			return result;
		}
		return null;
	}
}
