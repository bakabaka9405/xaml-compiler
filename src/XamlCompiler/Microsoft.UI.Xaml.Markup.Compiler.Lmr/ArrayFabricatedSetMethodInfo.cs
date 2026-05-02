using System;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class ArrayFabricatedSetMethodInfo : ArrayFabricatedMethodInfo
{
	public override string Name => "Set";

	public override Type ReturnType => base.Universe.GetBuiltInType(CorElementType.Void);

	public ArrayFabricatedSetMethodInfo(Type arrayType)
		: base(arrayType)
	{
	}

	public override ParameterInfo[] GetParameters()
	{
		ParameterInfo[] array = MakeParameterHelper(1);
		int rank = base.Rank;
		Type elementType = GetElementType();
		array[rank] = MakeParameterInfo(elementType, rank);
		return array;
	}
}
