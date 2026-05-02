using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.UI.Xaml.Markup.Compiler.Core;
using Microsoft.UI.Xaml.Markup.Compiler.Parsing;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class ConditionalNamespace
{
	private static Dictionary<string, ConditionalNamespace> cache = new InstanceCache<string, ConditionalNamespace>();

	public string UnconditionalNamespace { get; }

	public ApiInformation ApiInfo { get; }

	public Platform PlatConditional { get; }

	public ConditionalNamespace(string unconditionalNamespace, ApiInformation apiInfo, Platform targetPlat)
	{
		UnconditionalNamespace = unconditionalNamespace;
		ApiInfo = apiInfo;
		PlatConditional = targetPlat;
	}

	public static ConditionalNamespace Parse(string namespaceFullName)
	{
		if (cache.ContainsKey(namespaceFullName))
		{
			return cache[namespaceFullName];
		}
		AntlrInputStream input = new AntlrInputStream(namespaceFullName);
		ConditionalNamespaceLexer conditionalNamespaceLexer = new ConditionalNamespaceLexer(input);
		CommonTokenStream input2 = new CommonTokenStream(conditionalNamespaceLexer);
		ConditionalNamespaceParser conditionalNamespaceParser = new ConditionalNamespaceParser(input2);
		conditionalNamespaceParser.RemoveErrorListeners();
		conditionalNamespaceParser.AddErrorListener(new ParseErrorListener());
		conditionalNamespaceParser.ErrorHandler = new Microsoft.UI.Xaml.Markup.Compiler.Parsing.BailErrorStrategy();
		ConditionalNamespaceListener listener = new ConditionalNamespaceListener(namespaceFullName);
		ParseTreeWalker parseTreeWalker = new ParseTreeWalker();
		ConditionalNamespaceParser.ExpressionContext expressionContext = conditionalNamespaceParser.expression();
		parseTreeWalker.Walk(listener, expressionContext);
		conditionalNamespaceLexer.ConfirmInputFullyConsumed();
		ConditionalNamespace conditionalNamespace = new ConditionalNamespace(expressionContext.GetChild(0).GetText(), expressionContext.ApiInformation, expressionContext.TargetPlatform);
		cache[namespaceFullName] = conditionalNamespace;
		return conditionalNamespace;
	}
}
