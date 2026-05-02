using System;
using System.Reflection;
using System.Reflection.Adds;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class GenericContext
{
	public Type[] TypeArgs { get; protected set; }

	public Type[] MethodArgs { get; protected set; }

	public GenericContext(Type[] typeArgs, Type[] methodArgs)
	{
		TypeArgs = ((typeArgs == null) ? Type.EmptyTypes : typeArgs);
		MethodArgs = ((methodArgs == null) ? Type.EmptyTypes : methodArgs);
	}

	public GenericContext(MethodBase methodTypeContext)
		: this(methodTypeContext.DeclaringType.GetGenericArguments(), methodTypeContext.GetGenericArguments())
	{
	}

	public override bool Equals(object obj)
	{
		GenericContext genericContext = (GenericContext)obj;
		if (genericContext == null)
		{
			return false;
		}
		if (IsArrayEqual(TypeArgs, genericContext.TypeArgs))
		{
			return IsArrayEqual(MethodArgs, genericContext.MethodArgs);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return GetArrayHashCode(TypeArgs) * 32768 + GetArrayHashCode(MethodArgs);
	}

	public virtual GenericContext VerifyAndUpdateMethodArguments(int expectedNumberOfMethodArgs)
	{
		if (MethodArgs.Length != expectedNumberOfMethodArgs)
		{
			throw new ArgumentException(Resources.InvalidMetadataSignature);
		}
		return this;
	}

	private static int GetArrayHashCode<T>(T[] a)
	{
		int num = 0;
		for (int i = 0; i < a.Length; i++)
		{
			num += a[i].GetHashCode() * i;
		}
		return num;
	}

	private static bool IsArrayEqual<T>(T[] a1, T[] a2) where T : Type
	{
		if (a1.Length == a2.Length)
		{
			for (int i = 0; i < a1.Length; i++)
			{
				if (!a1[i].Equals(a2[i]))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private static string ArrayToString<T>(T[] a)
	{
		if (a == null)
		{
			return "empty";
		}
		StringBuilder builder = StringBuilderPool.Get();
		for (int i = 0; i < a.Length; i++)
		{
			if (i != 0)
			{
				builder.Append(",");
			}
			builder.Append(a[i]);
		}
		string result = builder.ToString();
		StringBuilderPool.Release(ref builder);
		return result;
	}

	public override string ToString()
	{
		return "Type: " + ArrayToString(TypeArgs) + ", Method: " + ArrayToString(MethodArgs);
	}

	public GenericContext DeleteMethodArgs()
	{
		if (MethodArgs.Length == 0)
		{
			return this;
		}
		return new GenericContext(TypeArgs, null);
	}

	public static bool IsNullOrEmptyMethodArgs(GenericContext context)
	{
		if (context == null || context.MethodArgs.Length == 0)
		{
			return true;
		}
		return false;
	}

	public static bool IsNullOrEmptyTypeArgs(GenericContext context)
	{
		if (context == null || context.TypeArgs.Length == 0)
		{
			return true;
		}
		return false;
	}
}
