using System.Collections.Generic;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class XamlReflectionFactory : DefaultFactory
{
	private Dictionary<string, Dictionary<int, MetadataOnlyCommonType>> _scopeCache = new Dictionary<string, Dictionary<int, MetadataOnlyCommonType>>();

	public override MetadataOnlyCommonType CreateSimpleType(MetadataOnlyModule scope, Token tokenTypeDef)
	{
		if (!_scopeCache.TryGetValue(scope.FullyQualifiedName, out var value))
		{
			value = new Dictionary<int, MetadataOnlyCommonType>();
			_scopeCache.Add(scope.FullyQualifiedName, value);
		}
		if (!value.TryGetValue(tokenTypeDef.Value, out var value2))
		{
			value2 = base.CreateSimpleType(scope, tokenTypeDef);
			value.Add(tokenTypeDef.Value, value2);
		}
		return value2;
	}
}
