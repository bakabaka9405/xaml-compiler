using System;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class PhaseAssignment : ILineNumberAndErrorInfo, IComparable<PhaseAssignment>
{
	public ConnectionIdElement ConnectionIdElement { get; private set; }

	public LineNumberInfo LineNumberInfo { get; set; }

	public int Phase { get; set; }

	public PhaseAssignment(XamlDomMember phaseMember, ConnectionIdElement connectionIdElement)
	{
		LineNumberInfo = new LineNumberInfo(phaseMember);
		ConnectionIdElement = connectionIdElement;
		string stringValueOfProperty = DomHelper.GetStringValueOfProperty(phaseMember);
		int result = 0;
		int.TryParse(stringValueOfProperty, out result);
		Phase = result;
	}

	public XamlCompileError GetAttributeProcessingError()
	{
		return new XamlRewriterErrorDataTypeLongForm(LineNumberInfo.StartLineNumber, LineNumberInfo.StartLinePosition);
	}

	public int CompareTo(PhaseAssignment other)
	{
		return Phase - other.Phase;
	}
}
