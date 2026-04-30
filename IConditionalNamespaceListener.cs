using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.5.3")]
public interface IConditionalNamespaceListener : IParseTreeListener
{
	void EnterProgram([NotNull] ConditionalNamespaceParser.ProgramContext context);

	void ExitProgram([NotNull] ConditionalNamespaceParser.ProgramContext context);

	void EnterExpression([NotNull] ConditionalNamespaceParser.ExpressionContext context);

	void ExitExpression([NotNull] ConditionalNamespaceParser.ExpressionContext context);

	void EnterUri([NotNull] ConditionalNamespaceParser.UriContext context);

	void ExitUri([NotNull] ConditionalNamespaceParser.UriContext context);

	void EnterUnquoted_namespace([NotNull] ConditionalNamespaceParser.Unquoted_namespaceContext context);

	void ExitUnquoted_namespace([NotNull] ConditionalNamespaceParser.Unquoted_namespaceContext context);

	void EnterApi_information([NotNull] ConditionalNamespaceParser.Api_informationContext context);

	void ExitApi_information([NotNull] ConditionalNamespaceParser.Api_informationContext context);

	void EnterFunction_param([NotNull] ConditionalNamespaceParser.Function_paramContext context);

	void ExitFunction_param([NotNull] ConditionalNamespaceParser.Function_paramContext context);

	void EnterTarget_platform_value([NotNull] ConditionalNamespaceParser.Target_platform_valueContext context);

	void ExitTarget_platform_value([NotNull] ConditionalNamespaceParser.Target_platform_valueContext context);

	void EnterQuery_string([NotNull] ConditionalNamespaceParser.Query_stringContext context);

	void ExitQuery_string([NotNull] ConditionalNamespaceParser.Query_stringContext context);

	void EnterQueryStringTargetPlatform([NotNull] ConditionalNamespaceParser.QueryStringTargetPlatformContext context);

	void ExitQueryStringTargetPlatform([NotNull] ConditionalNamespaceParser.QueryStringTargetPlatformContext context);

	void EnterQueryStringApiInformation([NotNull] ConditionalNamespaceParser.QueryStringApiInformationContext context);

	void ExitQueryStringApiInformation([NotNull] ConditionalNamespaceParser.QueryStringApiInformationContext context);

	void EnterTarget_platform_func([NotNull] ConditionalNamespaceParser.Target_platform_funcContext context);

	void ExitTarget_platform_func([NotNull] ConditionalNamespaceParser.Target_platform_funcContext context);
}
