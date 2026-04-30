namespace MS.Internal.Xaml.Parser;

internal class XamlPropertyName : XamlName
{
	public readonly XamlName Owner;

	public override string ScopedName
	{
		get
		{
			if (!IsDotted)
			{
				return base.Name;
			}
			return Owner.ScopedName + "." + base.Name;
		}
	}

	public string OwnerName
	{
		get
		{
			if (!IsDotted)
			{
				return string.Empty;
			}
			return Owner.Name;
		}
	}

	public bool IsDotted => Owner != null;

	private XamlPropertyName(XamlName owner, string prefix, string name)
		: base(name)
	{
		if (owner != null)
		{
			Owner = owner;
			_prefix = owner.Prefix ?? string.Empty;
		}
		else
		{
			_prefix = prefix ?? string.Empty;
		}
	}

	public static XamlPropertyName Parse(string longName)
	{
		if (string.IsNullOrEmpty(longName))
		{
			return null;
		}
		if (!XamlQualifiedName.Parse(longName, out var prefix, out var name))
		{
			return null;
		}
		int num = 0;
		string text = string.Empty;
		int num2 = name.IndexOf('.');
		if (num2 != -1)
		{
			text = name.Substring(num, num2);
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			num = num2 + 1;
		}
		string name2 = ((num == 0) ? name : name.Substring(num));
		XamlQualifiedName owner = null;
		if (!string.IsNullOrEmpty(text))
		{
			owner = new XamlQualifiedName(prefix, text);
		}
		return new XamlPropertyName(owner, prefix, name2);
	}

	public static XamlPropertyName Parse(string longName, string namespaceURI)
	{
		XamlPropertyName xamlPropertyName = Parse(longName);
		xamlPropertyName._namespace = namespaceURI;
		return xamlPropertyName;
	}
}
