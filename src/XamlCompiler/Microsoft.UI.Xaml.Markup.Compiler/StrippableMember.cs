using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class StrippableMember : ILineNumberAndErrorInfo
{
	public LineNumberInfo LineNumberInfo { get; set; }

	public StrippableMember(XamlDomMember dataTypeMember)
	{
		LineNumberInfo = new LineNumberInfo(dataTypeMember);
	}

	public XamlCompileError GetAttributeProcessingError()
	{
		return new XamlRewriterErrorDataTypeLongForm(LineNumberInfo.StartLineNumber, LineNumberInfo.StartLinePosition);
	}
}
