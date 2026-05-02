using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.5.1")]
public interface ISuccinctCollectionSyntaxListener : IParseTreeListener
{
	void EnterProgram([NotNull] SuccinctCollectionSyntaxParser.ProgramContext context);

	void ExitProgram([NotNull] SuccinctCollectionSyntaxParser.ProgramContext context);

	void EnterItems([NotNull] SuccinctCollectionSyntaxParser.ItemsContext context);

	void ExitItems([NotNull] SuccinctCollectionSyntaxParser.ItemsContext context);

	void EnterZ([NotNull] SuccinctCollectionSyntaxParser.ZContext context);

	void ExitZ([NotNull] SuccinctCollectionSyntaxParser.ZContext context);

	void EnterItem([NotNull] SuccinctCollectionSyntaxParser.ItemContext context);

	void ExitItem([NotNull] SuccinctCollectionSyntaxParser.ItemContext context);

	void EnterText([NotNull] SuccinctCollectionSyntaxParser.TextContext context);

	void ExitText([NotNull] SuccinctCollectionSyntaxParser.TextContext context);

	void EnterLiteral_text([NotNull] SuccinctCollectionSyntaxParser.Literal_textContext context);

	void ExitLiteral_text([NotNull] SuccinctCollectionSyntaxParser.Literal_textContext context);

	void EnterSequence([NotNull] SuccinctCollectionSyntaxParser.SequenceContext context);

	void ExitSequence([NotNull] SuccinctCollectionSyntaxParser.SequenceContext context);
}
