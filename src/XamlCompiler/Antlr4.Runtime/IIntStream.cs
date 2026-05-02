namespace Antlr4.Runtime;

public interface IIntStream
{
	int Index { get; }

	int Size { get; }

	string SourceName { get; }

	void Consume();

	int La(int i);

	int Mark();

	void Release(int marker);

	void Seek(int index);
}
