using System;
using System.CodeDom.Compiler;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.UI.Xaml.Markup.Compiler;

[GeneratedCode("ANTLR", "4.5.3")]
public class ConditionalNamespaceParser : Parser
{
	public class ProgramContext : ParserRuleContext
	{
		public override int RuleIndex => 0;

		public ExpressionContext expression()
		{
			return GetRuleContext<ExpressionContext>(0);
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
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.EnterProgram(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.ExitProgram(this);
			}
		}
	}

	public class ExpressionContext : ParserRuleContext
	{
		public ApiInformation ApiInformation;

		public Platform TargetPlatform;

		public override int RuleIndex => 1;

		public UriContext uri()
		{
			return GetRuleContext<UriContext>(0);
		}

		public Query_stringContext query_string()
		{
			return GetRuleContext<Query_stringContext>(0);
		}

		public ExpressionContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.EnterExpression(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.ExitExpression(this);
			}
		}
	}

	public class UriContext : ParserRuleContext
	{
		public override int RuleIndex => 2;

		public UriContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.EnterUri(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.ExitUri(this);
			}
		}
	}

	public class Unquoted_namespaceContext : ParserRuleContext
	{
		public override int RuleIndex => 3;

		public ITerminalNode[] IDENTIFIER()
		{
			return GetTokens(20);
		}

		public ITerminalNode IDENTIFIER(int i)
		{
			return GetToken(20, i);
		}

		public Unquoted_namespaceContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.EnterUnquoted_namespace(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.ExitUnquoted_namespace(this);
			}
		}
	}

	public class Api_informationContext : ParserRuleContext
	{
		public ApiInformation ApiInformation;

		public override int RuleIndex => 4;

		public ITerminalNode IDENTIFIER()
		{
			return GetToken(20, 0);
		}

		public Function_paramContext[] function_param()
		{
			return GetRuleContexts<Function_paramContext>();
		}

		public Function_paramContext function_param(int i)
		{
			return GetRuleContext<Function_paramContext>(i);
		}

		public Api_informationContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.EnterApi_information(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.ExitApi_information(this);
			}
		}
	}

	public class Function_paramContext : ParserRuleContext
	{
		public ApiInformationParameter ApiInformationParameter;

		public override int RuleIndex => 5;

		public Unquoted_namespaceContext unquoted_namespace()
		{
			return GetRuleContext<Unquoted_namespaceContext>(0);
		}

		public ITerminalNode IDENTIFIER()
		{
			return GetToken(20, 0);
		}

		public ITerminalNode QuotedString()
		{
			return GetToken(19, 0);
		}

		public ITerminalNode Digits()
		{
			return GetToken(18, 0);
		}

		public Function_paramContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.EnterFunction_param(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.ExitFunction_param(this);
			}
		}
	}

	public class Target_platform_valueContext : ParserRuleContext
	{
		public override int RuleIndex => 6;

		public ITerminalNode PlatformUWP()
		{
			return GetToken(15, 0);
		}

		public ITerminalNode PlatformiOS()
		{
			return GetToken(16, 0);
		}

		public ITerminalNode PlatformAndroid()
		{
			return GetToken(17, 0);
		}

		public Target_platform_valueContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.EnterTarget_platform_value(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.ExitTarget_platform_value(this);
			}
		}
	}

	public class Query_stringContext : ParserRuleContext
	{
		public ApiInformation ApiInformation;

		public Platform TargetPlatform;

		public override int RuleIndex => 7;

		public Query_string_componentContext[] query_string_component()
		{
			return GetRuleContexts<Query_string_componentContext>();
		}

		public Query_string_componentContext query_string_component(int i)
		{
			return GetRuleContext<Query_string_componentContext>(i);
		}

		public Query_stringContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.EnterQuery_string(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.ExitQuery_string(this);
			}
		}
	}

	public class Query_string_componentContext : ParserRuleContext
	{
		public ApiInformation ApiInformation;

		public Platform TargetPlatform;

		public override int RuleIndex => 8;

		public Query_string_componentContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public Query_string_componentContext()
		{
		}

		public virtual void CopyFrom(Query_string_componentContext context)
		{
			base.CopyFrom(context);
			ApiInformation = context.ApiInformation;
			TargetPlatform = context.TargetPlatform;
		}
	}

	public class QueryStringTargetPlatformContext : Query_string_componentContext
	{
		public Target_platform_funcContext target_platform_func()
		{
			return GetRuleContext<Target_platform_funcContext>(0);
		}

		public QueryStringTargetPlatformContext(Query_string_componentContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.EnterQueryStringTargetPlatform(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.ExitQueryStringTargetPlatform(this);
			}
		}
	}

	public class QueryStringApiInformationContext : Query_string_componentContext
	{
		public Api_informationContext api_information()
		{
			return GetRuleContext<Api_informationContext>(0);
		}

		public QueryStringApiInformationContext(Query_string_componentContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.EnterQueryStringApiInformation(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.ExitQueryStringApiInformation(this);
			}
		}
	}

	public class Target_platform_funcContext : ParserRuleContext
	{
		public Platform TargetPlatform;

		public override int RuleIndex => 9;

		public ITerminalNode TargetPlatformString()
		{
			return GetToken(14, 0);
		}

		public Target_platform_valueContext target_platform_value()
		{
			return GetRuleContext<Target_platform_valueContext>(0);
		}

		public Target_platform_funcContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.EnterTarget_platform_func(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IConditionalNamespaceListener conditionalNamespaceListener)
			{
				conditionalNamespaceListener.ExitTarget_platform_func(this);
			}
		}
	}

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

	public const int RULE_program = 0;

	public const int RULE_expression = 1;

	public const int RULE_uri = 2;

	public const int RULE_unquoted_namespace = 3;

	public const int RULE_api_information = 4;

	public const int RULE_function_param = 5;

	public const int RULE_target_platform_value = 6;

	public const int RULE_query_string = 7;

	public const int RULE_query_string_component = 8;

	public const int RULE_target_platform_func = 9;

	public static readonly string[] ruleNames = new string[10] { "program", "expression", "uri", "unquoted_namespace", "api_information", "function_param", "target_platform_value", "query_string", "query_string_component", "target_platform_func" };

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

	public override string SerializedAtn => _serializedATN;

	public ConditionalNamespaceParser(ITokenStream input)
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
			base.State = 20;
			expression();
			base.State = 21;
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
	public ExpressionContext expression()
	{
		ExpressionContext expressionContext = new ExpressionContext(Context, base.State);
		EnterRule(expressionContext, 2, 1);
		try
		{
			EnterOuterAlt(expressionContext, 1);
			base.State = 23;
			uri();
			base.State = 24;
			Match(1);
			base.State = 25;
			query_string();
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (expressionContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return expressionContext;
	}

	[RuleVersion(0)]
	public UriContext uri()
	{
		UriContext uriContext = new UriContext(Context, base.State);
		EnterRule(uriContext, 4, 2);
		try
		{
			EnterOuterAlt(uriContext, 1);
			base.State = 34;
			ErrorHandler.Sync(this);
			int num = Interpreter.AdaptivePredict(base.TokenStream, 1, Context);
			while (true)
			{
				switch (num)
				{
				case 2:
					base.State = 32;
					ErrorHandler.Sync(this);
					switch (Interpreter.AdaptivePredict(base.TokenStream, 0, Context))
					{
					case 1:
						base.State = 27;
						MatchWildcard();
						break;
					case 2:
						base.State = 28;
						Match(2);
						break;
					case 3:
						base.State = 29;
						Match(3);
						break;
					case 4:
						base.State = 30;
						Match(4);
						break;
					case 5:
						base.State = 31;
						Match(5);
						break;
					}
					break;
				case 0:
				case 1:
					goto end_IL_012b;
				}
				base.State = 36;
				ErrorHandler.Sync(this);
				num = Interpreter.AdaptivePredict(base.TokenStream, 1, Context);
				continue;
				end_IL_012b:
				break;
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (uriContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return uriContext;
	}

	[RuleVersion(0)]
	public Unquoted_namespaceContext unquoted_namespace()
	{
		Unquoted_namespaceContext unquoted_namespaceContext = new Unquoted_namespaceContext(Context, base.State);
		EnterRule(unquoted_namespaceContext, 6, 3);
		try
		{
			EnterOuterAlt(unquoted_namespaceContext, 1);
			base.State = 37;
			Match(20);
			base.State = 42;
			ErrorHandler.Sync(this);
			for (int num = base.TokenStream.La(1); num == 5; num = base.TokenStream.La(1))
			{
				base.State = 38;
				Match(5);
				base.State = 39;
				Match(20);
				base.State = 44;
				ErrorHandler.Sync(this);
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (unquoted_namespaceContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return unquoted_namespaceContext;
	}

	[RuleVersion(0)]
	public Api_informationContext api_information()
	{
		Api_informationContext api_informationContext = new Api_informationContext(Context, base.State);
		EnterRule(api_informationContext, 8, 4);
		try
		{
			EnterOuterAlt(api_informationContext, 1);
			base.State = 45;
			Match(20);
			base.State = 46;
			Match(6);
			base.State = 47;
			function_param();
			base.State = 52;
			ErrorHandler.Sync(this);
			for (int num = base.TokenStream.La(1); num == 7; num = base.TokenStream.La(1))
			{
				base.State = 48;
				Match(7);
				base.State = 49;
				function_param();
				base.State = 54;
				ErrorHandler.Sync(this);
			}
			base.State = 55;
			Match(8);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (api_informationContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return api_informationContext;
	}

	[RuleVersion(0)]
	public Function_paramContext function_param()
	{
		Function_paramContext function_paramContext = new Function_paramContext(Context, base.State);
		EnterRule(function_paramContext, 10, 5);
		try
		{
			base.State = 61;
			ErrorHandler.Sync(this);
			switch (Interpreter.AdaptivePredict(base.TokenStream, 4, Context))
			{
			case 1:
				EnterOuterAlt(function_paramContext, 1);
				base.State = 57;
				unquoted_namespace();
				break;
			case 2:
				EnterOuterAlt(function_paramContext, 2);
				base.State = 58;
				Match(20);
				break;
			case 3:
				EnterOuterAlt(function_paramContext, 3);
				base.State = 59;
				Match(19);
				break;
			case 4:
				EnterOuterAlt(function_paramContext, 4);
				base.State = 60;
				Match(18);
				break;
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (function_paramContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return function_paramContext;
	}

	[RuleVersion(0)]
	public Target_platform_valueContext target_platform_value()
	{
		Target_platform_valueContext target_platform_valueContext = new Target_platform_valueContext(Context, base.State);
		EnterRule(target_platform_valueContext, 12, 6);
		try
		{
			EnterOuterAlt(target_platform_valueContext, 1);
			base.State = 63;
			int num = base.TokenStream.La(1);
			if ((num & -64) != 0 || ((1L << num) & 0x38000) == 0L)
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
			RecognitionException e = (target_platform_valueContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return target_platform_valueContext;
	}

	[RuleVersion(0)]
	public Query_stringContext query_string()
	{
		Query_stringContext query_stringContext = new Query_stringContext(Context, base.State);
		EnterRule(query_stringContext, 14, 7);
		try
		{
			EnterOuterAlt(query_stringContext, 1);
			base.State = 65;
			query_string_component();
			base.State = 70;
			ErrorHandler.Sync(this);
			for (int num = base.TokenStream.La(1); num == 9; num = base.TokenStream.La(1))
			{
				base.State = 66;
				Match(9);
				base.State = 67;
				query_string_component();
				base.State = 72;
				ErrorHandler.Sync(this);
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (query_stringContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return query_stringContext;
	}

	[RuleVersion(0)]
	public Query_string_componentContext query_string_component()
	{
		Query_string_componentContext query_string_componentContext = new Query_string_componentContext(Context, base.State);
		EnterRule(query_string_componentContext, 16, 8);
		try
		{
			base.State = 75;
			switch (base.TokenStream.La(1))
			{
			case 14:
				query_string_componentContext = new QueryStringTargetPlatformContext(query_string_componentContext);
				EnterOuterAlt(query_string_componentContext, 1);
				base.State = 73;
				target_platform_func();
				break;
			case 20:
				query_string_componentContext = new QueryStringApiInformationContext(query_string_componentContext);
				EnterOuterAlt(query_string_componentContext, 2);
				base.State = 74;
				api_information();
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (query_string_componentContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return query_string_componentContext;
	}

	[RuleVersion(0)]
	public Target_platform_funcContext target_platform_func()
	{
		Target_platform_funcContext target_platform_funcContext = new Target_platform_funcContext(Context, base.State);
		EnterRule(target_platform_funcContext, 18, 9);
		try
		{
			EnterOuterAlt(target_platform_funcContext, 1);
			base.State = 77;
			Match(14);
			base.State = 78;
			Match(6);
			base.State = 79;
			target_platform_value();
			base.State = 80;
			Match(8);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (target_platform_funcContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return target_platform_funcContext;
	}

	private static string _serializeATN()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("\u0003а훑舆괭䐗껱趀ꫝ\u0003\u0016");
		stringBuilder.Append("U\u0004\u0002\t\u0002\u0004\u0003\t\u0003\u0004\u0004\t\u0004\u0004\u0005\t\u0005\u0004\u0006\t\u0006\u0004");
		stringBuilder.Append("\a\t\a\u0004\b\t\b\u0004\t\t\t\u0004\n\t\n\u0004\v\t\v\u0003\u0002\u0003\u0002\u0003\u0002");
		stringBuilder.Append("\u0003\u0003\u0003\u0003\u0003\u0003\u0003\u0003\u0003\u0004\u0003\u0004\u0003\u0004\u0003\u0004\u0003\u0004\a\u0004");
		stringBuilder.Append("#\n\u0004\f\u0004\u000e\u0004&\v\u0004\u0003\u0005\u0003\u0005\u0003\u0005\a\u0005+\n\u0005\f\u0005\u000e");
		stringBuilder.Append("\u0005.\v\u0005\u0003\u0006\u0003\u0006\u0003\u0006\u0003\u0006\u0003\u0006\a\u00065\n\u0006\f\u0006\u000e");
		stringBuilder.Append("\u00068\v\u0006\u0003\u0006\u0003\u0006\u0003\a\u0003\a\u0003\a\u0003\a\u0005\a@\n\a\u0003\b");
		stringBuilder.Append("\u0003\b\u0003\t\u0003\t\u0003\t\a\tG\n\t\f\t\u000e\tJ\v\t\u0003\n\u0003\n\u0005\nN");
		stringBuilder.Append("\n\n\u0003\v\u0003\v\u0003\v\u0003\v\u0003\v\u0003\v\u0003$\u0002\f\u0002\u0004\u0006\b\n\f\u000e");
		stringBuilder.Append("\u0010\u0012\u0014\u0002\u0003\u0003\u0002\u0011\u0013V\u0002\u0016\u0003\u0002\u0002\u0002\u0004\u0019");
		stringBuilder.Append("\u0003\u0002\u0002\u0002\u0006$\u0003\u0002\u0002\u0002\b'\u0003\u0002\u0002\u0002\n/\u0003\u0002\u0002\u0002");
		stringBuilder.Append("\f?\u0003\u0002\u0002\u0002\u000eA\u0003\u0002\u0002\u0002\u0010C\u0003\u0002\u0002\u0002\u0012M");
		stringBuilder.Append("\u0003\u0002\u0002\u0002\u0014O\u0003\u0002\u0002\u0002\u0016\u0017\u0005\u0004\u0003\u0002\u0017\u0018\a");
		stringBuilder.Append("\u0002\u0002\u0003\u0018\u0003\u0003\u0002\u0002\u0002\u0019\u001a\u0005\u0006\u0004\u0002\u001a\u001b\a\u0003");
		stringBuilder.Append("\u0002\u0002\u001b\u001c\u0005\u0010\t\u0002\u001c\u0005\u0003\u0002\u0002\u0002\u001d#\v\u0002\u0002\u0002");
		stringBuilder.Append("\u001e#\a\u0004\u0002\u0002\u001f#\a\u0005\u0002\u0002 #\a\u0006\u0002\u0002!#\a\a\u0002\u0002\"");
		stringBuilder.Append("\u001d\u0003\u0002\u0002\u0002\"\u001e\u0003\u0002\u0002\u0002\"\u001f\u0003\u0002\u0002\u0002\" \u0003\u0002");
		stringBuilder.Append("\u0002\u0002\"!\u0003\u0002\u0002\u0002#&\u0003\u0002\u0002\u0002$%\u0003\u0002\u0002\u0002$\"\u0003\u0002\u0002");
		stringBuilder.Append("\u0002%\a\u0003\u0002\u0002\u0002&$\u0003\u0002\u0002\u0002',\a\u0016\u0002\u0002()\a\a\u0002\u0002");
		stringBuilder.Append(")+\a\u0016\u0002\u0002*(\u0003\u0002\u0002\u0002+.\u0003\u0002\u0002\u0002,*\u0003\u0002\u0002\u0002,-\u0003");
		stringBuilder.Append("\u0002\u0002\u0002-\t\u0003\u0002\u0002\u0002.,\u0003\u0002\u0002\u0002/0\a\u0016\u0002\u00020");
		stringBuilder.Append("1\a\b\u0002\u000216\u0005\f\a\u000223\a\t\u0002\u000235\u0005");
		stringBuilder.Append("\f\a\u000242\u0003\u0002\u0002\u000258\u0003\u0002\u0002\u000264\u0003\u0002");
		stringBuilder.Append("\u0002\u000267\u0003\u0002\u0002\u000279\u0003\u0002\u0002\u000286\u0003\u0002");
		stringBuilder.Append("\u0002\u00029:\a\n\u0002\u0002:\v\u0003\u0002\u0002\u0002;@\u0005\b\u0005\u0002<@\a\u0016\u0002");
		stringBuilder.Append("\u0002=@\a\u0015\u0002\u0002>@\a\u0014\u0002\u0002?;\u0003\u0002\u0002\u0002?<\u0003\u0002\u0002\u0002");
		stringBuilder.Append("?=\u0003\u0002\u0002\u0002?>\u0003\u0002\u0002\u0002@\r\u0003\u0002\u0002\u0002AB\t\u0002\u0002");
		stringBuilder.Append("\u0002B\u000f\u0003\u0002\u0002\u0002CH\u0005\u0012\n\u0002DE\a\v\u0002\u0002E");
		stringBuilder.Append("G\u0005\u0012\n\u0002FD\u0003\u0002\u0002\u0002GJ\u0003\u0002\u0002\u0002HF\u0003\u0002\u0002");
		stringBuilder.Append("\u0002HI\u0003\u0002\u0002\u0002I\u0011\u0003\u0002\u0002\u0002JH\u0003\u0002\u0002\u0002KN\u0005\u0014\v");
		stringBuilder.Append("\u0002LN\u0005\n\u0006\u0002MK\u0003\u0002\u0002\u0002ML\u0003\u0002\u0002\u0002N\u0013\u0003\u0002\u0002\u0002");
		stringBuilder.Append("OP\a\u0010\u0002\u0002PQ\a\b\u0002\u0002QR\u0005\u000e\b\u0002RS\a\n\u0002\u0002S\u0015\u0003");
		stringBuilder.Append("\u0002\u0002\u0002\t\"$,6?HM");
		return stringBuilder.ToString();
	}
}
