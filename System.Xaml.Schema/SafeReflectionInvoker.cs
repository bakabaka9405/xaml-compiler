using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;

namespace System.Xaml.Schema;

internal static class SafeReflectionInvoker
{
	private delegate Delegate CreateDelegate1Delegate(Type delegateType, Type targetType, string methodName);

	private delegate Delegate CreateDelegate2Delegate(Type delegateType, object target, string methodName);

	private delegate object CreateInstanceDelegate(Type type, object[] arguments);

	private delegate object InvokeMethodDelegate(MethodInfo method, object instance, object[] args);

	private static bool s_UseDynamicAssembly = false;

	private static object lockObject = new object();

	private static CreateDelegate1Delegate s_CreateDelegate1;

	private static CreateDelegate2Delegate s_CreateDelegate2;

	private static CreateInstanceDelegate s_CreateInstance;

	private static InvokeMethodDelegate s_InvokeMethod;

	[SecurityCritical]
	private static ReflectionPermission s_reflectionMemberAccess;

	private static readonly Assembly SystemXaml = typeof(SafeReflectionInvoker).Assembly;

	[SecurityCritical]
	private static bool UseDynamicAssembly()
	{
		if (!s_UseDynamicAssembly)
		{
			bool flag = false;
			try
			{
				PermissionSet permissionSet = new PermissionSet(PermissionState.Unrestricted);
				permissionSet.Demand();
			}
			catch (SecurityException)
			{
				flag = true;
			}
			if (flag)
			{
				lock (lockObject)
				{
					if (!s_UseDynamicAssembly)
					{
						CreateDynamicAssembly();
						s_UseDynamicAssembly = true;
					}
				}
			}
		}
		return s_UseDynamicAssembly;
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	[SecurityCritical]
	private static void CreateDynamicAssembly()
	{
		new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Assert();
		Type[] array = new Type[3]
		{
			typeof(Type),
			typeof(Type),
			typeof(string)
		};
		MethodInfo method = typeof(Delegate).GetMethod("CreateDelegate", array);
		DynamicMethod dynamicMethod = new DynamicMethod("CreateDelegate", typeof(Delegate), array);
		dynamicMethod.DefineParameter(1, ParameterAttributes.In, "delegateType");
		dynamicMethod.DefineParameter(2, ParameterAttributes.In, "targetType");
		dynamicMethod.DefineParameter(3, ParameterAttributes.In, "methodName");
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator(5);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_2);
		iLGenerator.EmitCall(OpCodes.Call, method, null);
		iLGenerator.Emit(OpCodes.Ret);
		s_CreateDelegate1 = (CreateDelegate1Delegate)dynamicMethod.CreateDelegate(typeof(CreateDelegate1Delegate));
		array = new Type[3]
		{
			typeof(Type),
			typeof(object),
			typeof(string)
		};
		method = typeof(Delegate).GetMethod("CreateDelegate", array);
		dynamicMethod = new DynamicMethod("CreateDelegate", typeof(Delegate), array);
		dynamicMethod.DefineParameter(1, ParameterAttributes.In, "delegateType");
		dynamicMethod.DefineParameter(2, ParameterAttributes.In, "target");
		dynamicMethod.DefineParameter(3, ParameterAttributes.In, "methodName");
		iLGenerator = dynamicMethod.GetILGenerator(5);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_2);
		iLGenerator.EmitCall(OpCodes.Call, method, null);
		iLGenerator.Emit(OpCodes.Ret);
		s_CreateDelegate2 = (CreateDelegate2Delegate)dynamicMethod.CreateDelegate(typeof(CreateDelegate2Delegate));
		array = new Type[2]
		{
			typeof(Type),
			typeof(object[])
		};
		method = typeof(Activator).GetMethod("CreateInstance", array);
		dynamicMethod = new DynamicMethod("CreateInstance", typeof(object), array);
		dynamicMethod.DefineParameter(1, ParameterAttributes.In, "type");
		dynamicMethod.DefineParameter(2, ParameterAttributes.In, "arguments");
		iLGenerator = dynamicMethod.GetILGenerator(4);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.EmitCall(OpCodes.Call, method, null);
		iLGenerator.Emit(OpCodes.Ret);
		s_CreateInstance = (CreateInstanceDelegate)dynamicMethod.CreateDelegate(typeof(CreateInstanceDelegate));
		array = new Type[2]
		{
			typeof(object),
			typeof(object[])
		};
		Type[] parameterTypes = new Type[3]
		{
			typeof(MethodInfo),
			typeof(object),
			typeof(object[])
		};
		method = typeof(MethodInfo).GetMethod("Invoke", array);
		dynamicMethod = new DynamicMethod("InvokeMethod", typeof(object), parameterTypes);
		dynamicMethod.DefineParameter(1, ParameterAttributes.In, "method");
		dynamicMethod.DefineParameter(2, ParameterAttributes.In, "instance");
		dynamicMethod.DefineParameter(3, ParameterAttributes.In, "args");
		iLGenerator = dynamicMethod.GetILGenerator(5);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_2);
		iLGenerator.EmitCall(OpCodes.Callvirt, method, null);
		iLGenerator.Emit(OpCodes.Ret);
		s_InvokeMethod = (InvokeMethodDelegate)dynamicMethod.CreateDelegate(typeof(InvokeMethodDelegate));
	}

	[SecuritySafeCritical]
	public static bool IsInSystemXaml(Type type)
	{
		if (type.Assembly == SystemXaml)
		{
			return true;
		}
		if (type.IsGenericType)
		{
			Type[] genericArguments = type.GetGenericArguments();
			foreach (Type type2 in genericArguments)
			{
				if (IsInSystemXaml(type2))
				{
					return true;
				}
			}
		}
		return false;
	}

	[SecuritySafeCritical]
	internal static Delegate CreateDelegate(Type delegateType, Type targetType, string methodName)
	{
		if (!UseDynamicAssembly())
		{
			return CreateDelegateCritical(delegateType, targetType, methodName);
		}
		return s_CreateDelegate1(delegateType, targetType, methodName);
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	internal static Delegate CreateDelegateCritical(Type delegateType, Type targetType, string methodName)
	{
		return Delegate.CreateDelegate(delegateType, targetType, methodName);
	}

	[SecuritySafeCritical]
	internal static Delegate CreateDelegate(Type delegateType, object target, string methodName)
	{
		if (!UseDynamicAssembly())
		{
			return CreateDelegateCritical(delegateType, target, methodName);
		}
		return s_CreateDelegate2(delegateType, target, methodName);
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	internal static Delegate CreateDelegateCritical(Type delegateType, object target, string methodName)
	{
		return Delegate.CreateDelegate(delegateType, target, methodName);
	}

	[SecuritySafeCritical]
	internal static object CreateInstance(Type type, object[] arguments)
	{
		if (!UseDynamicAssembly())
		{
			return CreateInstanceCritical(type, arguments);
		}
		return s_CreateInstance(type, arguments);
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	internal static object CreateInstanceCritical(Type type, object[] arguments)
	{
		return Activator.CreateInstance(type, arguments);
	}

	[SecuritySafeCritical]
	internal static void DemandMemberAccessPermission()
	{
		if (s_reflectionMemberAccess == null)
		{
			s_reflectionMemberAccess = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
		}
		s_reflectionMemberAccess.Demand();
	}

	[SecuritySafeCritical]
	internal static object InvokeMethod(MethodInfo method, object instance, object[] args)
	{
		if (!UseDynamicAssembly())
		{
			return InvokeMethodCritical(method, instance, args);
		}
		return s_InvokeMethod(method, instance, args);
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	internal static object InvokeMethodCritical(MethodInfo method, object instance, object[] args)
	{
		return method.Invoke(instance, args);
	}

	[SecuritySafeCritical]
	internal static bool IsSystemXamlNonPublic(MethodInfo method)
	{
		Type declaringType = method.DeclaringType;
		if (IsInSystemXaml(declaringType) && (!method.IsPublic || !declaringType.IsVisible))
		{
			return true;
		}
		if (method.IsGenericMethod)
		{
			Type[] genericArguments = method.GetGenericArguments();
			foreach (Type type in genericArguments)
			{
				if (IsInSystemXaml(type) && !type.IsVisible)
				{
					return true;
				}
			}
		}
		return false;
	}
}
