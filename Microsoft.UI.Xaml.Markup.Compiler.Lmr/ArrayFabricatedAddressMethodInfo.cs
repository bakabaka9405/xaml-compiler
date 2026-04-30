using System;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class ArrayFabricatedAddressMethodInfo : ArrayFabricatedMethodInfo
{
	public override string Name => "Address";

	public override Type ReturnType => GetElementType().MakeByRefType();

	public ArrayFabricatedAddressMethodInfo(Type arrayType)
		: base(arrayType)
	{
	}

	public override ParameterInfo[] GetParameters()
	{
		return MakeParameterHelper(0);
	}
}
