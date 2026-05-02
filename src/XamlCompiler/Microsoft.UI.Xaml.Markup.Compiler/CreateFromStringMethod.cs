using System.Reflection;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class CreateFromStringMethod
{
	private string fullName;

	private string localName;

	private XamlType declaringType;

	public MethodInfo MethodInfo { get; private set; }

	public XamlType DeclaringType => declaringType;

	public bool Resolved { get; private set; }

	public bool Exists
	{
		get
		{
			if (string.IsNullOrEmpty(fullName) && string.IsNullOrEmpty(localName))
			{
				return false;
			}
			return true;
		}
	}

	public string UnresolvedName
	{
		get
		{
			if (!string.IsNullOrEmpty(fullName))
			{
				return fullName;
			}
			if (!string.IsNullOrEmpty(localName))
			{
				string[] array = localName.Split('.');
				if (array.Length == 2)
				{
					string text = array[0];
					string text2 = array[1];
					return declaringType.UnderlyingType.FullName + "+" + text + "." + text2;
				}
				return declaringType.UnderlyingType.FullName + "." + localName;
			}
			return string.Empty;
		}
	}

	public LanguageSpecificString ResolvedName => new LanguageSpecificString(() => DeclaringType.CppCXName(IncludeHatIfApplicable: false) + "::" + MethodInfo.Name, () => DeclaringType.CppWinRTName() + "::" + MethodInfo.Name, () => DeclaringType.CSharpName() + "." + MethodInfo.Name, () => DeclaringType.VBName() + "." + MethodInfo.Name);

	public CreateFromStringMethod()
	{
		fullName = string.Empty;
		localName = string.Empty;
	}

	public CreateFromStringMethod(string fullName)
	{
		this.fullName = fullName;
		localName = string.Empty;
	}

	public CreateFromStringMethod(XamlType declaringType, string localName)
	{
		fullName = string.Empty;
		this.localName = localName;
		this.declaringType = declaringType;
	}

	public void SetResolved(XamlType declaringType, string methodName, MethodInfo methodInfo)
	{
		this.declaringType = declaringType;
		MethodInfo = methodInfo;
	}
}
