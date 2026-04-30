namespace System.Xaml;

public interface IXamlLineInfoConsumer
{
	bool ShouldProvideLineInfo { get; }

	void SetLineInfo(int lineNumber, int linePosition);
}
