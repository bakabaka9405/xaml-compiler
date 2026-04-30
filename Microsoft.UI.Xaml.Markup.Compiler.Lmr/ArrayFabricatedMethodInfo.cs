using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal abstract class ArrayFabricatedMethodInfo : MethodInfo
{
	private Type m_arrayType;

	protected ITypeUniverse Universe => Helpers.Universe(m_arrayType);

	protected int Rank => m_arrayType.GetArrayRank();

	public override ICustomAttributeProvider ReturnTypeCustomAttributes
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override ParameterInfo ReturnParameter => MakeParameterInfo(ReturnType, -1);

	public override MethodAttributes Attributes => MethodAttributes.Public;

	public override CallingConventions CallingConvention => CallingConventions.Standard | CallingConventions.HasThis;

	public override bool IsGenericMethodDefinition => false;

	public override bool ContainsGenericParameters => GetElementType().IsGenericParameter;

	public override RuntimeMethodHandle MethodHandle
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override MemberTypes MemberType => MemberTypes.Method;

	public override Type DeclaringType => m_arrayType;

	public override int MetadataToken => new Token(TokenType.MethodDef, 0);

	public override Module Module => DeclaringType.Module;

	public override Type ReflectedType
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	protected ArrayFabricatedMethodInfo(Type arrayType)
	{
		m_arrayType = arrayType;
	}

	protected Type GetElementType()
	{
		return m_arrayType.GetElementType();
	}

	protected ParameterInfo[] MakeParameterHelper(int extra)
	{
		int rank = Rank;
		ITypeUniverse universe = Universe;
		Type builtInType = universe.GetBuiltInType(CorElementType.Int);
		ParameterInfo[] array = new ParameterInfo[rank + extra];
		for (int i = 0; i < rank; i++)
		{
			array[i] = MakeParameterInfo(builtInType, i);
		}
		return array;
	}

	protected ParameterInfo MakeParameterInfo(Type t, int position)
	{
		return new SimpleParameterInfo(this, t, position);
	}

	public override MethodInfo GetBaseDefinition()
	{
		return this;
	}

	public override MethodInfo MakeGenericMethod(params Type[] types)
	{
		throw new InvalidOperationException();
	}

	public override Type[] GetGenericArguments()
	{
		return Type.EmptyTypes;
	}

	public override MethodBody GetMethodBody()
	{
		return null;
	}

	public override MethodImplAttributes GetMethodImplementationFlags()
	{
		return MethodImplAttributes.IL;
	}

	public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		return new object[0];
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		return new object[0];
	}

	public override bool IsDefined(Type attributeType, bool inherit)
	{
		throw new NotImplementedException();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return new CustomAttributeData[0];
	}

	public override string ToString()
	{
		return MetadataOnlyMethodInfo.CommonToString(this);
	}

	public override bool Equals(object obj)
	{
		ArrayFabricatedMethodInfo arrayFabricatedMethodInfo = obj as ArrayFabricatedMethodInfo;
		if (arrayFabricatedMethodInfo == null)
		{
			return false;
		}
		if (!DeclaringType.Equals(arrayFabricatedMethodInfo.DeclaringType))
		{
			return false;
		}
		return Name.Equals(arrayFabricatedMethodInfo.Name);
	}

	public override int GetHashCode()
	{
		return DeclaringType.GetHashCode() + Name.GetHashCode();
	}
}
