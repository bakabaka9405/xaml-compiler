using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Xaml.Schema;

namespace System.Xaml;

public class XamlDirective : XamlMember
{
	private AllowedMemberLocations _allowedLocation;

	private IList<string> _xamlNamespaces;

	public AllowedMemberLocations AllowedLocation => _allowedLocation;

	internal XamlDirective(IEnumerable<string> xamlNamespaces, string name, AllowedMemberLocations allowedLocation, MemberReflector reflector)
		: base(name, reflector)
	{
		_xamlNamespaces = GetReadOnly(xamlNamespaces);
		_allowedLocation = allowedLocation;
	}

	public XamlDirective(IEnumerable<string> xamlNamespaces, string name, XamlType xamlType, XamlValueConverter<TypeConverter> typeConverter, AllowedMemberLocations allowedLocation)
		: base(name, new MemberReflector(xamlType, typeConverter))
	{
		if (xamlType == null)
		{
			throw new ArgumentNullException("xamlType");
		}
		_xamlNamespaces = GetReadOnly(xamlNamespaces);
		_allowedLocation = allowedLocation;
	}

	public XamlDirective(string xamlNamespace, string name)
		: base(name, null)
	{
		_xamlNamespaces = GetReadOnly(xamlNamespace);
		_allowedLocation = AllowedMemberLocations.Any;
	}

	public override int GetHashCode()
	{
		int num = ((base.Name != null) ? base.Name.GetHashCode() : 0);
		foreach (string xamlNamespace in _xamlNamespaces)
		{
			num ^= xamlNamespace.GetHashCode();
		}
		return num;
	}

	public override string ToString()
	{
		if (_xamlNamespaces.Count > 0)
		{
			return "{" + _xamlNamespaces[0] + "}" + base.Name;
		}
		return base.Name;
	}

	public override IList<string> GetXamlNamespaces()
	{
		return _xamlNamespaces;
	}

	internal static bool NamespacesAreEqual(XamlDirective directive1, XamlDirective directive2)
	{
		IList<string> xamlNamespaces = directive1._xamlNamespaces;
		IList<string> xamlNamespaces2 = directive2._xamlNamespaces;
		if (xamlNamespaces.Count != xamlNamespaces2.Count)
		{
			return false;
		}
		for (int i = 0; i < xamlNamespaces.Count; i++)
		{
			if (xamlNamespaces[i] != xamlNamespaces2[i])
			{
				return false;
			}
		}
		return true;
	}

	protected sealed override XamlMemberInvoker LookupInvoker()
	{
		return XamlMemberInvoker.DirectiveInvoker;
	}

	protected sealed override ICustomAttributeProvider LookupCustomAttributeProvider()
	{
		return null;
	}

	protected sealed override IList<XamlMember> LookupDependsOn()
	{
		return null;
	}

	protected sealed override XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
	{
		return null;
	}

	protected sealed override bool LookupIsAmbient()
	{
		return false;
	}

	protected sealed override bool LookupIsEvent()
	{
		return false;
	}

	protected sealed override bool LookupIsReadOnly()
	{
		return false;
	}

	protected sealed override bool LookupIsReadPublic()
	{
		return true;
	}

	protected sealed override bool LookupIsUnknown()
	{
		return base.IsUnknown;
	}

	protected sealed override bool LookupIsWriteOnly()
	{
		return false;
	}

	protected sealed override bool LookupIsWritePublic()
	{
		return true;
	}

	protected sealed override XamlType LookupTargetType()
	{
		return null;
	}

	protected sealed override XamlValueConverter<TypeConverter> LookupTypeConverter()
	{
		return base.TypeConverter;
	}

	protected sealed override XamlType LookupType()
	{
		return base.Type;
	}

	protected sealed override MethodInfo LookupUnderlyingGetter()
	{
		return null;
	}

	protected sealed override MemberInfo LookupUnderlyingMember()
	{
		return null;
	}

	protected sealed override MethodInfo LookupUnderlyingSetter()
	{
		return null;
	}

	private static ReadOnlyCollection<string> GetReadOnly(string xamlNamespace)
	{
		if (xamlNamespace == null)
		{
			throw new ArgumentNullException("xamlNamespace");
		}
		return new ReadOnlyCollection<string>(new string[1] { xamlNamespace });
	}

	private static ReadOnlyCollection<string> GetReadOnly(IEnumerable<string> xamlNamespaces)
	{
		if (xamlNamespaces == null)
		{
			throw new ArgumentNullException("xamlNamespaces");
		}
		List<string> list = new List<string>(xamlNamespaces);
		foreach (string item in list)
		{
			if (item == null)
			{
				throw new ArgumentException(SR.Get("CollectionCannotContainNulls", "xamlNamespaces"));
			}
		}
		return list.AsReadOnly();
	}
}
