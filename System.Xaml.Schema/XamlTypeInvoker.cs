using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Windows.Markup;

namespace System.Xaml.Schema;

public class XamlTypeInvoker
{
	private static class DefaultCtorXamlActivator
	{
		private static ThreeValuedBool s_securityFailureWithCtorDelegate;

		private static ConstructorInfo s_actionCtor = typeof(Action<object>).GetConstructor(new Type[2]
		{
			typeof(object),
			typeof(IntPtr)
		});

		public static object CreateInstance(XamlTypeInvoker type)
		{
			if (!EnsureConstructorDelegate(type))
			{
				return null;
			}
			return CallCtorDelegate(type);
		}

		[SecuritySafeCritical]
		private static object CallCtorDelegate(XamlTypeInvoker type)
		{
			object uninitializedObject = FormatterServices.GetUninitializedObject(type._xamlType.UnderlyingType);
			InvokeDelegate(type._constructorDelegate, uninitializedObject);
			return uninitializedObject;
		}

		private static void InvokeDelegate(Action<object> action, object argument)
		{
			action(argument);
		}

		[SecuritySafeCritical]
		private static bool EnsureConstructorDelegate(XamlTypeInvoker type)
		{
			if (type._constructorDelegate != null)
			{
				return true;
			}
			if (!type.IsPublic)
			{
				return false;
			}
			if (s_securityFailureWithCtorDelegate == ThreeValuedBool.NotSet)
			{
				s_securityFailureWithCtorDelegate = (AppDomain.CurrentDomain.PermissionSet.IsUnrestricted() ? ThreeValuedBool.False : ThreeValuedBool.True);
			}
			if (s_securityFailureWithCtorDelegate == ThreeValuedBool.True)
			{
				return false;
			}
			try
			{
				Type underlyingSystemType = type._xamlType.UnderlyingType.UnderlyingSystemType;
				ConstructorInfo constructor = underlyingSystemType.GetConstructor(Type.EmptyTypes);
				if (constructor == null)
				{
					throw new MissingMethodException(SR.Get("NoDefaultConstructor", underlyingSystemType.FullName));
				}
				if ((constructor.IsSecurityCritical && !constructor.IsSecuritySafeCritical) || (constructor.Attributes & MethodAttributes.HasSecurity) == MethodAttributes.HasSecurity || (underlyingSystemType.Attributes & TypeAttributes.HasSecurity) == TypeAttributes.HasSecurity)
				{
					type._isPublic = ThreeValuedBool.False;
					return false;
				}
				IntPtr functionPointer = constructor.MethodHandle.GetFunctionPointer();
				Action<object> constructorDelegate = (constructorDelegate = (Action<object>)s_actionCtor.Invoke(new object[2] { null, functionPointer }));
				type._constructorDelegate = constructorDelegate;
				return true;
			}
			catch (SecurityException)
			{
				s_securityFailureWithCtorDelegate = ThreeValuedBool.True;
				return false;
			}
		}
	}

	private class UnknownTypeInvoker : XamlTypeInvoker
	{
		public override void AddToCollection(object instance, object item)
		{
			throw new NotSupportedException(SR.Get("NotSupportedOnUnknownType"));
		}

		public override void AddToDictionary(object instance, object key, object item)
		{
			throw new NotSupportedException(SR.Get("NotSupportedOnUnknownType"));
		}

		public override object CreateInstance(object[] arguments)
		{
			throw new NotSupportedException(SR.Get("NotSupportedOnUnknownType"));
		}

		public override IEnumerator GetItems(object instance)
		{
			throw new NotSupportedException(SR.Get("NotSupportedOnUnknownType"));
		}
	}

	private static XamlTypeInvoker s_Unknown;

	private static object[] s_emptyObjectArray = new object[0];

	private Dictionary<XamlType, MethodInfo> _addMethods;

	private XamlType _xamlType;

	[SecurityCritical]
	private Action<object> _constructorDelegate;

	[SecurityCritical]
	private ThreeValuedBool _isPublic;

	[SecurityCritical]
	private ThreeValuedBool _isInSystemXaml;

	internal MethodInfo EnumeratorMethod { get; set; }

	public static XamlTypeInvoker UnknownInvoker
	{
		get
		{
			if (s_Unknown == null)
			{
				s_Unknown = new XamlTypeInvoker();
			}
			return s_Unknown;
		}
	}

	public EventHandler<XamlSetMarkupExtensionEventArgs> SetMarkupExtensionHandler
	{
		get
		{
			if (!(_xamlType != null))
			{
				return null;
			}
			return _xamlType.SetMarkupExtensionHandler;
		}
	}

	public EventHandler<XamlSetTypeConverterEventArgs> SetTypeConverterHandler
	{
		get
		{
			if (!(_xamlType != null))
			{
				return null;
			}
			return _xamlType.SetTypeConverterHandler;
		}
	}

	private bool IsInSystemXaml
	{
		[SecuritySafeCritical]
		get
		{
			if (_isInSystemXaml == ThreeValuedBool.NotSet)
			{
				Type underlyingSystemType = _xamlType.UnderlyingType.UnderlyingSystemType;
				bool flag = SafeReflectionInvoker.IsInSystemXaml(underlyingSystemType);
				_isInSystemXaml = ((!flag) ? ThreeValuedBool.False : ThreeValuedBool.True);
			}
			return _isInSystemXaml == ThreeValuedBool.True;
		}
	}

	private bool IsPublic
	{
		[SecuritySafeCritical]
		get
		{
			if (_isPublic == ThreeValuedBool.NotSet)
			{
				Type underlyingSystemType = _xamlType.UnderlyingType.UnderlyingSystemType;
				_isPublic = ((!underlyingSystemType.IsVisible) ? ThreeValuedBool.False : ThreeValuedBool.True);
			}
			return _isPublic == ThreeValuedBool.True;
		}
	}

	private bool IsUnknown
	{
		get
		{
			if (!(_xamlType == null))
			{
				return _xamlType.UnderlyingType == null;
			}
			return true;
		}
	}

	protected XamlTypeInvoker()
	{
	}

	public XamlTypeInvoker(XamlType type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		_xamlType = type;
	}

	public virtual void AddToCollection(object instance, object item)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if (instance is IList list)
		{
			list.Add(item);
			return;
		}
		ThrowIfUnknown();
		if (!_xamlType.IsCollection)
		{
			throw new NotSupportedException(SR.Get("OnlySupportedOnCollections"));
		}
		XamlType xamlType = ((item == null) ? _xamlType.ItemType : _xamlType.SchemaContext.GetXamlType(item.GetType()));
		MethodInfo addMethod = GetAddMethod(xamlType);
		if (addMethod == null)
		{
			throw new XamlSchemaException(SR.Get("NoAddMethodFound", _xamlType, xamlType));
		}
		SafeReflectionInvoker.InvokeMethod(addMethod, instance, new object[1] { item });
	}

	public virtual void AddToDictionary(object instance, object key, object item)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if (instance is IDictionary dictionary)
		{
			dictionary.Add(key, item);
			return;
		}
		ThrowIfUnknown();
		if (!_xamlType.IsDictionary)
		{
			throw new NotSupportedException(SR.Get("OnlySupportedOnDictionaries"));
		}
		XamlType xamlType = ((item == null) ? _xamlType.ItemType : _xamlType.SchemaContext.GetXamlType(item.GetType()));
		MethodInfo addMethod = GetAddMethod(xamlType);
		if (addMethod == null)
		{
			throw new XamlSchemaException(SR.Get("NoAddMethodFound", _xamlType, xamlType));
		}
		SafeReflectionInvoker.InvokeMethod(addMethod, instance, new object[2] { key, item });
	}

	public virtual object CreateInstance(object[] arguments)
	{
		ThrowIfUnknown();
		if (!_xamlType.UnderlyingType.IsValueType && (arguments == null || arguments.Length == 0))
		{
			object obj = DefaultCtorXamlActivator.CreateInstance(this);
			if (obj != null)
			{
				return obj;
			}
		}
		return CreateInstanceWithActivator(_xamlType.UnderlyingType, arguments);
	}

	public virtual MethodInfo GetAddMethod(XamlType contentType)
	{
		if (contentType == null)
		{
			throw new ArgumentNullException("contentType");
		}
		if (IsUnknown || _xamlType.ItemType == null)
		{
			return null;
		}
		if (contentType == _xamlType.ItemType || (_xamlType.AllowedContentTypes.Count == 1 && contentType.CanAssignTo(_xamlType.ItemType)))
		{
			return _xamlType.AddMethod;
		}
		if (!_xamlType.IsCollection)
		{
			return null;
		}
		MethodInfo value;
		if (_addMethods == null)
		{
			Dictionary<XamlType, MethodInfo> dictionary = new Dictionary<XamlType, MethodInfo>();
			dictionary.Add(_xamlType.ItemType, _xamlType.AddMethod);
			foreach (XamlType allowedContentType in _xamlType.AllowedContentTypes)
			{
				value = CollectionReflector.GetAddMethod(_xamlType.UnderlyingType, allowedContentType.UnderlyingType);
				if (value != null)
				{
					dictionary.Add(allowedContentType, value);
				}
			}
			_addMethods = dictionary;
		}
		if (_addMethods.TryGetValue(contentType, out value))
		{
			return value;
		}
		foreach (KeyValuePair<XamlType, MethodInfo> addMethod in _addMethods)
		{
			if (contentType.CanAssignTo(addMethod.Key))
			{
				return addMethod.Value;
			}
		}
		return null;
	}

	public virtual MethodInfo GetEnumeratorMethod()
	{
		return _xamlType.GetEnumeratorMethod;
	}

	public virtual IEnumerator GetItems(object instance)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if (instance is IEnumerable enumerable)
		{
			return enumerable.GetEnumerator();
		}
		ThrowIfUnknown();
		if (!_xamlType.IsCollection && !_xamlType.IsDictionary)
		{
			throw new NotSupportedException(SR.Get("OnlySupportedOnCollectionsAndDictionaries"));
		}
		MethodInfo enumeratorMethod = GetEnumeratorMethod();
		return (IEnumerator)SafeReflectionInvoker.InvokeMethod(enumeratorMethod, instance, s_emptyObjectArray);
	}

	[SecuritySafeCritical]
	private object CreateInstanceWithActivator(Type type, object[] arguments)
	{
		return SafeReflectionInvoker.CreateInstance(type, arguments);
	}

	private void ThrowIfUnknown()
	{
		if (IsUnknown)
		{
			throw new NotSupportedException(SR.Get("NotSupportedOnUnknownType"));
		}
	}
}
