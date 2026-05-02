namespace Antlr4.Runtime.Tree;

public interface ITree
{
	ITree Parent { get; }

	object Payload { get; }

	int ChildCount { get; }

	ITree GetChild(int i);

	string ToStringTree();
}
