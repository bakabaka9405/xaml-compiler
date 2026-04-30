namespace System.Xaml;

public interface IXamlLineInfo
{
	bool HasLineInfo { get; }

	int LineNumber { get; }

	int LinePosition { get; }
}
