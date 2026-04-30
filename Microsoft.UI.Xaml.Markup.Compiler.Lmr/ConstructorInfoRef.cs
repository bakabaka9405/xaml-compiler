using System;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class ConstructorInfoRef : ConstructorInfoProxy
{
	private readonly Type m_declaringType;

	private readonly Token m_token;

	private readonly MetadataOnlyModule m_scope;

	public override Type DeclaringType => m_declaringType;

	public ConstructorInfoRef(Type declaringType, MetadataOnlyModule scope, Token token)
	{
		m_declaringType = declaringType;
		m_token = token;
		m_scope = scope;
	}

	protected override ConstructorInfo GetResolvedWorker()
	{
		MethodBase methodBase = m_scope.ResolveMethod(m_token);
		return (ConstructorInfo)methodBase;
	}

	public ParameterInfo[] GetSignatureParameters()
	{
		m_scope.GetMemberRefData(m_token, out var _, out var _, out var sig);
		GenericContext context = new GenericContext(null, null);
		MethodSignatureDescriptor methodSignatureDescriptor = SignatureUtil.ExtractMethodSignature(sig, m_scope, context);
		ParameterInfo[] array = new SimpleParameterInfo[methodSignatureDescriptor.Parameters.Length];
		ParameterInfo[] array2 = array;
		for (int i = 0; i < methodSignatureDescriptor.Parameters.Length; i++)
		{
			array2[i] = new SignatureParameterInfo(this, methodSignatureDescriptor.Parameters[i].Type, i, methodSignatureDescriptor.Parameters[i].CustomModifiers);
		}
		return array2;
	}
}
