using System;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyLocalVariableInfo : LocalVariableInfo
{
	private readonly Type m_type;

	private readonly int m_index;

	private readonly bool m_fPinned;

	public override bool IsPinned => m_fPinned;

	public override int LocalIndex => m_index;

	public override Type LocalType => m_type;

	public MetadataOnlyLocalVariableInfo(int index, Type type, bool fPinned)
	{
		m_type = type;
		m_index = index;
		m_fPinned = fPinned;
	}
}
