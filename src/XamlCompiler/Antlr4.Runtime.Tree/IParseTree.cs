namespace Antlr4.Runtime.Tree;

public interface IParseTree : ISyntaxTree, ITree
{
	new IParseTree Parent { get; }

	new IParseTree GetChild(int i);

	T Accept<T>(IParseTreeVisitor<T> visitor);

	string GetText();

	string ToStringTree(Parser parser);
}
