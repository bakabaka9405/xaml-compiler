using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class StrippableNamespace : ILineNumberAndErrorInfo
{
	public LineNumberInfo LineNumberInfo { get; set; }

	public bool StripWholeNamespace { get; }

	public StrippableNamespace(XamlDomNamespace nameSpace, bool stripWholeNamespace)
	{
		LineNumberInfo = new LineNumberInfo(nameSpace);
		StripWholeNamespace = stripWholeNamespace;
	}

	public XamlCompileError GetAttributeProcessingError()
	{
		return new XamlRewriterErrorDataTypeLongForm(LineNumberInfo.StartLineNumber, LineNumberInfo.StartLinePosition);
	}
}
