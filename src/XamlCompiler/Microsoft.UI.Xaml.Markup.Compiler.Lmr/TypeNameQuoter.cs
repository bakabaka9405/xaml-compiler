using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal static class TypeNameQuoter
{
	private static char[] specialCharacters = new char[7] { '\\', '[', ']', ',', '+', '&', '*' };

	internal static string GetQuotedTypeName(string name)
	{
		if (name.IndexOfAny(specialCharacters) == -1)
		{
			return name;
		}
		StringBuilder builder = StringBuilderPool.Get();
		for (int i = 0; i < name.Length; i++)
		{
			if (Contains(specialCharacters, name[i]))
			{
				builder.Append('\\');
			}
			builder.Append(name[i]);
		}
		string result = builder.ToString();
		StringBuilderPool.Release(ref builder);
		return result;
	}

	private static bool Contains(char[] This, char ch)
	{
		foreach (char c in This)
		{
			if (c == ch)
			{
				return true;
			}
		}
		return false;
	}
}
