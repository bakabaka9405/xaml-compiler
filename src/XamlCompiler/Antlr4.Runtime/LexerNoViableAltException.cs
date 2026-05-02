using System;
using System.Globalization;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

[Serializable]
public class LexerNoViableAltException : RecognitionException
{
	private const long serialVersionUID = -730999203913001726L;

	private readonly int startIndex;

	[Nullable]
	private readonly ATNConfigSet deadEndConfigs;

	public virtual int StartIndex => startIndex;

	[Nullable]
	public virtual ATNConfigSet DeadEndConfigs => deadEndConfigs;

	public override IIntStream InputStream => (ICharStream)base.InputStream;

	public LexerNoViableAltException(Lexer lexer, ICharStream input, int startIndex, ATNConfigSet deadEndConfigs)
		: base(lexer, input)
	{
		this.startIndex = startIndex;
		this.deadEndConfigs = deadEndConfigs;
	}

	public override string ToString()
	{
		string arg = string.Empty;
		if (startIndex >= 0 && startIndex < ((ICharStream)InputStream).Size)
		{
			arg = ((ICharStream)InputStream).GetText(Interval.Of(startIndex, startIndex));
			arg = Utils.EscapeWhitespace(arg, escapeSpaces: false);
		}
		return string.Format(CultureInfo.CurrentCulture, "{0}('{1}')", typeof(LexerNoViableAltException).Name, arg);
	}
}
