using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.UI.Xaml.Markup.Compiler;

[GeneratedCode("ANTLR", "4.5.1")]
public class BindingPathParser : Parser
{
	public class ProgramContext : ParserRuleContext
	{
		public override int RuleIndex => 0;

		public PathContext path()
		{
			return GetRuleContext<PathContext>(0);
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
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterProgram(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitProgram(this);
			}
		}
	}

	public class Decimal_valueContext : ParserRuleContext
	{
		public override int RuleIndex => 1;

		public ITerminalNode[] Digits()
		{
			return GetTokens(16);
		}

		public ITerminalNode Digits(int i)
		{
			return GetToken(16, i);
		}

		public Decimal_valueContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterDecimal_value(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitDecimal_value(this);
			}
		}
	}

	public class Boolean_valueContext : ParserRuleContext
	{
		public override int RuleIndex => 2;

		public ITerminalNode TRUE()
		{
			return GetToken(13, 0);
		}

		public ITerminalNode FALSE()
		{
			return GetToken(14, 0);
		}

		public Boolean_valueContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterBoolean_value(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitBoolean_value(this);
			}
		}
	}

	public class Namespace_qualifierContext : ParserRuleContext
	{
		public override int RuleIndex => 3;

		public ITerminalNode IDENTIFIER()
		{
			return GetToken(18, 0);
		}

		public Namespace_qualifierContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterNamespace_qualifier(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitNamespace_qualifier(this);
			}
		}
	}

	public class Static_typeContext : ParserRuleContext
	{
		public override int RuleIndex => 4;

		public Namespace_qualifierContext namespace_qualifier()
		{
			return GetRuleContext<Namespace_qualifierContext>(0);
		}

		public ITerminalNode IDENTIFIER()
		{
			return GetToken(18, 0);
		}

		public Static_typeContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterStatic_type(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitStatic_type(this);
			}
		}
	}

	public class Attached_exprContext : ParserRuleContext
	{
		public override int RuleIndex => 5;

		public Static_typeContext static_type()
		{
			return GetRuleContext<Static_typeContext>(0);
		}

		public ITerminalNode[] IDENTIFIER()
		{
			return GetTokens(18);
		}

		public ITerminalNode IDENTIFIER(int i)
		{
			return GetToken(18, i);
		}

		public Attached_exprContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterAttached_expr(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitAttached_expr(this);
			}
		}
	}

	public class Cast_exprContext : ParserRuleContext
	{
		public override int RuleIndex => 6;

		public Static_typeContext static_type()
		{
			return GetRuleContext<Static_typeContext>(0);
		}

		public ITerminalNode IDENTIFIER()
		{
			return GetToken(18, 0);
		}

		public Cast_exprContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterCast_expr(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitCast_expr(this);
			}
		}
	}

	public class FunctionContext : ParserRuleContext
	{
		public override int RuleIndex => 7;

		public ITerminalNode IDENTIFIER()
		{
			return GetToken(18, 0);
		}

		public Function_paramContext[] function_param()
		{
			return GetRuleContexts<Function_paramContext>();
		}

		public Function_paramContext function_param(int i)
		{
			return GetRuleContext<Function_paramContext>(i);
		}

		public FunctionContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterFunction(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitFunction(this);
			}
		}
	}

	public class PathContext : ParserRuleContext
	{
		public BindPathStep PathStep;

		public override int RuleIndex => 8;

		public PathContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public PathContext()
		{
		}

		public virtual void CopyFrom(PathContext context)
		{
			base.CopyFrom(context);
			PathStep = context.PathStep;
		}
	}

	public class PathStaticFuctionContext : PathContext
	{
		public Static_typeContext static_type()
		{
			return GetRuleContext<Static_typeContext>(0);
		}

		public FunctionContext function()
		{
			return GetRuleContext<FunctionContext>(0);
		}

		public PathStaticFuctionContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathStaticFuction(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathStaticFuction(this);
			}
		}
	}

	public class PathCastContext : PathContext
	{
		public Cast_exprContext cast_expr()
		{
			return GetRuleContext<Cast_exprContext>(0);
		}

		public PathCastContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathCast(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathCast(this);
			}
		}
	}

	public class PathPathToFunctionContext : PathContext
	{
		public PathContext path()
		{
			return GetRuleContext<PathContext>(0);
		}

		public FunctionContext function()
		{
			return GetRuleContext<FunctionContext>(0);
		}

		public PathPathToFunctionContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathPathToFunction(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathPathToFunction(this);
			}
		}
	}

	public class PathIndexerContext : PathContext
	{
		public PathContext path()
		{
			return GetRuleContext<PathContext>(0);
		}

		public ITerminalNode Digits()
		{
			return GetToken(16, 0);
		}

		public PathIndexerContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathIndexer(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathIndexer(this);
			}
		}
	}

	public class PathCastInvalidContext : PathContext
	{
		public Attached_exprContext attached_expr()
		{
			return GetRuleContext<Attached_exprContext>(0);
		}

		public PathCastInvalidContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathCastInvalid(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathCastInvalid(this);
			}
		}
	}

	public class PathCastPathParenContext : PathContext
	{
		public Cast_exprContext cast_expr()
		{
			return GetRuleContext<Cast_exprContext>(0);
		}

		public PathContext path()
		{
			return GetRuleContext<PathContext>(0);
		}

		public PathCastPathParenContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathCastPathParen(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathCastPathParen(this);
			}
		}
	}

	public class PathFunctionContext : PathContext
	{
		public FunctionContext function()
		{
			return GetRuleContext<FunctionContext>(0);
		}

		public PathFunctionContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathFunction(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathFunction(this);
			}
		}
	}

	public class PathStringIndexerContext : PathContext
	{
		public PathContext path()
		{
			return GetRuleContext<PathContext>(0);
		}

		public ITerminalNode QuotedString()
		{
			return GetToken(17, 0);
		}

		public PathStringIndexerContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathStringIndexer(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathStringIndexer(this);
			}
		}
	}

	public class PathIdentifierContext : PathContext
	{
		public ITerminalNode IDENTIFIER()
		{
			return GetToken(18, 0);
		}

		public PathIdentifierContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathIdentifier(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathIdentifier(this);
			}
		}
	}

	public class PathCastPathContext : PathContext
	{
		public Cast_exprContext cast_expr()
		{
			return GetRuleContext<Cast_exprContext>(0);
		}

		public PathContext path()
		{
			return GetRuleContext<PathContext>(0);
		}

		public PathCastPathContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathCastPath(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathCastPath(this);
			}
		}
	}

	public class PathStaticIdentifierContext : PathContext
	{
		public Static_typeContext static_type()
		{
			return GetRuleContext<Static_typeContext>(0);
		}

		public ITerminalNode IDENTIFIER()
		{
			return GetToken(18, 0);
		}

		public PathStaticIdentifierContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathStaticIdentifier(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathStaticIdentifier(this);
			}
		}
	}

	public class PathDotIdentifierContext : PathContext
	{
		public PathContext path()
		{
			return GetRuleContext<PathContext>(0);
		}

		public ITerminalNode IDENTIFIER()
		{
			return GetToken(18, 0);
		}

		public PathDotIdentifierContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathDotIdentifier(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathDotIdentifier(this);
			}
		}
	}

	public class PathDotAttachedContext : PathContext
	{
		public PathContext path()
		{
			return GetRuleContext<PathContext>(0);
		}

		public Attached_exprContext attached_expr()
		{
			return GetRuleContext<Attached_exprContext>(0);
		}

		public PathDotAttachedContext(PathContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterPathDotAttached(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitPathDotAttached(this);
			}
		}
	}

	public class Function_paramContext : ParserRuleContext
	{
		public FunctionParam Param;

		public override int RuleIndex => 9;

		public Function_paramContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public Function_paramContext()
		{
		}

		public virtual void CopyFrom(Function_paramContext context)
		{
			base.CopyFrom(context);
			Param = context.Param;
		}
	}

	public class FunctionParamNumberContext : Function_paramContext
	{
		public Decimal_valueContext decimal_value()
		{
			return GetRuleContext<Decimal_valueContext>(0);
		}

		public FunctionParamNumberContext(Function_paramContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterFunctionParamNumber(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitFunctionParamNumber(this);
			}
		}
	}

	public class FunctionParamBoolContext : Function_paramContext
	{
		public Boolean_valueContext boolean_value()
		{
			return GetRuleContext<Boolean_valueContext>(0);
		}

		public FunctionParamBoolContext(Function_paramContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterFunctionParamBool(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitFunctionParamBool(this);
			}
		}
	}

	public class FunctionParameterInvalidContext : Function_paramContext
	{
		public FunctionContext function()
		{
			return GetRuleContext<FunctionContext>(0);
		}

		public FunctionParameterInvalidContext(Function_paramContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterFunctionParameterInvalid(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitFunctionParameterInvalid(this);
			}
		}
	}

	public class FunctionParamStringContext : Function_paramContext
	{
		public ITerminalNode QuotedString()
		{
			return GetToken(17, 0);
		}

		public FunctionParamStringContext(Function_paramContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterFunctionParamString(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitFunctionParamString(this);
			}
		}
	}

	public class FunctionParamPathContext : Function_paramContext
	{
		public PathContext path()
		{
			return GetRuleContext<PathContext>(0);
		}

		public FunctionParamPathContext(Function_paramContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterFunctionParamPath(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitFunctionParamPath(this);
			}
		}
	}

	public class FunctionParamNullValueContext : Function_paramContext
	{
		public ITerminalNode NULL()
		{
			return GetToken(15, 0);
		}

		public FunctionParamNullValueContext(Function_paramContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.EnterFunctionParamNullValue(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IBindingPathListener bindingPathListener)
			{
				bindingPathListener.ExitFunctionParamNullValue(this);
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

	public const int WS = 9;

	public const int ESCAPEDQUOTE = 10;

	public const int QUOTE = 11;

	public const int DOUBLE_QUOTE = 12;

	public const int TRUE = 13;

	public const int FALSE = 14;

	public const int NULL = 15;

	public const int Digits = 16;

	public const int QuotedString = 17;

	public const int IDENTIFIER = 18;

	public const int RULE_program = 0;

	public const int RULE_decimal_value = 1;

	public const int RULE_boolean_value = 2;

	public const int RULE_namespace_qualifier = 3;

	public const int RULE_static_type = 4;

	public const int RULE_attached_expr = 5;

	public const int RULE_cast_expr = 6;

	public const int RULE_function = 7;

	public const int RULE_path = 8;

	public const int RULE_function_param = 9;

	public static readonly string[] ruleNames = new string[10] { "program", "decimal_value", "boolean_value", "namespace_qualifier", "static_type", "attached_expr", "cast_expr", "function", "path", "function_param" };

	private static readonly string[] _LiteralNames = new string[16]
	{
		null, "'-'", "'.'", "':'", "'('", "')'", "','", "'['", "']'", null,
		null, "'''", "'\"'", "'x:True'", "'x:False'", "'x:Null'"
	};

	private static readonly string[] _SymbolicNames = new string[19]
	{
		null, null, null, null, null, null, null, null, null, "WS",
		"ESCAPEDQUOTE", "QUOTE", "DOUBLE_QUOTE", "TRUE", "FALSE", "NULL", "Digits", "QuotedString", "IDENTIFIER"
	};

	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	public static readonly string _serializedATN = "\u0003а훑舆괭䐗껱趀ꫝ\u0003\u0014\u0083\u0004\u0002\t\u0002\u0004\u0003\t\u0003\u0004\u0004\t\u0004\u0004\u0005\t\u0005\u0004\u0006\t\u0006\u0004\a\t\a\u0004\b\t\b\u0004\t\t\t\u0004\n\t\n\u0004\v\t\v\u0003\u0002\u0003\u0002\u0003\u0002\u0003\u0003\u0005\u0003\u001b\n\u0003\u0003\u0003\u0003\u0003\u0003\u0003\u0005\u0003 \n\u0003\u0003\u0004\u0003\u0004\u0003\u0005\u0003\u0005\u0003\u0005\u0003\u0006\u0003\u0006\u0003\u0006\u0003\a\u0003\a\u0003\a\u0003\a\u0003\a\u0003\a\u0003\a\u0003\a\u0003\a\u0003\a\u0003\a\u0005\a5\n\a\u0003\b\u0003\b\u0003\b\u0003\b\u0003\b\u0003\b\u0003\b\u0005\b>\n\b\u0003\t\u0003\t\u0003\t\u0005\tC\n\t\u0003\t\u0003\t\a\tG\n\t\f\t\u000e\tJ\v\t\u0003\t\u0003\t\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0005\nc\n\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\u0003\n\a\nv\n\n\f\n\u000e\ny\v\n\u0003\v\u0003\v\u0003\v\u0003\v\u0003\v\u0003\v\u0005\v\u0081\n\v\u0003\v\u0002\u0003\u0012\f\u0002\u0004\u0006\b\n\f\u000e\u0010\u0012\u0014\u0002\u0003\u0003\u0002\u000f\u0010\u008f\u0002\u0016\u0003\u0002\u0002\u0002\u0004\u001a\u0003\u0002\u0002\u0002\u0006!\u0003\u0002\u0002\u0002\b#\u0003\u0002\u0002\u0002\n&\u0003\u0002\u0002\u0002\f4\u0003\u0002\u0002\u0002\u000e=\u0003\u0002\u0002\u0002\u0010?\u0003\u0002\u0002\u0002\u0012b\u0003\u0002\u0002\u0002\u0014\u0080\u0003\u0002\u0002\u0002\u0016\u0017\u0005\u0012\n\u0002\u0017\u0018\a\u0002\u0002\u0003\u0018\u0003\u0003\u0002\u0002\u0002\u0019\u001b\a\u0003\u0002\u0002\u001a\u0019\u0003\u0002\u0002\u0002\u001a\u001b\u0003\u0002\u0002\u0002\u001b\u001c\u0003\u0002\u0002\u0002\u001c\u001f\a\u0012\u0002\u0002\u001d\u001e\a\u0004\u0002\u0002\u001e \a\u0012\u0002\u0002\u001f\u001d\u0003\u0002\u0002\u0002\u001f \u0003\u0002\u0002\u0002 \u0005\u0003\u0002\u0002\u0002!\"\t\u0002\u0002\u0002\"\a\u0003\u0002\u0002\u0002#$\a\u0014\u0002\u0002$%\a\u0005\u0002\u0002%\t\u0003\u0002\u0002\u0002&'\u0005\b\u0005\u0002'(\a\u0014\u0002\u0002(\v\u0003\u0002\u0002\u0002)*\a\u0006\u0002\u0002*+\u0005\n\u0006\u0002+,\a\u0004\u0002\u0002,-\a\u0014\u0002\u0002-.\a\a\u0002\u0002.5\u0003\u0002\u0002\u0002/0\a\u0006\u0002\u000201\a\u0014\u0002\u000212\a\u0004\u0002\u000223\a\u0014\u0002\u000235\a\a\u0002\u00024)\u0003\u0002\u0002\u00024/\u0003\u0002\u0002\u00025\r\u0003\u0002\u0002\u000267\a\u0006\u0002\u000278\u0005\n\u0006\u000289\a\a\u0002\u00029>\u0003\u0002\u0002\u0002:;\a\u0006\u0002\u0002;<\a\u0014\u0002\u0002<>\a\a\u0002\u0002=6\u0003\u0002\u0002\u0002=:\u0003\u0002\u0002\u0002>\u000f\u0003\u0002\u0002\u0002?@\a\u0014\u0002\u0002@B\a\u0006\u0002\u0002AC\u0005\u0014\v\u0002BA\u0003\u0002\u0002\u0002BC\u0003\u0002\u0002\u0002CH\u0003\u0002\u0002\u0002DE\a\b\u0002\u0002EG\u0005\u0014\v\u0002FD\u0003\u0002\u0002\u0002GJ\u0003\u0002\u0002\u0002HF\u0003\u0002\u0002\u0002HI\u0003\u0002\u0002\u0002IK\u0003\u0002\u0002\u0002JH\u0003\u0002\u0002\u0002KL\a\a\u0002\u0002L\u0011\u0003\u0002\u0002\u0002MN\b\n\u0001\u0002NO\u0005\u000e\b\u0002OP\u0005\u0012\n\aPc\u0003\u0002\u0002\u0002Qc\a\u0014\u0002\u0002RS\u0005\n\u0006\u0002ST\a\u0004\u0002\u0002TU\a\u0014\u0002\u0002Uc\u0003\u0002\u0002\u0002Vc\u0005\f\a\u0002Wc\u0005\u000e\b\u0002XY\a\u0006\u0002\u0002YZ\u0005\u000e\b\u0002Z[\u0005\u0012\n\u0002[\\\a\a\u0002\u0002\\c\u0003\u0002\u0002\u0002]c\u0005\u0010\t\u0002^_\u0005\n\u0006\u0002_`\a\u0004\u0002\u0002`a\u0005\u0010\t\u0002ac\u0003\u0002\u0002\u0002bM\u0003\u0002\u0002\u0002bQ\u0003\u0002\u0002\u0002bR\u0003\u0002\u0002\u0002bV\u0003\u0002\u0002\u0002bW\u0003\u0002\u0002\u0002bX\u0003\u0002\u0002\u0002b]\u0003\u0002\u0002\u0002b^\u0003\u0002\u0002\u0002cw\u0003\u0002\u0002\u0002de\f\u000e\u0002\u0002ef\a\u0004\u0002\u0002fv\a\u0014\u0002\u0002gh\f\f\u0002\u0002hi\a\t\u0002\u0002ij\a\u0012\u0002\u0002jv\a\n\u0002\u0002kl\f\v\u0002\u0002lm\a\t\u0002\u0002mn\a\u0013\u0002\u0002nv\a\n\u0002\u0002op\f\n\u0002\u0002pq\a\u0004\u0002\u0002qv\u0005\f\a\u0002rs\f\u0004\u0002\u0002st\a\u0004\u0002\u0002tv\u0005\u0010\t\u0002ud\u0003\u0002\u0002\u0002ug\u0003\u0002\u0002\u0002uk\u0003\u0002\u0002\u0002uo\u0003\u0002\u0002\u0002ur\u0003\u0002\u0002\u0002vy\u0003\u0002\u0002\u0002wu\u0003\u0002\u0002\u0002wx\u0003\u0002\u0002\u0002x\u0013\u0003\u0002\u0002\u0002yw\u0003\u0002\u0002\u0002z\u0081\u0005\u0010\t\u0002{\u0081\u0005\u0012\n\u0002|\u0081\u0005\u0006\u0004\u0002}\u0081\u0005\u0004\u0003\u0002~\u0081\a\u0013\u0002\u0002\u007f\u0081\a\u0011\u0002\u0002\u0080z\u0003\u0002\u0002\u0002\u0080{\u0003\u0002\u0002\u0002\u0080|\u0003\u0002\u0002\u0002\u0080}\u0003\u0002\u0002\u0002\u0080~\u0003\u0002\u0002\u0002\u0080\u007f\u0003\u0002\u0002\u0002\u0081\u0015\u0003\u0002\u0002\u0002\f\u001a\u001f4=BHbuw\u0080";

	public static readonly ATN _ATN = new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());

	[NotNull]
	public override IVocabulary Vocabulary => DefaultVocabulary;

	public override string GrammarFileName => "BindingPath.g4";

	public override string[] RuleNames => ruleNames;

	public override string SerializedAtn => _serializedATN;

	public BindingPathParser(ITokenStream input)
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
			path(0);
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
	public Decimal_valueContext decimal_value()
	{
		Decimal_valueContext decimal_valueContext = new Decimal_valueContext(Context, base.State);
		EnterRule(decimal_valueContext, 2, 1);
		try
		{
			EnterOuterAlt(decimal_valueContext, 1);
			base.State = 24;
			int num = base.TokenStream.La(1);
			if (num == 1)
			{
				base.State = 23;
				Match(1);
			}
			base.State = 26;
			Match(16);
			base.State = 29;
			num = base.TokenStream.La(1);
			if (num == 2)
			{
				base.State = 27;
				Match(2);
				base.State = 28;
				Match(16);
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (decimal_valueContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return decimal_valueContext;
	}

	[RuleVersion(0)]
	public Boolean_valueContext boolean_value()
	{
		Boolean_valueContext boolean_valueContext = new Boolean_valueContext(Context, base.State);
		EnterRule(boolean_valueContext, 4, 2);
		try
		{
			EnterOuterAlt(boolean_valueContext, 1);
			base.State = 31;
			int num = base.TokenStream.La(1);
			if (num != 13 && num != 14)
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
			RecognitionException e = (boolean_valueContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return boolean_valueContext;
	}

	[RuleVersion(0)]
	public Namespace_qualifierContext namespace_qualifier()
	{
		Namespace_qualifierContext namespace_qualifierContext = new Namespace_qualifierContext(Context, base.State);
		EnterRule(namespace_qualifierContext, 6, 3);
		try
		{
			EnterOuterAlt(namespace_qualifierContext, 1);
			base.State = 33;
			Match(18);
			base.State = 34;
			Match(3);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (namespace_qualifierContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return namespace_qualifierContext;
	}

	[RuleVersion(0)]
	public Static_typeContext static_type()
	{
		Static_typeContext static_typeContext = new Static_typeContext(Context, base.State);
		EnterRule(static_typeContext, 8, 4);
		try
		{
			EnterOuterAlt(static_typeContext, 1);
			base.State = 36;
			namespace_qualifier();
			base.State = 37;
			Match(18);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (static_typeContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return static_typeContext;
	}

	[RuleVersion(0)]
	public Attached_exprContext attached_expr()
	{
		Attached_exprContext attached_exprContext = new Attached_exprContext(Context, base.State);
		EnterRule(attached_exprContext, 10, 5);
		try
		{
			base.State = 50;
			switch (Interpreter.AdaptivePredict(base.TokenStream, 2, Context))
			{
			case 1:
				EnterOuterAlt(attached_exprContext, 1);
				base.State = 39;
				Match(4);
				base.State = 40;
				static_type();
				base.State = 41;
				Match(2);
				base.State = 42;
				Match(18);
				base.State = 43;
				Match(5);
				break;
			case 2:
				EnterOuterAlt(attached_exprContext, 2);
				base.State = 45;
				Match(4);
				base.State = 46;
				Match(18);
				base.State = 47;
				Match(2);
				base.State = 48;
				Match(18);
				base.State = 49;
				Match(5);
				break;
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (attached_exprContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return attached_exprContext;
	}

	[RuleVersion(0)]
	public Cast_exprContext cast_expr()
	{
		Cast_exprContext cast_exprContext = new Cast_exprContext(Context, base.State);
		EnterRule(cast_exprContext, 12, 6);
		try
		{
			base.State = 59;
			switch (Interpreter.AdaptivePredict(base.TokenStream, 3, Context))
			{
			case 1:
				EnterOuterAlt(cast_exprContext, 1);
				base.State = 52;
				Match(4);
				base.State = 53;
				static_type();
				base.State = 54;
				Match(5);
				break;
			case 2:
				EnterOuterAlt(cast_exprContext, 2);
				base.State = 56;
				Match(4);
				base.State = 57;
				Match(18);
				base.State = 58;
				Match(5);
				break;
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (cast_exprContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return cast_exprContext;
	}

	[RuleVersion(0)]
	public FunctionContext function()
	{
		FunctionContext functionContext = new FunctionContext(Context, base.State);
		EnterRule(functionContext, 14, 7);
		try
		{
			EnterOuterAlt(functionContext, 1);
			base.State = 61;
			Match(18);
			base.State = 62;
			Match(4);
			base.State = 64;
			int num = base.TokenStream.La(1);
			if ((num & -64) == 0 && ((1L << num) & 0x7E012) != 0L)
			{
				base.State = 63;
				function_param();
			}
			base.State = 70;
			ErrorHandler.Sync(this);
			for (num = base.TokenStream.La(1); num == 6; num = base.TokenStream.La(1))
			{
				base.State = 66;
				Match(6);
				base.State = 67;
				function_param();
				base.State = 72;
				ErrorHandler.Sync(this);
			}
			base.State = 73;
			Match(5);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (functionContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return functionContext;
	}

	[RuleVersion(0)]
	public PathContext path()
	{
		return path(0);
	}

	private PathContext path(int _p)
	{
		ParserRuleContext context = Context;
		int state = base.State;
		PathContext pathContext = new PathContext(Context, state);
		PathContext pathContext2 = pathContext;
		int state2 = 16;
		EnterRecursionRule(pathContext, 16, 8, _p);
		try
		{
			EnterOuterAlt(pathContext, 1);
			base.State = 96;
			switch (Interpreter.AdaptivePredict(base.TokenStream, 6, Context))
			{
			case 1:
				pathContext = (PathContext)(Context = new PathCastPathContext(pathContext));
				pathContext2 = pathContext;
				base.State = 76;
				cast_expr();
				base.State = 77;
				path(5);
				break;
			case 2:
				pathContext = (PathContext)(Context = new PathIdentifierContext(pathContext));
				pathContext2 = pathContext;
				base.State = 79;
				Match(18);
				break;
			case 3:
				pathContext = (PathContext)(Context = new PathStaticIdentifierContext(pathContext));
				pathContext2 = pathContext;
				base.State = 80;
				static_type();
				base.State = 81;
				Match(2);
				base.State = 82;
				Match(18);
				break;
			case 4:
				pathContext = (PathContext)(Context = new PathCastInvalidContext(pathContext));
				pathContext2 = pathContext;
				base.State = 84;
				attached_expr();
				break;
			case 5:
				pathContext = (PathContext)(Context = new PathCastContext(pathContext));
				pathContext2 = pathContext;
				base.State = 85;
				cast_expr();
				break;
			case 6:
				pathContext = (PathContext)(Context = new PathCastPathParenContext(pathContext));
				pathContext2 = pathContext;
				base.State = 86;
				Match(4);
				base.State = 87;
				cast_expr();
				base.State = 88;
				path(0);
				base.State = 89;
				Match(5);
				break;
			case 7:
				pathContext = (PathContext)(Context = new PathFunctionContext(pathContext));
				pathContext2 = pathContext;
				base.State = 91;
				function();
				break;
			case 8:
				pathContext = (PathContext)(Context = new PathStaticFuctionContext(pathContext));
				pathContext2 = pathContext;
				base.State = 92;
				static_type();
				base.State = 93;
				Match(2);
				base.State = 94;
				function();
				break;
			}
			Context.Stop = base.TokenStream.Lt(-1);
			base.State = 117;
			ErrorHandler.Sync(this);
			int num = Interpreter.AdaptivePredict(base.TokenStream, 8, Context);
			while (true)
			{
				switch (num)
				{
				case 1:
					if (ParseListeners != null)
					{
						TriggerExitRuleEvent();
					}
					pathContext2 = pathContext;
					base.State = 115;
					switch (Interpreter.AdaptivePredict(base.TokenStream, 7, Context))
					{
					case 1:
						pathContext = new PathDotIdentifierContext(new PathContext(context, state));
						PushNewRecursionContext(pathContext, state2, 8);
						base.State = 98;
						if (!Precpred(Context, 12))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 12)");
						}
						base.State = 99;
						Match(2);
						base.State = 100;
						Match(18);
						break;
					case 2:
						pathContext = new PathIndexerContext(new PathContext(context, state));
						PushNewRecursionContext(pathContext, state2, 8);
						base.State = 101;
						if (!Precpred(Context, 10))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 10)");
						}
						base.State = 102;
						Match(7);
						base.State = 103;
						Match(16);
						base.State = 104;
						Match(8);
						break;
					case 3:
						pathContext = new PathStringIndexerContext(new PathContext(context, state));
						PushNewRecursionContext(pathContext, state2, 8);
						base.State = 105;
						if (!Precpred(Context, 9))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 9)");
						}
						base.State = 106;
						Match(7);
						base.State = 107;
						Match(17);
						base.State = 108;
						Match(8);
						break;
					case 4:
						pathContext = new PathDotAttachedContext(new PathContext(context, state));
						PushNewRecursionContext(pathContext, state2, 8);
						base.State = 109;
						if (!Precpred(Context, 8))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 8)");
						}
						base.State = 110;
						Match(2);
						base.State = 111;
						attached_expr();
						break;
					case 5:
						pathContext = new PathPathToFunctionContext(new PathContext(context, state));
						PushNewRecursionContext(pathContext, state2, 8);
						base.State = 112;
						if (!Precpred(Context, 2))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 2)");
						}
						base.State = 113;
						Match(2);
						base.State = 114;
						function();
						break;
					}
					break;
				case 0:
				case 2:
					goto end_IL_04e6;
				}
				base.State = 119;
				ErrorHandler.Sync(this);
				num = Interpreter.AdaptivePredict(base.TokenStream, 8, Context);
				continue;
				end_IL_04e6:
				break;
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (pathContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			UnrollRecursionContexts(context);
		}
		return pathContext;
	}

	[RuleVersion(0)]
	public Function_paramContext function_param()
	{
		Function_paramContext function_paramContext = new Function_paramContext(Context, base.State);
		EnterRule(function_paramContext, 18, 9);
		try
		{
			base.State = 126;
			switch (Interpreter.AdaptivePredict(base.TokenStream, 9, Context))
			{
			case 1:
				function_paramContext = new FunctionParameterInvalidContext(function_paramContext);
				EnterOuterAlt(function_paramContext, 1);
				base.State = 120;
				function();
				break;
			case 2:
				function_paramContext = new FunctionParamPathContext(function_paramContext);
				EnterOuterAlt(function_paramContext, 2);
				base.State = 121;
				path(0);
				break;
			case 3:
				function_paramContext = new FunctionParamBoolContext(function_paramContext);
				EnterOuterAlt(function_paramContext, 3);
				base.State = 122;
				boolean_value();
				break;
			case 4:
				function_paramContext = new FunctionParamNumberContext(function_paramContext);
				EnterOuterAlt(function_paramContext, 4);
				base.State = 123;
				decimal_value();
				break;
			case 5:
				function_paramContext = new FunctionParamStringContext(function_paramContext);
				EnterOuterAlt(function_paramContext, 5);
				base.State = 124;
				Match(17);
				break;
			case 6:
				function_paramContext = new FunctionParamNullValueContext(function_paramContext);
				EnterOuterAlt(function_paramContext, 6);
				base.State = 125;
				Match(15);
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

	public override bool Sempred(RuleContext _localctx, int ruleIndex, int predIndex)
	{
		if (ruleIndex == 8)
		{
			return path_sempred((PathContext)_localctx, predIndex);
		}
		return true;
	}

	private bool path_sempred(PathContext _localctx, int predIndex)
	{
		return predIndex switch
		{
			0 => Precpred(Context, 12), 
			1 => Precpred(Context, 10), 
			2 => Precpred(Context, 9), 
			3 => Precpred(Context, 8), 
			4 => Precpred(Context, 2), 
			_ => true, 
		};
	}
}
