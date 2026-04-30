using System.Xaml;
using MS.Internal.Xaml.Runtime;

namespace MS.Internal.Xaml.Context;

internal class FixupTarget : IAddLineInfo
{
	public XamlMember Property { get; set; }

	public object Instance { get; set; }

	public string InstanceName { get; set; }

	public XamlType InstanceType { get; set; }

	public int TemporaryCollectionIndex { get; set; }

	public int EndInstanceLineNumber { get; set; }

	public int EndInstanceLinePosition { get; set; }

	public FixupTargetKeyHolder KeyHolder { get; set; }

	public bool InstanceIsOnTheStack { get; set; }

	public bool InstanceWasGotten { get; set; }

	XamlException IAddLineInfo.WithLineInfo(XamlException ex)
	{
		if (EndInstanceLineNumber > 0)
		{
			ex.SetLineInfo(EndInstanceLineNumber, EndInstanceLinePosition);
		}
		return ex;
	}
}
