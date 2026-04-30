using System;
using System.CodeDom.Compiler;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

[GeneratedCode("ANTLR", "4.5.3")]
public class ConditionalNamespaceLexer : Lexer
{
	public const int T__0 = 1;

	public const int T__1 = 2;

	public const int T__2 = 3;

	public const int T__3 = 4;

	public const int T__4 = 5;

	public const int T__5 = 6;

	public const int T__6 = 7;

	public const int T__7 = 8;

	public const int T__8 = 9;

	public const int WS = 10;

	public const int ESCAPEDQUOTE = 11;

	public const int QUOTE = 12;

	public const int DOUBLE_QUOTE = 13;

	public const int TargetPlatformString = 14;

	public const int PlatformUWP = 15;

	public const int PlatformiOS = 16;

	public const int PlatformAndroid = 17;

	public const int Digits = 18;

	public const int QuotedString = 19;

	public const int IDENTIFIER = 20;

	public static string[] modeNames = new string[1] { "DEFAULT_MODE" };

	public static readonly string[] ruleNames = new string[40]
	{
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", "WS",
		"ESCAPEDQUOTE", "QUOTE", "DOUBLE_QUOTE", "TargetPlatformString", "PlatformUWP", "PlatformiOS", "PlatformAndroid", "Digits", "QuotedString", "IDENTIFIER",
		"Available_identifier", "Identifier_or_keyword", "Identifier_start_character", "Identifier_part_character", "Letter_character", "Combining_character", "Decimal_digit_character", "Connecting_character", "Formatting_character", "UNICODE_CLASS_LU",
		"UNICODE_CLASS_LL", "UNICODE_CLASS_LT", "UNICODE_CLASS_LM", "UNICODE_CLASS_LO", "UNICODE_CLASS_NL", "UNICODE_CLASS_MN", "UNICODE_CLASS_MC", "UNICODE_CLASS_CF", "UNICODE_CLASS_PC", "UNICODE_CLASS_ND"
	};

	private static readonly string[] _LiteralNames = new string[18]
	{
		null, "'?'", "'/'", "':'", "'-'", "'.'", "'('", "','", "')'", "';'",
		null, null, "'''", "'\"'", "'TargetPlatform'", "'UWP'", "'iOS'", "'Android'"
	};

	private static readonly string[] _SymbolicNames = new string[21]
	{
		null, null, null, null, null, null, null, null, null, null,
		"WS", "ESCAPEDQUOTE", "QUOTE", "DOUBLE_QUOTE", "TargetPlatformString", "PlatformUWP", "PlatformiOS", "PlatformAndroid", "Digits", "QuotedString",
		"IDENTIFIER"
	};

	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	private static string _serializedATN = _serializeATN();

	public static readonly ATN _ATN = new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());

	[NotNull]
	public override IVocabulary Vocabulary => DefaultVocabulary;

	public override string GrammarFileName => "ConditionalNamespace.g4";

	public override string[] RuleNames => ruleNames;

	public override string[] ModeNames => modeNames;

	public override string SerializedAtn => _serializedATN;

	public ConditionalNamespaceLexer(ICharStream input)
		: base(input)
	{
		Interpreter = new LexerATNSimulator(this, _ATN);
	}

	private static string _serializeATN()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("\u0003а훑舆괭䐗껱趀ꫝ\u0002\u0016");
		stringBuilder.Append("î\b\u0001\u0004\u0002\t\u0002\u0004\u0003\t\u0003\u0004\u0004\t\u0004\u0004\u0005\t\u0005\u0004\u0006");
		stringBuilder.Append("\t\u0006\u0004\a\t\a\u0004\b\t\b\u0004\t\t\t\u0004\n\t\n\u0004\v\t\v\u0004\f\t\f");
		stringBuilder.Append("\u0004\r\t\r\u0004\u000e\t\u000e\u0004\u000f\t\u000f\u0004\u0010\t\u0010\u0004\u0011\t\u0011\u0004");
		stringBuilder.Append("\u0012\t\u0012\u0004\u0013\t\u0013\u0004\u0014\t\u0014\u0004\u0015\t\u0015\u0004\u0016\t\u0016");
		stringBuilder.Append("\u0004\u0017\t\u0017\u0004\u0018\t\u0018\u0004\u0019\t\u0019\u0004\u001a\t\u001a\u0004\u001b");
		stringBuilder.Append("\t\u001b\u0004\u001c\t\u001c\u0004\u001d\t\u001d\u0004\u001e\t\u001e\u0004\u001f\t\u001f\u0004");
		stringBuilder.Append(" \t \u0004!\t!\u0004\"\t\"\u0004#\t#\u0004$\t$\u0004%\t%\u0004&\t&\u0004'\t'\u0004");
		stringBuilder.Append("(\t(\u0004)\t)\u0003\u0002\u0003\u0002\u0003\u0003\u0003\u0003\u0003\u0004\u0003\u0004\u0003\u0005\u0003\u0005");
		stringBuilder.Append("\u0003\u0006\u0003\u0006\u0003\a\u0003\a\u0003\b\u0003\b\u0003\t\u0003\t\u0003\n\u0003\n\u0003\v\u0006");
		stringBuilder.Append("\vg\n\v\r\v\u000e\vh\u0003\v\u0003\v\u0003\f\u0003\f\u0003\f\u0003\f\u0005\fq\n\f\u0003");
		stringBuilder.Append("\r\u0003\r\u0003\u000e\u0003\u000e\u0003\u000f\u0003\u000f\u0003\u000f\u0003\u000f\u0003\u000f\u0003\u000f\u0003\u000f");
		stringBuilder.Append("\u0003\u000f\u0003\u000f\u0003\u000f\u0003\u000f\u0003\u000f\u0003\u000f\u0003\u000f\u0003\u000f\u0003\u0010\u0003\u0010");
		stringBuilder.Append("\u0003\u0010\u0003\u0010\u0003\u0011\u0003\u0011\u0003\u0011\u0003\u0011\u0003\u0012\u0003\u0012\u0003");
		stringBuilder.Append("\u0012\u0003\u0012\u0003\u0012\u0003\u0012\u0003\u0012\u0003\u0012\u0003\u0013\u0006\u0013\u0097\n");
		stringBuilder.Append("\u0013\r\u0013\u000e\u0013\u0098\u0003\u0014\u0003\u0014\u0003\u0014\a\u0014\u009e\n\u0014\f");
		stringBuilder.Append("\u0014\u000e\u0014¡\v\u0014\u0003\u0014\u0003\u0014\u0003\u0014\u0003\u0014\u0003\u0014\a\u0014");
		stringBuilder.Append("\u00a8\n\u0014\f\u0014\u000e\u0014«\v\u0014\u0003\u0014\u0003\u0014\u0005\u0014\u00af\n");
		stringBuilder.Append("\u0014\u0003\u0015\u0003\u0015\u0003\u0016\u0003\u0016\u0003\u0017\u0003\u0017\a\u0017·\n\u0017");
		stringBuilder.Append("\f\u0017\u000e\u0017º\v\u0017\u0003\u0018\u0003\u0018\u0005\u0018¾\n\u0018\u0003\u0019");
		stringBuilder.Append("\u0003\u0019\u0003\u0019\u0003\u0019\u0003\u0019\u0005\u0019Å\n\u0019\u0003\u001a\u0003\u001a");
		stringBuilder.Append("\u0003\u001a\u0003\u001a\u0003\u001a\u0003\u001a\u0005\u001aÍ\n\u001a\u0003\u001b\u0003\u001b");
		stringBuilder.Append("\u0005\u001bÑ\n\u001b\u0003\u001c\u0003\u001c\u0003\u001d\u0003\u001d\u0003\u001e\u0003\u001e");
		stringBuilder.Append("\u0003\u001f\u0003\u001f\u0003 \u0003 \u0003!\u0003!\u0003\"\u0003\"\u0003#\u0003#\u0003$\u0003$\u0003");
		stringBuilder.Append("%\u0003%\u0003&\u0003&\u0003'\u0003'\u0003(\u0003(\u0003)\u0003)\u0004\u009f©\u0002*\u0003\u0003");
		stringBuilder.Append("\u0005\u0004\a\u0005\t\u0006\v\a\r\b\u000f\t\u0011\n\u0013\v\u0015\f\u0017\r\u0019\u000e");
		stringBuilder.Append("\u001b\u000f\u001d\u0010\u001f\u0011!\u0012#\u0013%\u0014'\u0015)\u0016+\u0002-\u0002/\u0002");
		stringBuilder.Append("1\u00023\u00025\u00027\u00029\u0002;\u0002=\u0002?\u0002A\u0002C\u0002");
		stringBuilder.Append("E\u0002G\u0002I\u0002K\u0002M\u0002O\u0002Q\u0002\u0003\u0002\n\u0004\u0002\v\v\"\"\u0004\u0002");
		stringBuilder.Append("C\\Âà\u0006\u0002ǇǇǊǊǍǍǴǴ");
		stringBuilder.Append("\u0005\u0002ƽƽǂǅʖʖ\u0004\u0002ᛰᛲⅢ");
		stringBuilder.Append("ⅱ\u0005\u0002अअ\u0940\u0942\u094b\u094e\u0005\u0002\u00af\u00af\u0602");
		stringBuilder.Append("\u0605\u06df\u06df\b\u0002aa⁁⁂⁖⁖︵︶");
		stringBuilder.Append("\ufe4f﹑ａａí\u0002\u0003\u0003\u0002\u0002\u0002\u0002\u0005\u0003\u0002\u0002");
		stringBuilder.Append("\u0002\u0002\a\u0003\u0002\u0002\u0002\u0002\t\u0003\u0002\u0002\u0002\u0002\v\u0003\u0002\u0002\u0002\u0002\r");
		stringBuilder.Append("\u0003\u0002\u0002\u0002\u0002\u000f\u0003\u0002\u0002\u0002\u0002\u0011\u0003\u0002\u0002\u0002\u0002\u0013\u0003");
		stringBuilder.Append("\u0002\u0002\u0002\u0002\u0015\u0003\u0002\u0002\u0002\u0002\u0017\u0003\u0002\u0002\u0002\u0002\u0019\u0003\u0002");
		stringBuilder.Append("\u0002\u0002\u0002\u001b\u0003\u0002\u0002\u0002\u0002\u001d\u0003\u0002\u0002\u0002\u0002\u001f\u0003\u0002\u0002");
		stringBuilder.Append("\u0002\u0002!\u0003\u0002\u0002\u0002\u0002#\u0003\u0002\u0002\u0002\u0002%\u0003\u0002\u0002\u0002\u0002'\u0003");
		stringBuilder.Append("\u0002\u0002\u0002\u0002)\u0003\u0002\u0002\u0002\u0003S\u0003\u0002\u0002\u0002\u0005U\u0003\u0002\u0002\u0002\a");
		stringBuilder.Append("W\u0003\u0002\u0002\u0002\tY\u0003\u0002\u0002\u0002\v[\u0003\u0002\u0002\u0002\r]\u0003\u0002\u0002\u0002\u000f");
		stringBuilder.Append("_\u0003\u0002\u0002\u0002\u0011a\u0003\u0002\u0002\u0002\u0013c\u0003\u0002\u0002\u0002\u0015f");
		stringBuilder.Append("\u0003\u0002\u0002\u0002\u0017p\u0003\u0002\u0002\u0002\u0019r\u0003\u0002\u0002\u0002\u001bt\u0003\u0002\u0002");
		stringBuilder.Append("\u0002\u001dv\u0003\u0002\u0002\u0002\u001f\u0085\u0003\u0002\u0002\u0002!\u0089\u0003\u0002\u0002\u0002#\u008d");
		stringBuilder.Append("\u0003\u0002\u0002\u0002%\u0096\u0003\u0002\u0002\u0002'®\u0003\u0002\u0002\u0002)°\u0003\u0002\u0002");
		stringBuilder.Append("\u0002+²\u0003\u0002\u0002\u0002-\u00b4\u0003\u0002\u0002\u0002/½\u0003\u0002\u0002\u00021Ä");
		stringBuilder.Append("\u0003\u0002\u0002\u00023Ì\u0003\u0002\u0002\u00025Ð\u0003\u0002\u0002\u00027Ò");
		stringBuilder.Append("\u0003\u0002\u0002\u00029Ô\u0003\u0002\u0002\u0002;Ö\u0003\u0002\u0002\u0002=Ø\u0003\u0002");
		stringBuilder.Append("\u0002\u0002?Ú\u0003\u0002\u0002\u0002AÜ\u0003\u0002\u0002\u0002CÞ\u0003\u0002\u0002");
		stringBuilder.Append("\u0002Eà\u0003\u0002\u0002\u0002Gâ\u0003\u0002\u0002\u0002Iä\u0003\u0002\u0002\u0002Kæ");
		stringBuilder.Append("\u0003\u0002\u0002\u0002Mè\u0003\u0002\u0002\u0002Oê\u0003\u0002\u0002\u0002Qì\u0003\u0002\u0002");
		stringBuilder.Append("\u0002ST\aA\u0002\u0002T\u0004\u0003\u0002\u0002\u0002UV\a1\u0002\u0002V\u0006\u0003\u0002\u0002");
		stringBuilder.Append("\u0002WX\a<\u0002\u0002X\b\u0003\u0002\u0002\u0002YZ\a/\u0002\u0002Z\n\u0003\u0002\u0002\u0002[\\\a");
		stringBuilder.Append("0\u0002\u0002\\\f\u0003\u0002\u0002\u0002]^\a*\u0002\u0002^\u000e\u0003\u0002\u0002\u0002_`\a.\u0002");
		stringBuilder.Append("\u0002`\u0010\u0003\u0002\u0002\u0002ab\a+\u0002\u0002b\u0012\u0003\u0002\u0002\u0002c");
		stringBuilder.Append("d\a=\u0002\u0002d\u0014\u0003\u0002\u0002\u0002eg\t\u0002\u0002\u0002fe\u0003");
		stringBuilder.Append("\u0002\u0002\u0002gh\u0003\u0002\u0002\u0002hf\u0003\u0002\u0002\u0002hi\u0003\u0002\u0002\u0002ij\u0003");
		stringBuilder.Append("\u0002\u0002\u0002jk\b\v\u0002\u0002k\u0016\u0003\u0002\u0002\u0002lm\a`\u0002\u0002mq\a$\u0002\u0002");
		stringBuilder.Append("no\a`\u0002\u0002oq\a)\u0002\u0002pl\u0003\u0002\u0002\u0002pn\u0003\u0002\u0002\u0002q\u0018\u0003\u0002");
		stringBuilder.Append("\u0002\u0002rs\a)\u0002\u0002s\u001a\u0003\u0002\u0002\u0002tu\a$\u0002\u0002u\u001c\u0003\u0002\u0002");
		stringBuilder.Append("\u0002vw\aV\u0002\u0002wx\ac\u0002\u0002xy\at\u0002\u0002yz\ai\u0002\u0002z{\ag\u0002\u0002");
		stringBuilder.Append("{|\av\u0002\u0002|}\aR\u0002\u0002}~\an\u0002\u0002~\u007f\ac\u0002\u0002\u007f\u0080\a");
		stringBuilder.Append("v\u0002\u0002\u0080\u0081\ah\u0002\u0002\u0081\u0082\aq\u0002\u0002\u0082\u0083\at\u0002\u0002\u0083");
		stringBuilder.Append("\u0084\ao\u0002\u0002\u0084\u001e\u0003\u0002\u0002\u0002\u0085\u0086\aW\u0002\u0002\u0086\u0087\a");
		stringBuilder.Append("Y\u0002\u0002\u0087\u0088\aR\u0002\u0002\u0088 \u0003\u0002\u0002\u0002\u0089\u008a\ak\u0002\u0002\u008a");
		stringBuilder.Append("\u008b\aQ\u0002\u0002\u008b\u008c\aU\u0002\u0002\u008c\"\u0003\u0002\u0002\u0002\u008d\u008e\aC");
		stringBuilder.Append("\u0002\u0002\u008e\u008f\ap\u0002\u0002\u008f\u0090\af\u0002\u0002\u0090\u0091\at\u0002\u0002");
		stringBuilder.Append("\u0091\u0092\aq\u0002\u0002\u0092\u0093\ak\u0002\u0002\u0093\u0094\af\u0002\u0002\u0094$");
		stringBuilder.Append("\u0003\u0002\u0002\u0002\u0095\u0097\u00057\u001c\u0002\u0096\u0095\u0003\u0002\u0002\u0002\u0097\u0098");
		stringBuilder.Append("\u0003\u0002\u0002\u0002\u0098\u0096\u0003\u0002\u0002\u0002\u0098\u0099\u0003\u0002\u0002\u0002\u0099&\u0003");
		stringBuilder.Append("\u0002\u0002\u0002\u009a\u009f\u0005\u0019\r\u0002\u009b\u009e\u0005\u0017\f\u0002\u009c\u009e\v");
		stringBuilder.Append("\u0002\u0002\u0002\u009d\u009b\u0003\u0002\u0002\u0002\u009d\u009c\u0003\u0002\u0002\u0002\u009e¡\u0003");
		stringBuilder.Append("\u0002\u0002\u0002\u009f\u00a0\u0003\u0002\u0002\u0002\u009f\u009d\u0003\u0002\u0002\u0002\u00a0¢\u0003");
		stringBuilder.Append("\u0002\u0002\u0002¡\u009f\u0003\u0002\u0002\u0002¢£\u0005\u0019\r\u0002£\u00af\u0003");
		stringBuilder.Append("\u0002\u0002\u0002¤©\u0005\u001b\u000e\u0002¥\u00a8\u0005\u0017\f\u0002¦\u00a8\v");
		stringBuilder.Append("\u0002\u0002\u0002§¥\u0003\u0002\u0002\u0002§¦\u0003\u0002\u0002\u0002\u00a8«\u0003");
		stringBuilder.Append("\u0002\u0002\u0002©ª\u0003\u0002\u0002\u0002©§\u0003\u0002\u0002\u0002ª¬\u0003");
		stringBuilder.Append("\u0002\u0002\u0002«©\u0003\u0002\u0002\u0002¬\u00ad\u0005\u001b\u000e\u0002\u00ad\u00af\u0003");
		stringBuilder.Append("\u0002\u0002\u0002®\u009a\u0003\u0002\u0002\u0002®¤\u0003\u0002\u0002\u0002\u00af(\u0003\u0002");
		stringBuilder.Append("\u0002\u0002°±\u0005+\u0016\u0002±*\u0003\u0002\u0002\u0002²³\u0005-\u0017\u0002");
		stringBuilder.Append("³,\u0003\u0002\u0002\u0002\u00b4\u00b8\u0005/\u0018\u0002µ·\u00051\u0019\u0002¶");
		stringBuilder.Append("µ\u0003\u0002\u0002\u0002·º\u0003\u0002\u0002\u0002\u00b8¶\u0003\u0002\u0002\u0002\u00b8");
		stringBuilder.Append("¹\u0003\u0002\u0002\u0002¹.\u0003\u0002\u0002\u0002º\u00b8\u0003\u0002\u0002\u0002»¾");
		stringBuilder.Append("\u00053\u001a\u0002¼¾\aa\u0002\u0002½»\u0003\u0002\u0002\u0002½¼");
		stringBuilder.Append("\u0003\u0002\u0002\u0002¾0\u0003\u0002\u0002\u0002¿Å\u00053\u001a\u0002ÀÅ");
		stringBuilder.Append("\u00057\u001c\u0002ÁÅ\u00059\u001d\u0002ÂÅ\u00055\u001b\u0002Ã");
		stringBuilder.Append("Å\u0005;\u001e\u0002Ä¿\u0003\u0002\u0002\u0002ÄÀ\u0003\u0002\u0002\u0002Ä");
		stringBuilder.Append("Á\u0003\u0002\u0002\u0002ÄÂ\u0003\u0002\u0002\u0002ÄÃ\u0003\u0002\u0002\u0002Å");
		stringBuilder.Append("2\u0003\u0002\u0002\u0002ÆÍ\u0005=\u001f\u0002ÇÍ\u0005? \u0002ÈÍ");
		stringBuilder.Append("\u0005A!\u0002ÉÍ\u0005C\"\u0002ÊÍ\u0005E#\u0002ËÍ\u0005");
		stringBuilder.Append("G$\u0002ÌÆ\u0003\u0002\u0002\u0002ÌÇ\u0003\u0002\u0002\u0002ÌÈ\u0003\u0002");
		stringBuilder.Append("\u0002\u0002ÌÉ\u0003\u0002\u0002\u0002ÌÊ\u0003\u0002\u0002\u0002ÌË\u0003\u0002");
		stringBuilder.Append("\u0002\u0002Í4\u0003\u0002\u0002\u0002ÎÑ\u0005I%\u0002ÏÑ\u0005K&\u0002Ð");
		stringBuilder.Append("Î\u0003\u0002\u0002\u0002ÐÏ\u0003\u0002\u0002\u0002Ñ6\u0003\u0002\u0002\u0002Ò");
		stringBuilder.Append("Ó\u0005Q)\u0002Ó8\u0003\u0002\u0002\u0002ÔÕ\u0005O(\u0002Õ:\u0003\u0002");
		stringBuilder.Append("\u0002\u0002Ö×\u0005M'\u0002×<\u0003\u0002\u0002\u0002ØÙ\t\u0003\u0002\u0002");
		stringBuilder.Append("Ù>\u0003\u0002\u0002\u0002ÚÛ\u0004c|\u0002Û@\u0003\u0002\u0002\u0002ÜÝ");
		stringBuilder.Append("\t\u0004\u0002\u0002ÝB\u0003\u0002\u0002\u0002Þß\u0004ʲ\u02f0\u0002ß");
		stringBuilder.Append("D\u0003\u0002\u0002\u0002àá\t\u0005\u0002\u0002áF\u0003\u0002\u0002\u0002â");
		stringBuilder.Append("ã\t\u0006\u0002\u0002ãH\u0003\u0002\u0002\u0002äå\u0004\u0302\u0312\u0002å");
		stringBuilder.Append("J\u0003\u0002\u0002\u0002æç\t\a\u0002\u0002çL\u0003\u0002\u0002\u0002èé\t\b");
		stringBuilder.Append("\u0002\u0002éN\u0003\u0002\u0002\u0002êë\t\t\u0002\u0002ëP\u0003\u0002\u0002\u0002ì");
		stringBuilder.Append("í\u00042;\u0002íR\u0003\u0002\u0002\u0002\u0010\u0002hp\u0098\u009d\u009f§©");
		stringBuilder.Append("®\u00b8½ÄÌÐ\u0003\b\u0002\u0002");
		return stringBuilder.ToString();
	}
}
