namespace System.Xaml;

public interface IXamlIndexingReader
{
	int Count { get; }

	int CurrentIndex { get; set; }
}
