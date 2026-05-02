using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

public class XPathLexer : Lexer
{
	public const int TokenRef = 1;

	public const int RuleRef = 2;

	public const int Anywhere = 3;

	public const int Root = 4;

	public const int Wildcard = 5;

	public const int Bang = 6;

	public const int ID = 7;

	public const int String = 8;

	public static string[] modeNames = new string[1] { "DEFAULT_MODE" };

	public static readonly string[] ruleNames = new string[8] { "Anywhere", "Root", "Wildcard", "Bang", "ID", "NameChar", "NameStartChar", "String" };

	private static readonly string[] _LiteralNames = new string[7] { null, null, null, "'//'", "'/'", "'*'", "'!'" };

	private static readonly string[] _SymbolicNames = new string[9] { null, "TokenRef", "RuleRef", "Anywhere", "Root", "Wildcard", "Bang", "ID", "String" };

	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	public static readonly string _serializedATN = "\u0003а훑舆괭䐗껱趀ꫝ\u0002\n4\b\u0001\u0004\u0002\t\u0002\u0004\u0003\t\u0003\u0004\u0004\t\u0004\u0004\u0005\t\u0005\u0004\u0006\t\u0006\u0004\a\t\a\u0004\b\t\b\u0004\t\t\t\u0003\u0002\u0003\u0002\u0003\u0002\u0003\u0003\u0003\u0003\u0003\u0004\u0003\u0004\u0003\u0005\u0003\u0005\u0003\u0006\u0003\u0006\a\u0006\u001f\n\u0006\f\u0006\u000e\u0006\"\v\u0006\u0003\u0006\u0003\u0006\u0003\a\u0003\a\u0005\a(\n\a\u0003\b\u0003\b\u0003\t\u0003\t\a\t.\n\t\f\t\u000e\t1\v\t\u0003\t\u0003\t\u0003/\u0002\n\u0003\u0005\u0005\u0006\a\a\t\b\v\t\r\u0002\u000f\u0002\u0011\n\u0003\u0002\u0004\a\u00022;aa¹¹\u0302ͱ⁁⁂\u000f\u0002C\\c|ÂØÚøú\u0301ͲͿ\u0381\u2001\u200e\u200f\u2072↑Ⰲ⿱〃\ud801車\ufdd1ﷲ\uffff4\u0002\u0003\u0003\u0002\u0002\u0002\u0002\u0005\u0003\u0002\u0002\u0002\u0002\a\u0003\u0002\u0002\u0002\u0002\t\u0003\u0002\u0002\u0002\u0002\v\u0003\u0002\u0002\u0002\u0002\u0011\u0003\u0002\u0002\u0002\u0003\u0013\u0003\u0002\u0002\u0002\u0005\u0016\u0003\u0002\u0002\u0002\a\u0018\u0003\u0002\u0002\u0002\t\u001a\u0003\u0002\u0002\u0002\v\u001c\u0003\u0002\u0002\u0002\r'\u0003\u0002\u0002\u0002\u000f)\u0003\u0002\u0002\u0002\u0011+\u0003\u0002\u0002\u0002\u0013\u0014\a1\u0002\u0002\u0014\u0015\a1\u0002\u0002\u0015\u0004\u0003\u0002\u0002\u0002\u0016\u0017\a1\u0002\u0002\u0017\u0006\u0003\u0002\u0002\u0002\u0018\u0019\a,\u0002\u0002\u0019\b\u0003\u0002\u0002\u0002\u001a\u001b\a#\u0002\u0002\u001b\n\u0003\u0002\u0002\u0002\u001c \u0005\u000f\b\u0002\u001d\u001f\u0005\r\a\u0002\u001e\u001d\u0003\u0002\u0002\u0002\u001f\"\u0003\u0002\u0002\u0002 \u001e\u0003\u0002\u0002\u0002 !\u0003\u0002\u0002\u0002!#\u0003\u0002\u0002\u0002\" \u0003\u0002\u0002\u0002#$\b\u0006\u0002\u0002$\f\u0003\u0002\u0002\u0002%(\u0005\u000f\b\u0002&(\t\u0002\u0002\u0002'%\u0003\u0002\u0002\u0002'&\u0003\u0002\u0002\u0002(\u000e\u0003\u0002\u0002\u0002)*\t\u0003\u0002\u0002*\u0010\u0003\u0002\u0002\u0002+/\a)\u0002\u0002,.\v\u0002\u0002\u0002-,\u0003\u0002\u0002\u0002.1\u0003\u0002\u0002\u0002/0\u0003\u0002\u0002\u0002/-\u0003\u0002\u0002\u000202\u0003\u0002\u0002\u00021/\u0003\u0002\u0002\u000223\a)\u0002\u00023\u0012\u0003\u0002\u0002\u0002\u0006\u0002 '/\u0003\u0003\u0006\u0002";

	public static readonly ATN _ATN = new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());

	[NotNull]
	public override IVocabulary Vocabulary => DefaultVocabulary;

	public override string GrammarFileName => "XPathLexer.g4";

	public override string[] RuleNames => ruleNames;

	public override string[] ModeNames => modeNames;

	public override string SerializedAtn => _serializedATN;

	public XPathLexer(ICharStream input)
		: base(input)
	{
		Interpreter = new LexerATNSimulator(this, _ATN);
	}

	public override void Action(RuleContext _localctx, int ruleIndex, int actionIndex)
	{
		if (ruleIndex == 4)
		{
			ID_action(_localctx, actionIndex);
		}
	}

	private void ID_action(RuleContext _localctx, int actionIndex)
	{
		if (actionIndex == 0)
		{
			string text = Text;
			if (char.IsUpper(text[0]))
			{
				Type = 1;
			}
			else
			{
				Type = 2;
			}
		}
	}
}
