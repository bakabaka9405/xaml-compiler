using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;

namespace Microsoft.UI.Xaml.Markup.Compiler.Parsing;

internal class ConditionalNamespaceListener : ConditionalNamespaceBaseListener
{
	private readonly string expresion;

	public ConditionalNamespaceListener([NotNull] string expresion)
	{
		this.expresion = expresion;
	}

	public override void ExitExpression([NotNull] ConditionalNamespaceParser.ExpressionContext context)
	{
		context.TargetPlatform = context.query_string().TargetPlatform;
		context.ApiInformation = context.query_string().ApiInformation;
	}

	public override void ExitQuery_string([NotNull] ConditionalNamespaceParser.Query_stringContext context)
	{
		ConditionalNamespaceParser.Query_string_componentContext[] array = context.query_string_component();
		Platform platform = Platform.Any;
		ApiInformation apiInformation = null;
		ConditionalNamespaceParser.Query_string_componentContext[] array2 = array;
		foreach (ConditionalNamespaceParser.Query_string_componentContext query_string_componentContext in array2)
		{
			if (query_string_componentContext.ApiInformation != null)
			{
				if (apiInformation != null)
				{
					throw new ParseException(ErrorMessages.MultipleNamespaceConditionalStatements);
				}
				apiInformation = query_string_componentContext.ApiInformation;
			}
			if (query_string_componentContext.TargetPlatform != Platform.Any)
			{
				if (platform != Platform.Any)
				{
					throw new ParseException(ErrorMessages.MultipleTargetPlatforms);
				}
				platform = query_string_componentContext.TargetPlatform;
			}
		}
		context.ApiInformation = apiInformation;
		context.TargetPlatform = platform;
	}

	public override void ExitQueryStringApiInformation([NotNull] ConditionalNamespaceParser.QueryStringApiInformationContext context)
	{
		context.ApiInformation = context.api_information().ApiInformation;
	}

	public override void ExitQueryStringTargetPlatform([NotNull] ConditionalNamespaceParser.QueryStringTargetPlatformContext context)
	{
		context.TargetPlatform = context.target_platform_func().TargetPlatform;
	}

	public override void ExitTarget_platform_func([NotNull] ConditionalNamespaceParser.Target_platform_funcContext context)
	{
		string text = context.target_platform_value().GetText();
		context.TargetPlatform = (Platform)Enum.Parse(typeof(Platform), text);
	}

	public override void ExitApi_information([NotNull] ConditionalNamespaceParser.Api_informationContext context)
	{
		string text = context.IDENTIFIER().GetText();
		ConditionalNamespaceParser.Function_paramContext[] array = context.function_param();
		List<ApiInformationParameter> list = null;
		if (array.Any())
		{
			list = new List<ApiInformationParameter>();
			ConditionalNamespaceParser.Function_paramContext[] array2 = array;
			foreach (ConditionalNamespaceParser.Function_paramContext function_paramContext in array2)
			{
				list.Add(function_paramContext.ApiInformationParameter);
			}
		}
		try
		{
			context.ApiInformation = new ApiInformation(text);
		}
		catch (ArgumentException)
		{
			throw new ParseException(ErrorMessages.UnrecognizedApiInformation, text);
		}
		try
		{
			context.ApiInformation.SetParameters(list);
		}
		catch (ArgumentException)
		{
			throw new ParseException(ErrorMessages.UnmatchedApiInformationParameters, text);
		}
	}

	public override void ExitFunction_param([NotNull] ConditionalNamespaceParser.Function_paramContext context)
	{
		string text = context.GetText();
		context.ApiInformationParameter = new ApiInformationParameter(text);
	}
}
