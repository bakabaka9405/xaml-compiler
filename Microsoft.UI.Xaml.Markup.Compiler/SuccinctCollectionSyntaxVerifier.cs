using System;
using System.Collections.Generic;
using System.Xaml;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.UI.Xaml.Markup.Compiler.Parsing;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;
using SuccinctCollectionSyntax;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class SuccinctCollectionSyntaxVerifier
{
	public static bool TryParse(string collectionItem, XamlDomNode locationForErrors, List<XamlCompileError> Errors, XamlMember property)
	{
		try
		{
			Parse(collectionItem);
		}
		catch (SuccinctCollectionSyntaxException ex)
		{
			int startLineNumber = locationForErrors.StartLineNumber;
			int line = ex.Line;
			int line2 = startLineNumber + line - 1;
			int endLinePosition = locationForErrors.EndLinePosition;
			int col = ex.Col;
			int col2 = endLinePosition + col;
			Errors.Add(new XamlSuccinctSyntaxError(line2, col2, ex.OffendingToken, locationForErrors.SourceFilePath));
			return false;
		}
		catch (Exception)
		{
			Errors.Add(new XamlValidationErrorCannotAssignTextToProperty(locationForErrors, property, collectionItem));
			return false;
		}
		return true;
	}

	public static void Parse(string bindPath)
	{
		AntlrInputStream input = new AntlrInputStream(bindPath);
		SuccinctCollectionSyntaxLexer succinctCollectionSyntaxLexer = new SuccinctCollectionSyntaxLexer(input);
		CommonTokenStream input2 = new CommonTokenStream(succinctCollectionSyntaxLexer);
		SuccinctCollectionSyntaxParser succinctCollectionSyntaxParser = new SuccinctCollectionSyntaxParser(input2);
		succinctCollectionSyntaxParser.RemoveErrorListeners();
		succinctCollectionSyntaxParser.AddErrorListener(new SuccinctCollectionSyntaxErrorListener());
		succinctCollectionSyntaxParser.ErrorHandler = new Microsoft.UI.Xaml.Markup.Compiler.Parsing.BailErrorStrategy();
		SuccinctCollectionSyntaxBaseListener listener = new SuccinctCollectionSyntaxBaseListener();
		ParseTreeWalker parseTreeWalker = new ParseTreeWalker();
		parseTreeWalker.Walk(listener, succinctCollectionSyntaxParser.items());
		succinctCollectionSyntaxLexer.ConfirmInputFullyConsumed();
	}
}
