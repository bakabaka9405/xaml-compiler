using System;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class ArrayFabricatedConstructorInfo : MetadataOnlyConstructorInfo
{
	private class Adapter : ArrayFabricatedMethodInfo
	{
		private readonly int m_numParams;

		public override string Name => ".ctor";

		public override MethodAttributes Attributes => MethodAttributes.Public | MethodAttributes.RTSpecialName;

		public override Type ReturnType => base.Universe.GetBuiltInType(CorElementType.Void);

		public Adapter(Type arrayType, int numParams)
			: base(arrayType)
		{
			m_numParams = numParams;
		}

		public override ParameterInfo[] GetParameters()
		{
			ITypeUniverse universe = base.Universe;
			Type builtInType = universe.GetBuiltInType(CorElementType.Int);
			ParameterInfo[] array = new ParameterInfo[m_numParams];
			for (int i = 0; i < m_numParams; i++)
			{
				array[i] = MakeParameterInfo(builtInType, i);
			}
			return array;
		}
	}

	private readonly int m_numParams;

	public ArrayFabricatedConstructorInfo(Type arrayType, int numParams)
		: base(MakeMethodInfo(arrayType, numParams))
	{
		m_numParams = numParams;
	}

	private static MethodInfo MakeMethodInfo(Type arrayType, int numParams)
	{
		return new Adapter(arrayType, numParams);
	}

	public override bool Equals(object obj)
	{
		ArrayFabricatedConstructorInfo arrayFabricatedConstructorInfo = obj as ArrayFabricatedConstructorInfo;
		if (arrayFabricatedConstructorInfo == null)
		{
			return false;
		}
		if (!DeclaringType.Equals(arrayFabricatedConstructorInfo.DeclaringType))
		{
			return false;
		}
		return ToString().Equals(arrayFabricatedConstructorInfo.ToString());
	}

	public override int GetHashCode()
	{
		return DeclaringType.GetHashCode() + m_numParams;
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		return new object[0];
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		return new object[0];
	}
}
