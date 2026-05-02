using System;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class ArrayFabricatedGetMethodInfo : ArrayFabricatedMethodInfo
{
	public override string Name => "Get";

	public override Type ReturnType => GetElementType();

	public ArrayFabricatedGetMethodInfo(Type arrayType)
		: base(arrayType)
	{
	}

	public override ParameterInfo[] GetParameters()
	{
		return MakeParameterHelper(0);
	}
}
