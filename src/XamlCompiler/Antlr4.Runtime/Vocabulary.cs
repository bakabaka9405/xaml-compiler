using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public class Vocabulary : IVocabulary
{
	private static readonly string[] EmptyNames = new string[0];

	[NotNull]
	public static readonly Vocabulary EmptyVocabulary = new Vocabulary(EmptyNames, EmptyNames, EmptyNames);

	[NotNull]
	private readonly string[] literalNames;

	[NotNull]
	private readonly string[] symbolicNames;

	[NotNull]
	private readonly string[] displayNames;

	public Vocabulary(string[] literalNames, string[] symbolicNames)
		: this(literalNames, symbolicNames, null)
	{
	}

	public Vocabulary(string[] literalNames, string[] symbolicNames, string[] displayNames)
	{
		this.literalNames = ((literalNames != null) ? literalNames : EmptyNames);
		this.symbolicNames = ((symbolicNames != null) ? symbolicNames : EmptyNames);
		this.displayNames = ((displayNames != null) ? displayNames : EmptyNames);
	}

	[return: Nullable]
	public virtual string GetLiteralName(int tokenType)
	{
		if (tokenType >= 0 && tokenType < literalNames.Length)
		{
			return literalNames[tokenType];
		}
		return null;
	}

	[return: Nullable]
	public virtual string GetSymbolicName(int tokenType)
	{
		if (tokenType >= 0 && tokenType < symbolicNames.Length)
		{
			return symbolicNames[tokenType];
		}
		if (tokenType == -1)
		{
			return "EOF";
		}
		return null;
	}

	[return: NotNull]
	public virtual string GetDisplayName(int tokenType)
	{
		if (tokenType >= 0 && tokenType < displayNames.Length)
		{
			string text = displayNames[tokenType];
			if (text != null)
			{
				return text;
			}
		}
		string literalName = GetLiteralName(tokenType);
		if (literalName != null)
		{
			return literalName;
		}
		string symbolicName = GetSymbolicName(tokenType);
		if (symbolicName != null)
		{
			return symbolicName;
		}
		return tokenType.ToString();
	}
}
