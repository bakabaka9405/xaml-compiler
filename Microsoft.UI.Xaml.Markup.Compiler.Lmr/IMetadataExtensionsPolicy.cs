using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal interface IMetadataExtensionsPolicy
{
	Type[] GetExtraArrayInterfaces(Type elementType);

	MethodInfo[] GetExtraArrayMethods(Type arrayType);

	ConstructorInfo[] GetExtraArrayConstructors(Type arrayType);

	ParameterInfo GetFakeParameterInfo(MemberInfo member, Type paramType, int position);

	IEnumerable<CustomAttributeData> GetPseudoCustomAttributes(MetadataOnlyModule module, Token token);

	Type TryTypeForwardResolution(MetadataOnlyAssembly assembly, string fullname, bool ignoreCase);
}
