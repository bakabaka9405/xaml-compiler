using System.ComponentModel;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

[TypeConverter(typeof(LineNumberInfoTypeConverter))]
internal class LineNumberInfo
{
	public int StartLineNumber { get; set; }

	public int StartLinePosition { get; set; }

	public int EndLineNumber { get; set; }

	public int EndLinePosition { get; set; }

	public LineNumberInfo(IXamlDomNode domNode)
	{
		StartLineNumber = domNode.StartLineNumber;
		StartLinePosition = domNode.StartLinePosition;
		EndLineNumber = domNode.EndLineNumber;
		EndLinePosition = domNode.EndLinePosition;
	}
}
