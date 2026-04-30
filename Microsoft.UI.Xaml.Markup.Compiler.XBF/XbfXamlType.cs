using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

internal class XbfXamlType : IXbfType
{
	private DirectUIXamlType _xamlType;

	private IXmpXamlType _xmp;

	private Dictionary<string, XbfXamlMember> _memberMap = new Dictionary<string, XbfXamlMember>();

	private bool? isBindable;

	private string _fullName;

	public IXbfType BaseType => _xmp.GetXmpXamlType((DirectUIXamlType)_xamlType.BaseType);

	public IXbfMember ContentProperty
	{
		get
		{
			XamlMember contentProperty = _xamlType.ContentProperty;
			if (contentProperty == null)
			{
				return null;
			}
			return GetMember(contentProperty.Name);
		}
	}

	public string FullName
	{
		get
		{
			if (_fullName == null)
			{
				_fullName = XamlSchemaCodeInfo.GetFullGenericNestedName(UnderlyingType, XamlSchemaCodeInfo.SetAirityOnGenericTypeNames);
			}
			return _fullName;
		}
	}

	public bool IsArray => _xamlType.IsArray;

	public bool IsBindable
	{
		get
		{
			if (!isBindable.HasValue)
			{
				isBindable = HasBindableAttribute();
			}
			return isBindable.Value;
		}
	}

	public bool IsCollection => _xamlType.IsCollection;

	public bool IsConstructible => _xamlType.IsConstructible;

	public bool IsDictionary => _xamlType.IsDictionary;

	public bool IsMarkupExtension => _xamlType.IsMarkupExtension;

	public IXbfType ItemType => _xmp.GetXmpXamlType((DirectUIXamlType)_xamlType.ItemType);

	public IXbfType KeyType => _xmp.GetXmpXamlType((DirectUIXamlType)_xamlType.KeyType);

	public IXbfType BoxedType => null;

	public Type UnderlyingType => _xamlType.UnderlyingType;

	public XbfXamlType(DirectUIXamlType xamlType, IXmpXamlType xmp)
	{
		_xamlType = xamlType;
		_xmp = xmp;
	}

	public IXbfMember GetMember(string name)
	{
		if (!_memberMap.TryGetValue(name, out var value))
		{
			XamlMember xamlMember = _xamlType.GetMember(name);
			if (xamlMember == null)
			{
				xamlMember = _xamlType.GetAttachableMember(name);
			}
			DirectUIXamlType directUIXamlType = (DirectUIXamlType)xamlMember.DeclaringType;
			if (directUIXamlType != _xamlType)
			{
				xamlMember = null;
			}
			if (xamlMember != null)
			{
				value = new XbfXamlMember((DirectUIXamlMember)xamlMember, _xmp);
				_memberMap.Add(name, value);
			}
		}
		return value;
	}

	public object ActivateInstance()
	{
		throw new NotImplementedException();
	}

	public void AddToMap(object instance, object key, object value)
	{
		throw new NotImplementedException();
	}

	public void AddToVector(object instance, object value)
	{
		throw new NotImplementedException();
	}

	public object CreateFromString(string value)
	{
		throw new NotImplementedException();
	}

	public void RunInitializer()
	{
		throw new NotImplementedException();
	}

	private bool HasBindableAttribute()
	{
		Type underlyingType = _xamlType.UnderlyingType;
		using (IEnumerator<CustomAttributeData> enumerator = Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ReflectionHelper.GetCustomAttributeData(underlyingType, inherit: false, "Microsoft.UI.Xaml.Data.BindableAttribute").GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				CustomAttributeData current = enumerator.Current;
				return true;
			}
		}
		return false;
	}
}
