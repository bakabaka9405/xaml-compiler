using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.5.1")]
public interface IBindingPathListener : IParseTreeListener
{
	void EnterProgram([NotNull] BindingPathParser.ProgramContext context);

	void ExitProgram([NotNull] BindingPathParser.ProgramContext context);

	void EnterDecimal_value([NotNull] BindingPathParser.Decimal_valueContext context);

	void ExitDecimal_value([NotNull] BindingPathParser.Decimal_valueContext context);

	void EnterBoolean_value([NotNull] BindingPathParser.Boolean_valueContext context);

	void ExitBoolean_value([NotNull] BindingPathParser.Boolean_valueContext context);

	void EnterNamespace_qualifier([NotNull] BindingPathParser.Namespace_qualifierContext context);

	void ExitNamespace_qualifier([NotNull] BindingPathParser.Namespace_qualifierContext context);

	void EnterStatic_type([NotNull] BindingPathParser.Static_typeContext context);

	void ExitStatic_type([NotNull] BindingPathParser.Static_typeContext context);

	void EnterAttached_expr([NotNull] BindingPathParser.Attached_exprContext context);

	void ExitAttached_expr([NotNull] BindingPathParser.Attached_exprContext context);

	void EnterCast_expr([NotNull] BindingPathParser.Cast_exprContext context);

	void ExitCast_expr([NotNull] BindingPathParser.Cast_exprContext context);

	void EnterFunction([NotNull] BindingPathParser.FunctionContext context);

	void ExitFunction([NotNull] BindingPathParser.FunctionContext context);

	void EnterPathStaticFuction([NotNull] BindingPathParser.PathStaticFuctionContext context);

	void ExitPathStaticFuction([NotNull] BindingPathParser.PathStaticFuctionContext context);

	void EnterPathCast([NotNull] BindingPathParser.PathCastContext context);

	void ExitPathCast([NotNull] BindingPathParser.PathCastContext context);

	void EnterPathPathToFunction([NotNull] BindingPathParser.PathPathToFunctionContext context);

	void ExitPathPathToFunction([NotNull] BindingPathParser.PathPathToFunctionContext context);

	void EnterPathIndexer([NotNull] BindingPathParser.PathIndexerContext context);

	void ExitPathIndexer([NotNull] BindingPathParser.PathIndexerContext context);

	void EnterPathCastInvalid([NotNull] BindingPathParser.PathCastInvalidContext context);

	void ExitPathCastInvalid([NotNull] BindingPathParser.PathCastInvalidContext context);

	void EnterPathCastPathParen([NotNull] BindingPathParser.PathCastPathParenContext context);

	void ExitPathCastPathParen([NotNull] BindingPathParser.PathCastPathParenContext context);

	void EnterPathFunction([NotNull] BindingPathParser.PathFunctionContext context);

	void ExitPathFunction([NotNull] BindingPathParser.PathFunctionContext context);

	void EnterPathStringIndexer([NotNull] BindingPathParser.PathStringIndexerContext context);

	void ExitPathStringIndexer([NotNull] BindingPathParser.PathStringIndexerContext context);

	void EnterPathIdentifier([NotNull] BindingPathParser.PathIdentifierContext context);

	void ExitPathIdentifier([NotNull] BindingPathParser.PathIdentifierContext context);

	void EnterPathCastPath([NotNull] BindingPathParser.PathCastPathContext context);

	void ExitPathCastPath([NotNull] BindingPathParser.PathCastPathContext context);

	void EnterPathStaticIdentifier([NotNull] BindingPathParser.PathStaticIdentifierContext context);

	void ExitPathStaticIdentifier([NotNull] BindingPathParser.PathStaticIdentifierContext context);

	void EnterPathDotIdentifier([NotNull] BindingPathParser.PathDotIdentifierContext context);

	void ExitPathDotIdentifier([NotNull] BindingPathParser.PathDotIdentifierContext context);

	void EnterPathDotAttached([NotNull] BindingPathParser.PathDotAttachedContext context);

	void ExitPathDotAttached([NotNull] BindingPathParser.PathDotAttachedContext context);

	void EnterFunctionParameterInvalid([NotNull] BindingPathParser.FunctionParameterInvalidContext context);

	void ExitFunctionParameterInvalid([NotNull] BindingPathParser.FunctionParameterInvalidContext context);

	void EnterFunctionParamPath([NotNull] BindingPathParser.FunctionParamPathContext context);

	void ExitFunctionParamPath([NotNull] BindingPathParser.FunctionParamPathContext context);

	void EnterFunctionParamBool([NotNull] BindingPathParser.FunctionParamBoolContext context);

	void ExitFunctionParamBool([NotNull] BindingPathParser.FunctionParamBoolContext context);

	void EnterFunctionParamNumber([NotNull] BindingPathParser.FunctionParamNumberContext context);

	void ExitFunctionParamNumber([NotNull] BindingPathParser.FunctionParamNumberContext context);

	void EnterFunctionParamString([NotNull] BindingPathParser.FunctionParamStringContext context);

	void ExitFunctionParamString([NotNull] BindingPathParser.FunctionParamStringContext context);

	void EnterFunctionParamNullValue([NotNull] BindingPathParser.FunctionParamNullValueContext context);

	void ExitFunctionParamNullValue([NotNull] BindingPathParser.FunctionParamNullValueContext context);
}
