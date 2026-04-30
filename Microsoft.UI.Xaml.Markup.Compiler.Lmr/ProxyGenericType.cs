using System;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class ProxyGenericType : TypeProxy
{
	private readonly TypeProxy m_rawType;

	private readonly Type[] m_args;

	public override string Name => m_rawType.Name;

	public override string Namespace => m_rawType.Namespace;

	public override Type DeclaringType => m_rawType.DeclaringType;

	public override bool IsGenericParameter => false;

	public override bool IsGenericType => true;

	public override bool IsGenericTypeDefinition => false;

	public override bool IsEnum => m_rawType.IsEnum;

	public override Module Module => m_rawType.Module;

	public override Assembly Assembly => m_rawType.Assembly;

	public ProxyGenericType(TypeProxy rawType, Type[] args)
		: base(rawType.Resolver)
	{
		m_rawType = rawType;
		m_args = args;
	}

	protected override Type GetResolvedTypeWorker()
	{
		return m_rawType.GetResolvedType().MakeGenericType(m_args);
	}

	public override Type[] GetGenericArguments()
	{
		return (Type[])m_args.Clone();
	}

	public override Type GetGenericTypeDefinition()
	{
		return m_rawType;
	}

	protected override bool IsPointerImpl()
	{
		return false;
	}

	protected override bool IsArrayImpl()
	{
		return false;
	}

	protected override bool IsByRefImpl()
	{
		return false;
	}

	protected override bool IsValueTypeImpl()
	{
		return m_rawType.IsValueType;
	}

	protected override bool IsPrimitiveImpl()
	{
		return false;
	}
}
