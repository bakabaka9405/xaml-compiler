using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.5.1")]
public class SuccinctCollectionSyntaxParser : Parser
{
	public class ProgramContext : ParserRuleContext
	{
		public override int RuleIndex => 0;

		public ItemsContext items()
		{
			return GetRuleContext<ItemsContext>(0);
		}

		public ITerminalNode Eof()
		{
			return GetToken(-1, 0);
		}

		public ProgramContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.EnterProgram(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.ExitProgram(this);
			}
		}
	}

	public class ItemsContext : ParserRuleContext
	{
		public override int RuleIndex => 1;

		public ItemContext item()
		{
			return GetRuleContext<ItemContext>(0);
		}

		public ZContext z()
		{
			return GetRuleContext<ZContext>(0);
		}

		public ItemsContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.EnterItems(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.ExitItems(this);
			}
		}
	}

	public class ZContext : ParserRuleContext
	{
		public override int RuleIndex => 2;

		public ITerminalNode COMMA()
		{
			return GetToken(6, 0);
		}

		public ItemContext item()
		{
			return GetRuleContext<ItemContext>(0);
		}

		public ZContext z()
		{
			return GetRuleContext<ZContext>(0);
		}

		public ZContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.EnterZ(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.ExitZ(this);
			}
		}
	}

	public class ItemContext : ParserRuleContext
	{
		public override int RuleIndex => 3;

		public TextContext text()
		{
			return GetRuleContext<TextContext>(0);
		}

		public ITerminalNode[] SINGLE_QUOTE()
		{
			return GetTokens(1);
		}

		public ITerminalNode SINGLE_QUOTE(int i)
		{
			return GetToken(1, i);
		}

		public Literal_textContext literal_text()
		{
			return GetRuleContext<Literal_textContext>(0);
		}

		public ITerminalNode[] DOUBLE_QUOTE()
		{
			return GetTokens(2);
		}

		public ITerminalNode DOUBLE_QUOTE(int i)
		{
			return GetToken(2, i);
		}

		public ItemContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.EnterItem(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.ExitItem(this);
			}
		}
	}

	public class TextContext : ParserRuleContext
	{
		public override int RuleIndex => 4;

		public ITerminalNode CHARACTER()
		{
			return GetToken(5, 0);
		}

		public TextContext text()
		{
			return GetRuleContext<TextContext>(0);
		}

		public ITerminalNode BACKSLASH()
		{
			return GetToken(7, 0);
		}

		public SequenceContext sequence()
		{
			return GetRuleContext<SequenceContext>(0);
		}

		public TextContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.EnterText(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.ExitText(this);
			}
		}
	}

	public class Literal_textContext : ParserRuleContext
	{
		public override int RuleIndex => 5;

		public ITerminalNode CHARACTER()
		{
			return GetToken(5, 0);
		}

		public Literal_textContext literal_text()
		{
			return GetRuleContext<Literal_textContext>(0);
		}

		public ITerminalNode BACKSLASH()
		{
			return GetToken(7, 0);
		}

		public SequenceContext sequence()
		{
			return GetRuleContext<SequenceContext>(0);
		}

		public ITerminalNode RESERVED_SYMBOL()
		{
			return GetToken(8, 0);
		}

		public ITerminalNode COMMA()
		{
			return GetToken(6, 0);
		}

		public Literal_textContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.EnterLiteral_text(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.ExitLiteral_text(this);
			}
		}
	}

	public class SequenceContext : ParserRuleContext
	{
		public override int RuleIndex => 6;

		public ITerminalNode CHARACTER()
		{
			return GetToken(5, 0);
		}

		public ITerminalNode RESERVED_SYMBOL()
		{
			return GetToken(8, 0);
		}

		public ITerminalNode QUOTE()
		{
			return GetToken(9, 0);
		}

		public ITerminalNode COMMA()
		{
			return GetToken(6, 0);
		}

		public ITerminalNode BACKSLASH()
		{
			return GetToken(7, 0);
		}

		public SequenceContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.EnterSequence(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is ISuccinctCollectionSyntaxListener succinctCollectionSyntaxListener)
			{
				succinctCollectionSyntaxListener.ExitSequence(this);
			}
		}
	}

	public const int SINGLE_QUOTE = 1;

	public const int DOUBLE_QUOTE = 2;

	public const int OPEN_SQUARE_BRACE = 3;

	public const int CLOSE_SQUARE_BRACE = 4;

	public const int CHARACTER = 5;

	public const int COMMA = 6;

	public const int BACKSLASH = 7;

	public const int RESERVED_SYMBOL = 8;

	public const int QUOTE = 9;

	public const int RULE_program = 0;

	public const int RULE_items = 1;

	public const int RULE_z = 2;

	public const int RULE_item = 3;

	public const int RULE_text = 4;

	public const int RULE_literal_text = 5;

	public const int RULE_sequence = 6;

	public static readonly string[] ruleNames = new string[7] { "program", "items", "z", "item", "text", "literal_text", "sequence" };

	private static readonly string[] _LiteralNames = new string[8] { null, "'''", "'\"'", "'['", "']'", null, "','", "'\\'" };

	private static readonly string[] _SymbolicNames = new string[10] { null, "SINGLE_QUOTE", "DOUBLE_QUOTE", "OPEN_SQUARE_BRACE", "CLOSE_SQUARE_BRACE", "CHARACTER", "COMMA", "BACKSLASH", "RESERVED_SYMBOL", "QUOTE" };

	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	public static readonly string _serializedATN = "\u0003а훑舆괭䐗껱趀ꫝ\u0003\vB\u0004\u0002\t\u0002\u0004\u0003\t\u0003\u0004\u0004\t\u0004\u0004\u0005\t\u0005\u0004\u0006\t\u0006\u0004\a\t\a\u0004\b\t\b\u0003\u0002\u0003\u0002\u0003\u0002\u0003\u0003\u0003\u0003\u0003\u0003\u0003\u0004\u0003\u0004\u0003\u0004\u0003\u0004\u0005\u0004\u001b\n\u0004\u0003\u0005\u0003\u0005\u0003\u0005\u0003\u0005\u0003\u0005\u0003\u0005\u0003\u0005\u0003\u0005\u0003\u0005\u0005\u0005&\n\u0005\u0003\u0006\u0003\u0006\u0005\u0006*\n\u0006\u0003\u0006\u0003\u0006\u0003\u0006\u0003\u0006\u0005\u00060\n\u0006\u0003\a\u0003\a\u0005\a4\n\a\u0003\a\u0003\a\u0003\a\u0003\a\u0003\a\u0003\a\u0003\a\u0003\a\u0005\a>\n\a\u0003\b\u0003\b\u0003\b\u0002\u0002\t\u0002\u0004\u0006\b\n\f\u000e\u0002\u0003\u0003\u0002\a\vC\u0002\u0010\u0003\u0002\u0002\u0002\u0004\u0013\u0003\u0002\u0002\u0002\u0006\u001a\u0003\u0002\u0002\u0002\b%\u0003\u0002\u0002\u0002\n/\u0003\u0002\u0002\u0002\f=\u0003\u0002\u0002\u0002\u000e?\u0003\u0002\u0002\u0002\u0010\u0011\u0005\u0004\u0003\u0002\u0011\u0012\a\u0002\u0002\u0003\u0012\u0003\u0003\u0002\u0002\u0002\u0013\u0014\u0005\b\u0005\u0002\u0014\u0015\u0005\u0006\u0004\u0002\u0015\u0005\u0003\u0002\u0002\u0002\u0016\u0017\a\b\u0002\u0002\u0017\u0018\u0005\b\u0005\u0002\u0018\u0019\u0005\u0006\u0004\u0002\u0019\u001b\u0003\u0002\u0002\u0002\u001a\u0016\u0003\u0002\u0002\u0002\u001a\u001b\u0003\u0002\u0002\u0002\u001b\a\u0003\u0002\u0002\u0002\u001c&\u0005\n\u0006\u0002\u001d\u001e\a\u0003\u0002\u0002\u001e\u001f\u0005\f\a\u0002\u001f \a\u0003\u0002\u0002 &\u0003\u0002\u0002\u0002!\"\a\u0004\u0002\u0002\"#\u0005\f\a\u0002#$\a\u0004\u0002\u0002$&\u0003\u0002\u0002\u0002%\u001c\u0003\u0002\u0002\u0002%\u001d\u0003\u0002\u0002\u0002%!\u0003\u0002\u0002\u0002&\t\u0003\u0002\u0002\u0002'(\a\a\u0002\u0002(*\u0005\n\u0006\u0002)'\u0003\u0002\u0002\u0002)*\u0003\u0002\u0002\u0002*0\u0003\u0002\u0002\u0002+,\a\t\u0002\u0002,-\u0005\u000e\b\u0002-.\u0005\n\u0006\u0002.0\u0003\u0002\u0002\u0002/)\u0003\u0002\u0002\u0002/+\u0003\u0002\u0002\u00020\v\u0003\u0002\u0002\u000212\a\a\u0002\u000224\u0005\f\a\u000231\u0003\u0002\u0002\u000234\u0003\u0002\u0002\u00024>\u0003\u0002\u0002\u000256\a\t\u0002\u000267\u0005\u000e\b\u000278\u0005\f\a\u00028>\u0003\u0002\u0002\u00029:\a\n\u0002\u0002:>\u0005\f\a\u0002;<\a\b\u0002\u0002<>\u0005\f\a\u0002=3\u0003\u0002\u0002\u0002=5\u0003\u0002\u0002\u0002=9\u0003\u0002\u0002\u0002=;\u0003\u0002\u0002\u0002>\r\u0003\u0002\u0002\u0002?@\t\u0002\u0002\u0002@\u000f\u0003\u0002\u0002\u0002\b\u001a%)/3=";

	public static readonly ATN _ATN = new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());

	[NotNull]
	public override IVocabulary Vocabulary => DefaultVocabulary;

	public override string GrammarFileName => "SuccinctCollectionSyntax.g4";

	public override string[] RuleNames => ruleNames;

	public override string SerializedAtn => _serializedATN;

	public SuccinctCollectionSyntaxParser(ITokenStream input)
		: base(input)
	{
		Interpreter = new ParserATNSimulator(this, _ATN);
	}

	[RuleVersion(0)]
	public ProgramContext program()
	{
		ProgramContext programContext = new ProgramContext(Context, base.State);
		EnterRule(programContext, 0, 0);
		try
		{
			EnterOuterAlt(programContext, 1);
			base.State = 14;
			items();
			base.State = 15;
			Match(-1);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (programContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return programContext;
	}

	[RuleVersion(0)]
	public ItemsContext items()
	{
		ItemsContext itemsContext = new ItemsContext(Context, base.State);
		EnterRule(itemsContext, 2, 1);
		try
		{
			EnterOuterAlt(itemsContext, 1);
			base.State = 17;
			item();
			base.State = 18;
			z();
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (itemsContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return itemsContext;
	}

	[RuleVersion(0)]
	public ZContext z()
	{
		ZContext zContext = new ZContext(Context, base.State);
		EnterRule(zContext, 4, 2);
		try
		{
			EnterOuterAlt(zContext, 1);
			base.State = 24;
			int num = base.TokenStream.La(1);
			if (num == 6)
			{
				base.State = 20;
				Match(6);
				base.State = 21;
				item();
				base.State = 22;
				z();
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (zContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return zContext;
	}

	[RuleVersion(0)]
	public ItemContext item()
	{
		ItemContext itemContext = new ItemContext(Context, base.State);
		EnterRule(itemContext, 6, 3);
		try
		{
			base.State = 35;
			switch (base.TokenStream.La(1))
			{
			case -1:
			case 5:
			case 6:
			case 7:
				EnterOuterAlt(itemContext, 1);
				base.State = 26;
				text();
				break;
			case 1:
				EnterOuterAlt(itemContext, 2);
				base.State = 27;
				Match(1);
				base.State = 28;
				literal_text();
				base.State = 29;
				Match(1);
				break;
			case 2:
				EnterOuterAlt(itemContext, 3);
				base.State = 31;
				Match(2);
				base.State = 32;
				literal_text();
				base.State = 33;
				Match(2);
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (itemContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return itemContext;
	}

	[RuleVersion(0)]
	public TextContext text()
	{
		TextContext textContext = new TextContext(Context, base.State);
		EnterRule(textContext, 8, 4);
		try
		{
			base.State = 45;
			switch (base.TokenStream.La(1))
			{
			case -1:
			case 5:
			case 6:
			{
				EnterOuterAlt(textContext, 1);
				base.State = 39;
				int num = base.TokenStream.La(1);
				if (num == 5)
				{
					base.State = 37;
					Match(5);
					base.State = 38;
					text();
				}
				break;
			}
			case 7:
				EnterOuterAlt(textContext, 2);
				base.State = 41;
				Match(7);
				base.State = 42;
				sequence();
				base.State = 43;
				text();
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (textContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return textContext;
	}

	[RuleVersion(0)]
	public Literal_textContext literal_text()
	{
		Literal_textContext literal_textContext = new Literal_textContext(Context, base.State);
		EnterRule(literal_textContext, 10, 5);
		try
		{
			base.State = 59;
			switch (base.TokenStream.La(1))
			{
			case 1:
			case 2:
			case 5:
			{
				EnterOuterAlt(literal_textContext, 1);
				base.State = 49;
				int num = base.TokenStream.La(1);
				if (num == 5)
				{
					base.State = 47;
					Match(5);
					base.State = 48;
					literal_text();
				}
				break;
			}
			case 7:
				EnterOuterAlt(literal_textContext, 2);
				base.State = 51;
				Match(7);
				base.State = 52;
				sequence();
				base.State = 53;
				literal_text();
				break;
			case 8:
				EnterOuterAlt(literal_textContext, 3);
				base.State = 55;
				Match(8);
				base.State = 56;
				literal_text();
				break;
			case 6:
				EnterOuterAlt(literal_textContext, 4);
				base.State = 57;
				Match(6);
				base.State = 58;
				literal_text();
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (literal_textContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return literal_textContext;
	}

	[RuleVersion(0)]
	public SequenceContext sequence()
	{
		SequenceContext sequenceContext = new SequenceContext(Context, base.State);
		EnterRule(sequenceContext, 12, 6);
		try
		{
			EnterOuterAlt(sequenceContext, 1);
			base.State = 61;
			int num = base.TokenStream.La(1);
			if ((num & -64) != 0 || ((1L << num) & 0x3E0) == 0L)
			{
				ErrorHandler.RecoverInline(this);
			}
			else
			{
				Consume();
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (sequenceContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return sequenceContext;
	}
}
