namespace MS.Internal.Xaml.Parser;

internal class XamlQualifiedName : XamlName
{
	public override string ScopedName
	{
		get
		{
			if (!string.IsNullOrEmpty(base.Prefix))
			{
				return base.Prefix + ":" + base.Name;
			}
			return base.Name;
		}
	}

	public XamlQualifiedName(string prefix, string name)
		: base(prefix, name)
	{
	}

	internal static bool IsNameValid(string name)
	{
		if (name.Length == 0)
		{
			return false;
		}
		if (!XamlName.IsValidNameStartChar(name[0]))
		{
			return false;
		}
		for (int i = 1; i < name.Length; i++)
		{
			if (!XamlName.IsValidQualifiedNameChar(name[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsNameValid_WithPlus(string name)
	{
		if (name.Length == 0)
		{
			return false;
		}
		if (!XamlName.IsValidNameStartChar(name[0]))
		{
			return false;
		}
		for (int i = 1; i < name.Length; i++)
		{
			if (!XamlName.IsValidQualifiedNameCharPlus(name[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool Parse(string longName, out string prefix, out string name)
	{
		int num = 0;
		int num2 = longName.IndexOf(':');
		prefix = string.Empty;
		name = string.Empty;
		if (num2 != -1)
		{
			prefix = longName.Substring(num, num2);
			if (string.IsNullOrEmpty(prefix) || !IsNameValid(prefix))
			{
				return false;
			}
			num = num2 + 1;
		}
		name = ((num == 0) ? longName : longName.Substring(num));
		if (string.IsNullOrEmpty(name) || !IsNameValid_WithPlus(name))
		{
			return false;
		}
		return true;
	}
}
