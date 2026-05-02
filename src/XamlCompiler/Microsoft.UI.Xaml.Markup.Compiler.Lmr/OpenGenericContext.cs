using System;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class OpenGenericContext : GenericContext
{
	private readonly MetadataOnlyModule m_resolver;

	private readonly Token m_ownerMethod;

	public OpenGenericContext(Type[] typeArgs, Type[] methodArgs)
		: base(typeArgs, methodArgs)
	{
	}

	public OpenGenericContext(MetadataOnlyModule resolver, Type ownerType, Token ownerMethod)
		: base(null, null)
	{
		m_resolver = resolver;
		m_ownerMethod = ownerMethod;
		int num = ownerType.GetGenericArguments().Length;
		Type[] array = new Type[num];
		Token ownerToken = new Token(ownerType.MetadataToken);
		for (int i = 0; i < num; i++)
		{
			array[i] = new MetadataOnlyTypeVariableRef(resolver, ownerToken, i);
		}
		base.TypeArgs = array;
	}

	public override GenericContext VerifyAndUpdateMethodArguments(int expectedNumberOfMethodArgs)
	{
		if (expectedNumberOfMethodArgs != base.MethodArgs.Length)
		{
			Type[] array = new Type[expectedNumberOfMethodArgs];
			for (int i = 0; i < expectedNumberOfMethodArgs; i++)
			{
				array[i] = new MetadataOnlyTypeVariableRef(m_resolver, m_ownerMethod, i);
			}
			return new OpenGenericContext(base.TypeArgs, array);
		}
		return this;
	}
}
