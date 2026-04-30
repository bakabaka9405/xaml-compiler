using System;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public static class XamlMemberExtensions
{
	internal static bool IsDependencyProperty(this XamlMember instance)
	{
		if (instance is IXamlMemberMeta xamlMemberMeta)
		{
			return xamlMemberMeta?.IsDependencyProperty ?? false;
		}
		throw new ArgumentException("instance");
	}
}
