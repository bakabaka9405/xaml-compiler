using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[DebuggerDisplay("{m_typeSpecToken}")]
internal class TypeSpec : TypeProxy, ITypeSpec, ITypeSignatureBlob, ITypeProxy
{
	private readonly Token m_typeSpecToken;

	private readonly GenericContext m_context;

	public Token TypeSpecToken => m_typeSpecToken;

	public byte[] Blob
	{
		get
		{
			EmbeddedBlobPointer pSig;
			int cbSig;
			int typeSpecFromToken = m_resolver.RawImport.GetTypeSpecFromToken(m_typeSpecToken, out pSig, out cbSig);
			return m_resolver.ReadEmbeddedBlob(pSig, cbSig);
		}
	}

	public Module DeclaringScope => Resolver;

	public TypeSpec(MetadataOnlyModule module, Token typeSpecToken, Type[] typeArgs, Type[] methodArgs)
		: base(module)
	{
		m_typeSpecToken = typeSpecToken;
		m_context = new GenericContext(typeArgs, methodArgs);
	}

	protected override Type GetResolvedTypeWorker()
	{
		byte[] blob = Blob;
		int index = 0;
		return SignatureUtil.ExtractType(blob, ref index, Resolver, m_context);
	}
}
