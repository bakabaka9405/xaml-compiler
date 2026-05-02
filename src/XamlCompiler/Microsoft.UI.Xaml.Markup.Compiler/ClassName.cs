using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class ClassName
{
	public string Namespace { get; }

	public string ShortName { get; }

	public string FullName
	{
		get
		{
			if (!string.IsNullOrEmpty(ShortName))
			{
				return $"{Namespace}.{ShortName}";
			}
			return string.Empty;
		}
	}

	public ClassName(string fullName)
	{
		if (!string.IsNullOrEmpty(fullName))
		{
			int num = fullName.LastIndexOf('.');
			if (num == -1)
			{
				throw new ArgumentException("Class full name is invalid: " + fullName);
			}
			Namespace = fullName.Substring(0, num);
			ShortName = fullName.Substring(num + 1);
		}
	}
}
