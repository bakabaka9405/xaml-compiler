using System.Reflection;
using System.Security;

namespace System.Xaml.Schema;

public class XamlMemberInvoker
{
	private class DirectiveMemberInvoker : XamlMemberInvoker
	{
		public override object GetValue(object instance)
		{
			throw new NotSupportedException(SR.Get("NotSupportedOnDirective"));
		}

		public override void SetValue(object instance, object value)
		{
			throw new NotSupportedException(SR.Get("NotSupportedOnDirective"));
		}
	}

	private static XamlMemberInvoker s_Directive;

	private static XamlMemberInvoker s_Unknown;

	private static object[] s_emptyObjectArray = new object[0];

	private XamlMember _member;

	private NullableReference<MethodInfo> _shouldSerializeMethod;

	public static XamlMemberInvoker UnknownInvoker
	{
		get
		{
			if (s_Unknown == null)
			{
				s_Unknown = new XamlMemberInvoker();
			}
			return s_Unknown;
		}
	}

	public MethodInfo UnderlyingGetter
	{
		get
		{
			if (!IsUnknown)
			{
				return _member.Getter;
			}
			return null;
		}
	}

	public MethodInfo UnderlyingSetter
	{
		get
		{
			if (!IsUnknown)
			{
				return _member.Setter;
			}
			return null;
		}
	}

	internal static XamlMemberInvoker DirectiveInvoker
	{
		get
		{
			if (s_Directive == null)
			{
				s_Directive = new DirectiveMemberInvoker();
			}
			return s_Directive;
		}
	}

	private bool IsUnknown
	{
		get
		{
			if (!(_member == null))
			{
				return _member.UnderlyingMember == null;
			}
			return true;
		}
	}

	protected XamlMemberInvoker()
	{
	}

	public XamlMemberInvoker(XamlMember member)
	{
		if (member == null)
		{
			throw new ArgumentNullException("member");
		}
		_member = member;
	}

	public virtual object GetValue(object instance)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		ThrowIfUnknown();
		if (UnderlyingGetter == null)
		{
			throw new NotSupportedException(SR.Get("CantGetWriteonlyProperty", _member));
		}
		return GetValueSafeCritical(instance);
	}

	[SecuritySafeCritical]
	private object GetValueSafeCritical(object instance)
	{
		if (UnderlyingGetter.IsStatic)
		{
			return SafeReflectionInvoker.InvokeMethod(UnderlyingGetter, null, new object[1] { instance });
		}
		return SafeReflectionInvoker.InvokeMethod(UnderlyingGetter, instance, s_emptyObjectArray);
	}

	public virtual void SetValue(object instance, object value)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		ThrowIfUnknown();
		if (UnderlyingSetter == null)
		{
			throw new NotSupportedException(SR.Get("CantSetReadonlyProperty", _member));
		}
		SetValueSafeCritical(instance, value);
	}

	[SecuritySafeCritical]
	private void SetValueSafeCritical(object instance, object value)
	{
		if (UnderlyingSetter.IsStatic)
		{
			SafeReflectionInvoker.InvokeMethod(UnderlyingSetter, null, new object[2] { instance, value });
		}
		else
		{
			SafeReflectionInvoker.InvokeMethod(UnderlyingSetter, instance, new object[1] { value });
		}
	}

	public virtual ShouldSerializeResult ShouldSerializeValue(object instance)
	{
		if (IsUnknown)
		{
			return ShouldSerializeResult.Default;
		}
		if (!_shouldSerializeMethod.IsSet)
		{
			Type declaringType = _member.UnderlyingMember.DeclaringType;
			string name = "ShouldSerialize" + _member.Name;
			BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			Type[] types;
			if (_member.IsAttachable)
			{
				types = new Type[1] { _member.TargetType.UnderlyingType ?? typeof(object) };
			}
			else
			{
				bindingFlags |= BindingFlags.Instance;
				types = Type.EmptyTypes;
			}
			_shouldSerializeMethod.Value = declaringType.GetMethod(name, bindingFlags, null, types, null);
		}
		MethodInfo value = _shouldSerializeMethod.Value;
		if (value != null)
		{
			if (!((!_member.IsAttachable) ? ((bool)value.Invoke(instance, null)) : ((bool)value.Invoke(null, new object[1] { instance }))))
			{
				return ShouldSerializeResult.False;
			}
			return ShouldSerializeResult.True;
		}
		return ShouldSerializeResult.Default;
	}

	[SecuritySafeCritical]
	private static bool IsSystemXamlNonPublic(ref ThreeValuedBool methodIsSystemXamlNonPublic, MethodInfo method)
	{
		if (methodIsSystemXamlNonPublic == ThreeValuedBool.NotSet)
		{
			bool flag = SafeReflectionInvoker.IsSystemXamlNonPublic(method);
			methodIsSystemXamlNonPublic = ((!flag) ? ThreeValuedBool.False : ThreeValuedBool.True);
		}
		return methodIsSystemXamlNonPublic == ThreeValuedBool.True;
	}

	private void ThrowIfUnknown()
	{
		if (IsUnknown)
		{
			throw new NotSupportedException(SR.Get("NotSupportedOnUnknownMember"));
		}
	}
}
