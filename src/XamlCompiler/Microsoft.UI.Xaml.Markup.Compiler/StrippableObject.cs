using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class StrippableObject : ILineNumberAndErrorInfo
{
	public LineNumberInfo LineNumberInfo { get; set; }

	public StrippableObject(XamlDomObject obj)
	{
		LineNumberInfo = new LineNumberInfo(obj);
	}

	public XamlCompileError GetAttributeProcessingError()
	{
		return new XamlRewriterErrorDataTypeLongForm(LineNumberInfo.StartLineNumber, LineNumberInfo.StartLinePosition);
	}
}
