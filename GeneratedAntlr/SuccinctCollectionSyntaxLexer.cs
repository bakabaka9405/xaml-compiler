using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

[GeneratedCode("ANTLR", "4.5.1")]
public class SuccinctCollectionSyntaxLexer : Lexer
{
	public const int SINGLE_QUOTE = 1;

	public const int DOUBLE_QUOTE = 2;

	public const int OPEN_SQUARE_BRACE = 3;

	public const int CLOSE_SQUARE_BRACE = 4;

	public const int CHARACTER = 5;

	public const int COMMA = 6;

	public const int BACKSLASH = 7;

	public const int RESERVED_SYMBOL = 8;

	public const int QUOTE = 9;

	public static string[] modeNames = new string[1] { "DEFAULT_MODE" };

	public static readonly string[] ruleNames = new string[9] { "SINGLE_QUOTE", "DOUBLE_QUOTE", "OPEN_SQUARE_BRACE", "CLOSE_SQUARE_BRACE", "CHARACTER", "COMMA", "BACKSLASH", "RESERVED_SYMBOL", "QUOTE" };

	private static readonly string[] _LiteralNames = new string[8] { null, "'''", "'\"'", "'['", "']'", null, "','", "'\\'" };

	private static readonly string[] _SymbolicNames = new string[10] { null, "SINGLE_QUOTE", "DOUBLE_QUOTE", "OPEN_SQUARE_BRACE", "CLOSE_SQUARE_BRACE", "CHARACTER", "COMMA", "BACKSLASH", "RESERVED_SYMBOL", "QUOTE" };

	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	public static readonly string _serializedATN = "\u0003а훑舆괭䐗껱趀ꫝ\u0002\v'\b\u0001\u0004\u0002\t\u0002\u0004\u0003\t\u0003\u0004\u0004\t\u0004\u0004\u0005\t\u0005\u0004\u0006\t\u0006\u0004\a\t\a\u0004\b\t\b\u0004\t\t\t\u0004\n\t\n\u0003\u0002\u0003\u0002\u0003\u0003\u0003\u0003\u0003\u0004\u0003\u0004\u0003\u0005\u0003\u0005\u0003\u0006\u0003\u0006\u0003\a\u0003\a\u0003\b\u0003\b\u0003\t\u0003\t\u0003\n\u0003\n\u0002\u0002\v\u0003\u0003\u0005\u0004\a\u0005\t\u0006\v\a\r\b\u000f\t\u0011\n\u0013\v\u0003\u0002\u0005\u0006\u0002$$))..]_\u0003\u0002]_\u0004\u0002$$))&\u0002\u0003\u0003\u0002\u0002\u0002\u0002\u0005\u0003\u0002\u0002\u0002\u0002\a\u0003\u0002\u0002\u0002\u0002\t\u0003\u0002\u0002\u0002\u0002\v\u0003\u0002\u0002\u0002\u0002\r\u0003\u0002\u0002\u0002\u0002\u000f\u0003\u0002\u0002\u0002\u0002\u0011\u0003\u0002\u0002\u0002\u0002\u0013\u0003\u0002\u0002\u0002\u0003\u0015\u0003\u0002\u0002\u0002\u0005\u0017\u0003\u0002\u0002\u0002\a\u0019\u0003\u0002\u0002\u0002\t\u001b\u0003\u0002\u0002\u0002\v\u001d\u0003\u0002\u0002\u0002\r\u001f\u0003\u0002\u0002\u0002\u000f!\u0003\u0002\u0002\u0002\u0011#\u0003\u0002\u0002\u0002\u0013%\u0003\u0002\u0002\u0002\u0015\u0016\a)\u0002\u0002\u0016\u0004\u0003\u0002\u0002\u0002\u0017\u0018\a$\u0002\u0002\u0018\u0006\u0003\u0002\u0002\u0002\u0019\u001a\a]\u0002\u0002\u001a\b\u0003\u0002\u0002\u0002\u001b\u001c\a_\u0002\u0002\u001c\n\u0003\u0002\u0002\u0002\u001d\u001e\n\u0002\u0002\u0002\u001e\f\u0003\u0002\u0002\u0002\u001f \a.\u0002\u0002 \u000e\u0003\u0002\u0002\u0002!\"\a^\u0002\u0002\"\u0010\u0003\u0002\u0002\u0002#$\t\u0003\u0002\u0002$\u0012\u0003\u0002\u0002\u0002%&\t\u0004\u0002\u0002&\u0014\u0003\u0002\u0002\u0002\u0003\u0002\u0002";

	public static readonly ATN _ATN = new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());

	[NotNull]
	public override IVocabulary Vocabulary => DefaultVocabulary;

	public override string GrammarFileName => "SuccinctCollectionSyntax.g4";

	public override string[] RuleNames => ruleNames;

	public override string[] ModeNames => modeNames;

	public override string SerializedAtn => _serializedATN;

	public SuccinctCollectionSyntaxLexer(ICharStream input)
		: base(input)
	{
		Interpreter = new LexerATNSimulator(this, _ATN);
	}
}
