using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using System.Xaml;
using System.Xaml.Permissions;
using System.Xaml.Schema;

namespace MS.Internal.Xaml.Runtime;

[SecurityCritical]
internal class DynamicMethodRuntime : ClrObjectRuntime
{
	private delegate void PropertySetDelegate(object target, object value);

	private delegate object PropertyGetDelegate(object target);

	private delegate object FactoryDelegate(object[] args);

	private delegate Delegate DelegateCreator(Type delegateType, object target, string methodName);

	private const BindingFlags BF_AllInstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private const BindingFlags BF_AllStaticMembers = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private static PermissionSet s_FullTrustPermission;

	private static MethodInfo s_GetTypeFromHandleMethod;

	private static MethodInfo s_InvokeMemberMethod;

	private XamlLoadPermission _xamlLoadPermission;

	private Assembly _localAssembly;

	private Type _localType;

	private XamlSchemaContext _schemaContext;

	private Dictionary<MethodInfo, PropertyGetDelegate> _propertyGetDelegates;

	private Dictionary<MethodInfo, PropertySetDelegate> _propertySetDelegates;

	private Dictionary<MethodBase, FactoryDelegate> _factoryDelegates;

	private Dictionary<Type, object> _converterInstances;

	private Dictionary<Type, DelegateCreator> _delegateCreators;

	private DelegateCreator _delegateCreatorWithoutHelper;

	private Dictionary<MethodInfo, PropertyGetDelegate> PropertyGetDelegates
	{
		get
		{
			if (_propertyGetDelegates == null)
			{
				_propertyGetDelegates = new Dictionary<MethodInfo, PropertyGetDelegate>();
			}
			return _propertyGetDelegates;
		}
	}

	private Dictionary<MethodInfo, PropertySetDelegate> PropertySetDelegates
	{
		get
		{
			if (_propertySetDelegates == null)
			{
				_propertySetDelegates = new Dictionary<MethodInfo, PropertySetDelegate>();
			}
			return _propertySetDelegates;
		}
	}

	private Dictionary<MethodBase, FactoryDelegate> FactoryDelegates
	{
		get
		{
			if (_factoryDelegates == null)
			{
				_factoryDelegates = new Dictionary<MethodBase, FactoryDelegate>();
			}
			return _factoryDelegates;
		}
	}

	private Dictionary<Type, object> ConverterInstances
	{
		get
		{
			if (_converterInstances == null)
			{
				_converterInstances = new Dictionary<Type, object>();
			}
			return _converterInstances;
		}
	}

	private Dictionary<Type, DelegateCreator> DelegateCreators
	{
		get
		{
			if (_delegateCreators == null)
			{
				_delegateCreators = new Dictionary<Type, DelegateCreator>();
			}
			return _delegateCreators;
		}
	}

	[SecurityCritical]
	internal DynamicMethodRuntime(XamlRuntimeSettings settings, XamlSchemaContext schemaContext, XamlAccessLevel accessLevel)
		: base(settings, isWriter: true)
	{
		_schemaContext = schemaContext;
		_xamlLoadPermission = new XamlLoadPermission(accessLevel);
		_localAssembly = Assembly.Load(accessLevel.AssemblyAccessToAssemblyName);
		if (accessLevel.PrivateAccessToTypeName != null)
		{
			_localType = _localAssembly.GetType(accessLevel.PrivateAccessToTypeName, throwOnError: true);
		}
	}

	[SecuritySafeCritical]
	public override TConverterBase GetConverterInstance<TConverterBase>(XamlValueConverter<TConverterBase> ts)
	{
		DemandXamlLoadPermission();
		Type converterType = ts.ConverterType;
		if (converterType == null)
		{
			return null;
		}
		if (!ConverterInstances.TryGetValue(converterType, out var value))
		{
			value = CreateInstanceWithCtor(converterType, null);
			ConverterInstances.Add(converterType, value);
		}
		return (TConverterBase)value;
	}

	[SecuritySafeCritical]
	public override object CreateFromValue(ServiceProviderContext serviceContext, XamlValueConverter<TypeConverter> ts, object value, XamlMember property)
	{
		if (ts == BuiltInValueConverter.Event && value is string methodName)
		{
			EventConverter.GetRootObjectAndDelegateType(serviceContext, out var rootObject, out var delegateType);
			return CreateDelegate(delegateType, rootObject, methodName);
		}
		return base.CreateFromValue(serviceContext, ts, value, property);
	}

	[SecuritySafeCritical]
	protected override Delegate CreateDelegate(Type delegateType, object target, string methodName)
	{
		DemandXamlLoadPermission();
		Type type = target.GetType();
		if (!DelegateCreators.TryGetValue(type, out var value))
		{
			value = CreateDelegateCreator(type);
			DelegateCreators.Add(type, value);
		}
		return value(delegateType, target, methodName);
	}

	[SecuritySafeCritical]
	protected override object CreateInstanceWithCtor(XamlType xamlType, object[] args)
	{
		DemandXamlLoadPermission();
		return CreateInstanceWithCtor(xamlType.UnderlyingType, args);
	}

	private object CreateInstanceWithCtor(Type type, object[] args)
	{
		ConstructorInfo constructorInfo = null;
		if (args == null || args.Length == 0)
		{
			constructorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
		}
		if (constructorInfo == null)
		{
			ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			MethodBase[] candidates = constructors;
			constructorInfo = (ConstructorInfo)BindToMethod(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, candidates, args);
		}
		if (!FactoryDelegates.TryGetValue(constructorInfo, out var value))
		{
			value = CreateFactoryDelegate(constructorInfo);
			FactoryDelegates.Add(constructorInfo, value);
		}
		return value(args);
	}

	[SecuritySafeCritical]
	protected override object InvokeFactoryMethod(Type type, string methodName, object[] args)
	{
		DemandXamlLoadPermission();
		MethodInfo factoryMethod = GetFactoryMethod(type, methodName, args, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (!FactoryDelegates.TryGetValue(factoryMethod, out var value))
		{
			value = CreateFactoryDelegate(factoryMethod);
			FactoryDelegates.Add(factoryMethod, value);
		}
		return value(args);
	}

	[SecuritySafeCritical]
	protected override object GetValue(XamlMember member, object obj)
	{
		DemandXamlLoadPermission();
		MethodInfo underlyingGetter = member.Invoker.UnderlyingGetter;
		if (underlyingGetter == null)
		{
			throw new NotSupportedException(SR.Get("CantGetWriteonlyProperty", member));
		}
		if (!PropertyGetDelegates.TryGetValue(underlyingGetter, out var value))
		{
			value = CreateGetDelegate(underlyingGetter);
			PropertyGetDelegates.Add(underlyingGetter, value);
		}
		return value(obj);
	}

	[SecuritySafeCritical]
	protected override void SetValue(XamlMember member, object obj, object value)
	{
		DemandXamlLoadPermission();
		MethodInfo underlyingSetter = member.Invoker.UnderlyingSetter;
		if (underlyingSetter == null)
		{
			throw new NotSupportedException(SR.Get("CantSetReadonlyProperty", member));
		}
		if (!PropertySetDelegates.TryGetValue(underlyingSetter, out var value2))
		{
			value2 = CreateSetDelegate(underlyingSetter);
			PropertySetDelegates.Add(underlyingSetter, value2);
		}
		value2(obj, value);
	}

	private DelegateCreator CreateDelegateCreator(Type targetType)
	{
		MethodInfo method = targetType.GetMethod("_CreateDelegate", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[2]
		{
			typeof(Type),
			typeof(string)
		}, null);
		if (method == null)
		{
			if (_delegateCreatorWithoutHelper == null)
			{
				_delegateCreatorWithoutHelper = CreateDelegateCreatorWithoutHelper();
			}
			return _delegateCreatorWithoutHelper;
		}
		DynamicMethod dynamicMethod = CreateDynamicMethod(targetType.Name + "DelegateHelper", typeof(Delegate), typeof(Type), typeof(object), typeof(string));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		Emit_LateBoundInvoke(iLGenerator, targetType, "_CreateDelegate", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, 1, 0, 2);
		Emit_CastTo(iLGenerator, typeof(Delegate));
		iLGenerator.Emit(OpCodes.Ret);
		return (DelegateCreator)dynamicMethod.CreateDelegate(typeof(DelegateCreator));
	}

	private DelegateCreator CreateDelegateCreatorWithoutHelper()
	{
		DynamicMethod dynamicMethod = CreateDynamicMethod("CreateDelegateHelper", typeof(Delegate), typeof(Type), typeof(object), typeof(string));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_2);
		MethodInfo method = typeof(Delegate).GetMethod("CreateDelegate", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
		{
			typeof(Type),
			typeof(object),
			typeof(string)
		}, null);
		iLGenerator.Emit(OpCodes.Call, method);
		iLGenerator.Emit(OpCodes.Ret);
		return (DelegateCreator)dynamicMethod.CreateDelegate(typeof(DelegateCreator));
	}

	private FactoryDelegate CreateFactoryDelegate(ConstructorInfo ctor)
	{
		DynamicMethod dynamicMethod = CreateDynamicMethod(ctor.DeclaringType.Name + "Ctor", typeof(object), typeof(object[]));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		LocalBuilder[] locals = LoadArguments(iLGenerator, ctor);
		iLGenerator.Emit(OpCodes.Newobj, ctor);
		UnloadArguments(iLGenerator, locals);
		iLGenerator.Emit(OpCodes.Ret);
		return (FactoryDelegate)dynamicMethod.CreateDelegate(typeof(FactoryDelegate));
	}

	private FactoryDelegate CreateFactoryDelegate(MethodInfo factory)
	{
		DynamicMethod dynamicMethod = CreateDynamicMethod(factory.Name + "Factory", typeof(object), typeof(object[]));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		LocalBuilder[] locals = LoadArguments(iLGenerator, factory);
		iLGenerator.Emit(OpCodes.Call, factory);
		Emit_BoxIfValueType(iLGenerator, factory.ReturnType);
		UnloadArguments(iLGenerator, locals);
		iLGenerator.Emit(OpCodes.Ret);
		return (FactoryDelegate)dynamicMethod.CreateDelegate(typeof(FactoryDelegate));
	}

	private LocalBuilder[] LoadArguments(ILGenerator ilGenerator, MethodBase method)
	{
		ParameterInfo[] parameters = method.GetParameters();
		if (parameters.Length == 0)
		{
			return null;
		}
		ParameterInfo[] parameters2 = method.GetParameters();
		Type[] array = new Type[parameters2.Length];
		LocalBuilder[] array2 = new LocalBuilder[array.Length];
		for (int i = 0; i < parameters2.Length; i++)
		{
			Type parameterType = parameters2[i].ParameterType;
			ilGenerator.Emit(OpCodes.Ldarg_0);
			Emit_ConstInt(ilGenerator, i);
			ilGenerator.Emit(OpCodes.Ldelem_Ref);
			if (parameterType.IsByRef)
			{
				Type elementType = parameterType.GetElementType();
				Emit_CastTo(ilGenerator, elementType);
				array2[i] = ilGenerator.DeclareLocal(elementType);
				ilGenerator.Emit(OpCodes.Stloc, array2[i]);
				ilGenerator.Emit(OpCodes.Ldloca_S, array2[i]);
			}
			else
			{
				Emit_CastTo(ilGenerator, parameterType);
			}
		}
		return array2;
	}

	private void UnloadArguments(ILGenerator ilGenerator, LocalBuilder[] locals)
	{
		if (locals == null)
		{
			return;
		}
		for (int i = 0; i < locals.Length; i++)
		{
			if (locals[i] != null)
			{
				ilGenerator.Emit(OpCodes.Ldarg_0);
				Emit_ConstInt(ilGenerator, i);
				ilGenerator.Emit(OpCodes.Ldloc, locals[i]);
				Emit_BoxIfValueType(ilGenerator, locals[i].LocalType);
				ilGenerator.Emit(OpCodes.Stelem_Ref);
			}
		}
	}

	private PropertyGetDelegate CreateGetDelegate(MethodInfo getter)
	{
		DynamicMethod dynamicMethod = CreateDynamicMethod(getter.Name + "Getter", typeof(object), typeof(object));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		Type toType = (getter.IsStatic ? getter.GetParameters()[0].ParameterType : GetTargetType(getter));
		iLGenerator.Emit(OpCodes.Ldarg_0);
		Emit_CastTo(iLGenerator, toType);
		Emit_Call(iLGenerator, getter);
		Emit_BoxIfValueType(iLGenerator, getter.ReturnType);
		iLGenerator.Emit(OpCodes.Ret);
		return (PropertyGetDelegate)dynamicMethod.CreateDelegate(typeof(PropertyGetDelegate));
	}

	private PropertySetDelegate CreateSetDelegate(MethodInfo setter)
	{
		DynamicMethod dynamicMethod = CreateDynamicMethod(setter.Name + "Setter", typeof(void), typeof(object), typeof(object));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		ParameterInfo[] parameters = setter.GetParameters();
		Type toType = (setter.IsStatic ? parameters[0].ParameterType : GetTargetType(setter));
		Type toType2 = (setter.IsStatic ? parameters[1].ParameterType : parameters[0].ParameterType);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		Emit_CastTo(iLGenerator, toType);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		Emit_CastTo(iLGenerator, toType2);
		Emit_Call(iLGenerator, setter);
		iLGenerator.Emit(OpCodes.Ret);
		return (PropertySetDelegate)dynamicMethod.CreateDelegate(typeof(PropertySetDelegate));
	}

	private DynamicMethod CreateDynamicMethod(string name, Type returnType, params Type[] argTypes)
	{
		if (s_FullTrustPermission == null)
		{
			s_FullTrustPermission = new PermissionSet(PermissionState.Unrestricted);
		}
		s_FullTrustPermission.Assert();
		try
		{
			if (_localType != null)
			{
				return new DynamicMethod(name, returnType, argTypes, _localType);
			}
			return new DynamicMethod(name, returnType, argTypes, _localAssembly.ManifestModule);
		}
		finally
		{
			CodeAccessPermission.RevertAssert();
		}
	}

	private void DemandXamlLoadPermission()
	{
		_xamlLoadPermission.Demand();
	}

	private Type GetTargetType(MethodInfo instanceMethod)
	{
		Type declaringType = instanceMethod.DeclaringType;
		if (_localType != null && _localType != declaringType && declaringType.IsAssignableFrom(_localType))
		{
			if (instanceMethod.IsFamily || instanceMethod.IsFamilyAndAssembly)
			{
				return _localType;
			}
			if (instanceMethod.IsFamilyOrAssembly && !_schemaContext.AreInternalsVisibleTo(declaringType.Assembly, _localType.Assembly))
			{
				return _localType;
			}
		}
		return declaringType;
	}

	private static void Emit_Call(ILGenerator ilGenerator, MethodInfo method)
	{
		OpCode opcode = ((method.IsStatic || method.DeclaringType.IsValueType) ? OpCodes.Call : OpCodes.Callvirt);
		ilGenerator.Emit(opcode, method);
	}

	private static void Emit_CastTo(ILGenerator ilGenerator, Type toType)
	{
		if (toType.IsValueType)
		{
			ilGenerator.Emit(OpCodes.Unbox_Any, toType);
		}
		else
		{
			ilGenerator.Emit(OpCodes.Castclass, toType);
		}
	}

	private static void Emit_BoxIfValueType(ILGenerator ilGenerator, Type type)
	{
		if (type.IsValueType)
		{
			ilGenerator.Emit(OpCodes.Box, type);
		}
	}

	private static void Emit_ConstInt(ILGenerator ilGenerator, int value)
	{
		switch (value)
		{
		case -1:
			ilGenerator.Emit(OpCodes.Ldc_I4_M1);
			break;
		case 0:
			ilGenerator.Emit(OpCodes.Ldc_I4_0);
			break;
		case 1:
			ilGenerator.Emit(OpCodes.Ldc_I4_1);
			break;
		case 2:
			ilGenerator.Emit(OpCodes.Ldc_I4_2);
			break;
		case 3:
			ilGenerator.Emit(OpCodes.Ldc_I4_3);
			break;
		case 4:
			ilGenerator.Emit(OpCodes.Ldc_I4_4);
			break;
		case 5:
			ilGenerator.Emit(OpCodes.Ldc_I4_5);
			break;
		case 6:
			ilGenerator.Emit(OpCodes.Ldc_I4_6);
			break;
		case 7:
			ilGenerator.Emit(OpCodes.Ldc_I4_7);
			break;
		case 8:
			ilGenerator.Emit(OpCodes.Ldc_I4_8);
			break;
		default:
			ilGenerator.Emit(OpCodes.Ldc_I4, value);
			break;
		}
	}

	private void Emit_LateBoundInvoke(ILGenerator ilGenerator, Type targetType, string methodName, BindingFlags bindingFlags, short targetArgNum, params short[] paramArgNums)
	{
		Emit_TypeOf(ilGenerator, targetType);
		ilGenerator.Emit(OpCodes.Ldstr, methodName);
		Emit_ConstInt(ilGenerator, (int)bindingFlags);
		ilGenerator.Emit(OpCodes.Ldnull);
		ilGenerator.Emit(OpCodes.Ldarg, targetArgNum);
		LocalBuilder local = ilGenerator.DeclareLocal(typeof(object[]));
		Emit_ConstInt(ilGenerator, paramArgNums.Length);
		ilGenerator.Emit(OpCodes.Newarr, typeof(object));
		ilGenerator.Emit(OpCodes.Stloc, local);
		for (int i = 0; i < paramArgNums.Length; i++)
		{
			ilGenerator.Emit(OpCodes.Ldloc, local);
			Emit_ConstInt(ilGenerator, i);
			ilGenerator.Emit(OpCodes.Ldarg, paramArgNums[i]);
			ilGenerator.Emit(OpCodes.Stelem_Ref);
		}
		ilGenerator.Emit(OpCodes.Ldloc, local);
		if (s_InvokeMemberMethod == null)
		{
			s_InvokeMemberMethod = typeof(Type).GetMethod("InvokeMember", new Type[5]
			{
				typeof(string),
				typeof(BindingFlags),
				typeof(Binder),
				typeof(object),
				typeof(object[])
			});
		}
		ilGenerator.Emit(OpCodes.Callvirt, s_InvokeMemberMethod);
	}

	private void Emit_TypeOf(ILGenerator ilGenerator, Type type)
	{
		ilGenerator.Emit(OpCodes.Ldtoken, type);
		if (s_GetTypeFromHandleMethod == null)
		{
			s_GetTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(RuntimeTypeHandle) }, null);
		}
		ilGenerator.Emit(OpCodes.Call, s_GetTypeFromHandleMethod);
	}
}
