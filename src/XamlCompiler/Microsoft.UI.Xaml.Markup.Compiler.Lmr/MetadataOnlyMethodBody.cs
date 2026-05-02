using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyMethodBody : MethodBody
{
	private readonly MetadataOnlyMethodInfo m_method;

	protected MetadataOnlyMethodInfo Method => m_method;

	public override IList<ExceptionHandlingClause> ExceptionHandlingClauses
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override bool InitLocals
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public override int LocalSignatureMetadataToken
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public override IList<LocalVariableInfo> LocalVariables
	{
		get
		{
			Token token = new Token(LocalSignatureMetadataToken);
			EmbeddedBlobPointer pSig = default(EmbeddedBlobPointer);
			int cbSig = 0;
			if (!token.IsNil)
			{
				m_method.Resolver.RawImport.GetSigFromToken(token, out pSig, out cbSig);
			}
			if (cbSig == 0)
			{
				return new MetadataOnlyLocalVariableInfo[0];
			}
			GenericContext context = new GenericContext(m_method);
			byte[] sig = m_method.Resolver.ReadEmbeddedBlob(pSig, cbSig);
			int index = 0;
			CorCallingConvention corCallingConvention = SignatureUtil.ExtractCallingConvention(sig, ref index);
			int num = SignatureUtil.ExtractInt(sig, ref index);
			MetadataOnlyLocalVariableInfo[] array = new MetadataOnlyLocalVariableInfo[num];
			for (int i = 0; i < num; i++)
			{
				TypeSignatureDescriptor typeSignatureDescriptor = SignatureUtil.ExtractType(sig, ref index, m_method.Resolver, context, fAllowPinned: true);
				array[i] = new MetadataOnlyLocalVariableInfo(i, typeSignatureDescriptor.Type, typeSignatureDescriptor.IsPinned);
			}
			return array;
		}
	}

	public override int MaxStackSize
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	protected MetadataOnlyMethodBody(MetadataOnlyMethodInfo method)
	{
		m_method = method;
	}

	internal static MethodBody TryCreate(MetadataOnlyMethodInfo method)
	{
		MetadataOnlyModule resolver = method.Resolver;
		MethodBody body = null;
		if (resolver.Factory.TryCreateMethodBody(method, ref body))
		{
			return body;
		}
		return MetadataOnlyMethodBodyWorker.Create(method);
	}

	public override byte[] GetILAsByteArray()
	{
		throw new InvalidOperationException();
	}
}
