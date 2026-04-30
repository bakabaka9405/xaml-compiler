using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public static class StringExtensions
{
	private static readonly Regex MemberFriendlyNameRegex = new Regex("[^\\w]");

	public static string GetMemberFriendlyName(this string instance)
	{
		return MemberFriendlyNameRegex.Replace(instance, "_");
	}

	public static bool IsConditionalNamespace(this string instance)
	{
		return instance.Contains("?");
	}

	internal static bool HasAtLeastTwo(this string instance, char character)
	{
		if (instance.Contains(character))
		{
			return instance.IndexOf(character) != instance.LastIndexOf(character);
		}
		return false;
	}

	public static bool HasUsingPrefix(this string instance)
	{
		return instance.StartsWith("using:");
	}

	public static string StripUsingPrefix(this string instance)
	{
		if (instance.HasUsingPrefix())
		{
			return instance.Substring("using:".Length);
		}
		return instance;
	}
}
