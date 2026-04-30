using System;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal interface IReflectionFactory
{
	MetadataOnlyCommonType CreateSimpleType(MetadataOnlyModule scope, Token tokenTypeDef);

	MetadataOnlyCommonType CreateGenericType(MetadataOnlyModule scope, Token tokenTypeDef, Type[] typeArgs);

	MetadataOnlyCommonType CreateArrayType(MetadataOnlyCommonType elementType, int rank);

	MetadataOnlyCommonType CreateVectorType(MetadataOnlyCommonType elementType);

	MetadataOnlyCommonType CreateByRefType(MetadataOnlyCommonType type);

	MetadataOnlyCommonType CreatePointerType(MetadataOnlyCommonType type);

	MetadataOnlyTypeVariable CreateTypeVariable(MetadataOnlyModule resolver, Token typeVariableToken);

	MetadataOnlyFieldInfo CreateField(MetadataOnlyModule resolver, Token fieldDefToken, Type[] typeArgs, Type[] methodArgs);

	MetadataOnlyPropertyInfo CreatePropertyInfo(MetadataOnlyModule resolver, Token propToken, Type[] typeArgs, Type[] methodArgs);

	MetadataOnlyEventInfo CreateEventInfo(MetadataOnlyModule resolver, Token eventToken, Type[] typeArgs, Type[] methodArgs);

	MethodBase CreateMethodOrConstructor(MetadataOnlyModule resolver, Token methodToken, Type[] typeArgs, Type[] methodArgs);

	bool TryCreateMethodBody(MetadataOnlyMethodInfo method, ref MethodBody body);

	Type CreateTypeRef(MetadataOnlyModule scope, Token tokenTypeRef);

	Type CreateSignatureTypeRef(MetadataOnlyModule scope, Token tokenTypeRef, CorElementType elementType);

	Type CreateTypeSpec(MetadataOnlyModule scope, Token tokenTypeRef, Type[] typeArgs, Type[] methodArgs);
}
