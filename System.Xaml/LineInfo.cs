namespace System.Xaml;

internal class LineInfo
{
	private int _lineNumber;

	private int _linePosition;

	public int LineNumber => _lineNumber;

	public int LinePosition => _linePosition;

	internal LineInfo(int lineNumber, int linePosition)
	{
		_lineNumber = lineNumber;
		_linePosition = linePosition;
	}
}
