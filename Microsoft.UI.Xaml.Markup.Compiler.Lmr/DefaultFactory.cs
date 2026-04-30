using System;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class DefaultFactory : IReflectionFactory
{
	public virtual MetadataOnlyCommonType CreateSimpleType(MetadataOnlyModule scope, Token tokenTypeDef)
	{
		return new MetadataOnlyTypeDef(scope, tokenTypeDef);
	}

	public virtual MetadataOnlyCommonType CreateGenericType(MetadataOnlyModule scope, Token tokenTypeDef, Type[] typeArgs)
	{
		return new MetadataOnlyTypeDef(scope, tokenTypeDef, typeArgs);
	}

	public virtual MetadataOnlyCommonType CreateArrayType(MetadataOnlyCommonType elementType, int rank)
	{
		return new MetadataOnlyArrayType(elementType, rank);
	}

	public virtual MetadataOnlyCommonType CreateVectorType(MetadataOnlyCommonType elementType)
	{
		return new MetadataOnlyVectorType(elementType);
	}

	public virtual MetadataOnlyCommonType CreateByRefType(MetadataOnlyCommonType type)
	{
		return new MetadataOnlyModifiedType(type, "&");
	}

	public virtual MetadataOnlyCommonType CreatePointerType(MetadataOnlyCommonType type)
	{
		return new MetadataOnlyModifiedType(type, "*");
	}

	public virtual MetadataOnlyTypeVariable CreateTypeVariable(MetadataOnlyModule resolver, Token typeVariableToken)
	{
		return new MetadataOnlyTypeVariable(resolver, typeVariableToken);
	}

	public virtual MetadataOnlyFieldInfo CreateField(MetadataOnlyModule resolver, Token fieldDefToken, Type[] typeArgs, Type[] methodArgs)
	{
		return new MetadataOnlyFieldInfo(resolver, fieldDefToken, typeArgs, methodArgs);
	}

	public virtual MetadataOnlyPropertyInfo CreatePropertyInfo(MetadataOnlyModule resolver, Token propToken, Type[] typeArgs, Type[] methodArgs)
	{
		return new MetadataOnlyPropertyInfo(resolver, propToken, typeArgs, methodArgs);
	}

	public virtual MetadataOnlyEventInfo CreateEventInfo(MetadataOnlyModule resolver, Token eventToken, Type[] typeArgs, Type[] methodArgs)
	{
		return new MetadataOnlyEventInfo(resolver, eventToken, typeArgs, methodArgs);
	}

	public virtual MetadataOnlyConstructorInfo CreateConstructorInfo(MethodBase method)
	{
		return new MetadataOnlyConstructorInfo(method);
	}

	public virtual MetadataOnlyMethodInfo CreateMethodInfo(MetadataOnlyMethodInfo method)
	{
		return new MetadataOnlyMethodInfo(method);
	}

	public virtual MethodBase CreateMethodOrConstructor(MetadataOnlyModule resolver, Token methodDef, Type[] typeArgs, Type[] methodArgs)
	{
		MetadataOnlyMethodInfo metadataOnlyMethodInfo = new MetadataOnlyMethodInfo(resolver, methodDef, typeArgs, methodArgs);
		if (IsRawConstructor(metadataOnlyMethodInfo))
		{
			return CreateConstructorInfo(metadataOnlyMethodInfo);
		}
		return CreateMethodInfo(metadataOnlyMethodInfo);
	}

	private static bool IsRawConstructor(MethodInfo m)
	{
		if ((m.Attributes & MethodAttributes.RTSpecialName) == 0)
		{
			return false;
		}
		string name = m.Name;
		if (name.Equals(ConstructorInfo.ConstructorName, StringComparison.Ordinal))
		{
			return true;
		}
		if (name.Equals(ConstructorInfo.TypeConstructorName, StringComparison.Ordinal))
		{
			return true;
		}
		return false;
	}

	public virtual bool TryCreateMethodBody(MetadataOnlyMethodInfo method, ref MethodBody body)
	{
		return false;
	}

	public virtual Type CreateTypeRef(MetadataOnlyModule scope, Token tokenTypeRef)
	{
		return new MetadataOnlyTypeReference(scope, tokenTypeRef);
	}

	public virtual Type CreateSignatureTypeRef(MetadataOnlyModule scope, Token tokenTypeRef, CorElementType elemType)
	{
		return new MetadataOnlySignatureTypeReference(scope, tokenTypeRef, elemType);
	}

	public virtual Type CreateTypeSpec(MetadataOnlyModule scope, Token tokenTypeSpec, Type[] typeArgs, Type[] methodArgs)
	{
		return new TypeSpec(scope, tokenTypeSpec, typeArgs, methodArgs);
	}
}
