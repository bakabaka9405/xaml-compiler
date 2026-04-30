using System.Reflection;
using System.Security;

namespace System.Xaml.Permissions;

[Serializable]
public class XamlAccessLevel
{
	private static class XmlConstants
	{
		public const string XamlAccessLevel = "XamlAccessLevel";

		public const string AssemblyName = "AssemblyName";

		public const string TypeName = "TypeName";
	}

	public AssemblyName AssemblyAccessToAssemblyName => new AssemblyName(AssemblyNameString);

	public string PrivateAccessToTypeName { get; private set; }

	internal string AssemblyNameString { get; private set; }

	private XamlAccessLevel(string assemblyName, string typeName)
	{
		AssemblyNameString = assemblyName;
		PrivateAccessToTypeName = typeName;
	}

	public static XamlAccessLevel AssemblyAccessTo(Assembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		return new XamlAccessLevel(assembly.FullName, null);
	}

	public static XamlAccessLevel AssemblyAccessTo(AssemblyName assemblyName)
	{
		if (assemblyName == null)
		{
			throw new ArgumentNullException("assemblyName");
		}
		ValidateAssemblyName(assemblyName, "assemblyName");
		return new XamlAccessLevel(assemblyName.FullName, null);
	}

	public static XamlAccessLevel PrivateAccessTo(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return new XamlAccessLevel(type.Assembly.FullName, type.FullName);
	}

	public static XamlAccessLevel PrivateAccessTo(string assemblyQualifiedTypeName)
	{
		if (assemblyQualifiedTypeName == null)
		{
			throw new ArgumentNullException("assemblyQualifiedTypeName");
		}
		int num = assemblyQualifiedTypeName.IndexOf(',');
		if (num < 0)
		{
			throw new ArgumentException(SR.Get("ExpectedQualifiedTypeName", assemblyQualifiedTypeName), "assemblyQualifiedTypeName");
		}
		string typeName = assemblyQualifiedTypeName.Substring(0, num).Trim();
		string assemblyName = assemblyQualifiedTypeName.Substring(num + 1).Trim();
		AssemblyName assemblyName2 = new AssemblyName(assemblyName);
		ValidateAssemblyName(assemblyName2, "assemblyQualifiedTypeName");
		return new XamlAccessLevel(assemblyName2.FullName, typeName);
	}

	internal XamlAccessLevel AssemblyOnly()
	{
		return new XamlAccessLevel(AssemblyNameString, null);
	}

	internal static XamlAccessLevel FromXml(SecurityElement elem)
	{
		if (elem.Tag != "XamlAccessLevel")
		{
			throw new ArgumentException(SR.Get("SecurityXmlUnexpectedTag", elem.Tag, "XamlAccessLevel"), "elem");
		}
		string text = elem.Attribute("AssemblyName");
		if (text == null)
		{
			throw new ArgumentException(SR.Get("SecurityXmlMissingAttribute", "AssemblyName"), "elem");
		}
		AssemblyName assemblyName = new AssemblyName(text);
		ValidateAssemblyName(assemblyName, "elem");
		string text2 = elem.Attribute("TypeName");
		if (text2 != null)
		{
			text2 = text2.Trim();
		}
		return new XamlAccessLevel(assemblyName.FullName, text2);
	}

	internal bool Includes(XamlAccessLevel other)
	{
		if (other.AssemblyNameString == AssemblyNameString)
		{
			if (other.PrivateAccessToTypeName != null)
			{
				return other.PrivateAccessToTypeName == PrivateAccessToTypeName;
			}
			return true;
		}
		return false;
	}

	internal SecurityElement ToXml()
	{
		SecurityElement securityElement = new SecurityElement("XamlAccessLevel");
		securityElement.AddAttribute("AssemblyName", AssemblyNameString);
		if (PrivateAccessToTypeName != null)
		{
			securityElement.AddAttribute("TypeName", PrivateAccessToTypeName);
		}
		return securityElement;
	}

	private static void ValidateAssemblyName(AssemblyName assemblyName, string argName)
	{
		if (assemblyName.Name == null || assemblyName.Version == null || assemblyName.CultureInfo == null || assemblyName.GetPublicKeyToken() == null)
		{
			throw new ArgumentException(SR.Get("ExpectedQualifiedAssemblyName", assemblyName.FullName), argName);
		}
	}
}
