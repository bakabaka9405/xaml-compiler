using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xaml;

namespace System.Windows.Markup;

[TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[TypeConverter(typeof(StaticExtensionConverter))]
[MarkupExtensionReturnType(typeof(object))]
public class StaticExtension : MarkupExtension
{
	private string _member;

	private Type _memberType;

	[ConstructorArgument("member")]
	public string Member
	{
		get
		{
			return _member;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_member = value;
		}
	}

	[DefaultValue(null)]
	public Type MemberType
	{
		get
		{
			return _memberType;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_memberType = value;
		}
	}

	public StaticExtension()
	{
	}

	public StaticExtension(string member)
	{
		if (member == null)
		{
			throw new ArgumentNullException("member");
		}
		_member = member;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (_member == null)
		{
			throw new InvalidOperationException(SR.Get("MarkupExtensionStaticMember"));
		}
		Type type = MemberType;
		string text = null;
		string text2 = null;
		if (type != null)
		{
			text = _member;
			text2 = type.FullName + "." + _member;
		}
		else
		{
			text2 = _member;
			int num = _member.IndexOf('.');
			if (num < 0)
			{
				throw new ArgumentException(SR.Get("MarkupExtensionBadStatic", _member));
			}
			string text3 = _member.Substring(0, num);
			if (text3 == string.Empty)
			{
				throw new ArgumentException(SR.Get("MarkupExtensionBadStatic", _member));
			}
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver xamlTypeResolver))
			{
				throw new ArgumentException(SR.Get("MarkupExtensionNoContext", GetType().Name, "IXamlTypeResolver"));
			}
			type = xamlTypeResolver.Resolve(text3);
			text = _member.Substring(num + 1, _member.Length - num - 1);
			if (text == string.Empty)
			{
				throw new ArgumentException(SR.Get("MarkupExtensionBadStatic", _member));
			}
		}
		if (type.IsEnum)
		{
			return Enum.Parse(type, text);
		}
		if (GetFieldOrPropertyValue(type, text, out var value))
		{
			return value;
		}
		throw new ArgumentException(SR.Get("MarkupExtensionBadStatic", text2));
	}

	private bool GetFieldOrPropertyValue(Type type, string name, out object value)
	{
		FieldInfo fieldInfo = null;
		Type type2 = type;
		do
		{
			fieldInfo = type2.GetField(name, BindingFlags.Static | BindingFlags.Public);
			if (fieldInfo != null)
			{
				value = fieldInfo.GetValue(null);
				return true;
			}
			type2 = type2.BaseType;
		}
		while (type2 != null);
		PropertyInfo propertyInfo = null;
		type2 = type;
		do
		{
			propertyInfo = type2.GetProperty(name, BindingFlags.Static | BindingFlags.Public);
			if (propertyInfo != null)
			{
				value = propertyInfo.GetValue(null, null);
				return true;
			}
			type2 = type2.BaseType;
		}
		while (type2 != null);
		value = null;
		return false;
	}
}
