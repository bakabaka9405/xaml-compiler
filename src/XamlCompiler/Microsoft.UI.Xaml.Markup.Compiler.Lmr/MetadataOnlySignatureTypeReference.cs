using System.Diagnostics;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[DebuggerDisplay("\\{Name = {Name} FullName = {FullName} ElementType = {m_elemType} {m_typeRef}\\}")]
internal class MetadataOnlySignatureTypeReference : MetadataOnlyTypeReference
{
	private CorElementType m_elemType;

	public MetadataOnlySignatureTypeReference(MetadataOnlyModule resolver, Token typeRef, CorElementType elemType)
		: base(resolver, typeRef)
	{
		m_elemType = elemType;
	}

	protected override bool IsValueTypeImpl()
	{
		return m_elemType == CorElementType.ValueType;
	}
}
