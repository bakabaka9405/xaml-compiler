using System.Windows.Markup;
using System.Xaml;

namespace MS.Internal.Xaml.Context;

internal static class ContextServices
{
	public static object GetTargetProperty(ObjectWriterContext xamlContext)
	{
		if (xamlContext.ParentProperty is IProvideValueTarget provideValueTarget)
		{
			return provideValueTarget.TargetProperty;
		}
		XamlMember parentProperty = xamlContext.ParentProperty;
		if (parentProperty == null)
		{
			return null;
		}
		if (parentProperty.IsAttachable)
		{
			return parentProperty.Setter;
		}
		return parentProperty.UnderlyingMember;
	}
}
