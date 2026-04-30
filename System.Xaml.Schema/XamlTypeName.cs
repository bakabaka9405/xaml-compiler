using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MS.Internal.Xaml.Parser;

namespace System.Xaml.Schema;

[DebuggerDisplay("{{{Namespace}}}{Name}{TypeArgStringForDebugger}")]
public class XamlTypeName
{
	private List<XamlTypeName> _typeArguments;

	public string Name { get; set; }

	public string Namespace { get; set; }

	public IList<XamlTypeName> TypeArguments
	{
		get
		{
			if (_typeArguments == null)
			{
				_typeArguments = new List<XamlTypeName>();
			}
			return _typeArguments;
		}
	}

	internal bool HasTypeArgs
	{
		get
		{
			if (_typeArguments != null)
			{
				return _typeArguments.Count > 0;
			}
			return false;
		}
	}

	public XamlTypeName()
	{
	}

	public XamlTypeName(string xamlNamespace, string name)
		: this(xamlNamespace, name, null)
	{
	}

	public XamlTypeName(string xamlNamespace, string name, IEnumerable<XamlTypeName> typeArguments)
	{
		Name = name;
		Namespace = xamlNamespace;
		if (typeArguments != null)
		{
			List<XamlTypeName> typeArguments2 = new List<XamlTypeName>(typeArguments);
			_typeArguments = typeArguments2;
		}
	}

	public XamlTypeName(XamlType xamlType)
	{
		if (xamlType == null)
		{
			throw new ArgumentNullException("xamlType");
		}
		Name = xamlType.Name;
		Namespace = xamlType.GetXamlNamespaces()[0];
		if (xamlType.TypeArguments == null)
		{
			return;
		}
		foreach (XamlType typeArgument in xamlType.TypeArguments)
		{
			TypeArguments.Add(new XamlTypeName(typeArgument));
		}
	}

	public override string ToString()
	{
		return ToString(null);
	}

	public string ToString(INamespacePrefixLookup prefixLookup)
	{
		if (prefixLookup == null)
		{
			return ConvertToStringInternal(null);
		}
		return ConvertToStringInternal(prefixLookup.LookupPrefix);
	}

	public static string ToString(IList<XamlTypeName> typeNameList, INamespacePrefixLookup prefixLookup)
	{
		if (typeNameList == null)
		{
			throw new ArgumentNullException("typeNameList");
		}
		if (prefixLookup == null)
		{
			throw new ArgumentNullException("prefixLookup");
		}
		return ConvertListToStringInternal(typeNameList, prefixLookup.LookupPrefix);
	}

	public static XamlTypeName Parse(string typeName, IXamlNamespaceResolver namespaceResolver)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (namespaceResolver == null)
		{
			throw new ArgumentNullException("namespaceResolver");
		}
		string error;
		XamlTypeName xamlTypeName = ParseInternal(typeName, namespaceResolver.GetNamespace, out error);
		if (xamlTypeName == null)
		{
			throw new FormatException(error);
		}
		return xamlTypeName;
	}

	public static IList<XamlTypeName> ParseList(string typeNameList, IXamlNamespaceResolver namespaceResolver)
	{
		if (typeNameList == null)
		{
			throw new ArgumentNullException("typeNameList");
		}
		if (namespaceResolver == null)
		{
			throw new ArgumentNullException("namespaceResolver");
		}
		string error;
		IList<XamlTypeName> list = ParseListInternal(typeNameList, namespaceResolver.GetNamespace, out error);
		if (list == null)
		{
			throw new FormatException(error);
		}
		return list;
	}

	public static bool TryParse(string typeName, IXamlNamespaceResolver namespaceResolver, out XamlTypeName result)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (namespaceResolver == null)
		{
			throw new ArgumentNullException("namespaceResolver");
		}
		result = ParseInternal(typeName, namespaceResolver.GetNamespace, out var _);
		return result != null;
	}

	public static bool TryParseList(string typeNameList, IXamlNamespaceResolver namespaceResolver, out IList<XamlTypeName> result)
	{
		if (typeNameList == null)
		{
			throw new ArgumentNullException("typeNameList");
		}
		if (namespaceResolver == null)
		{
			throw new ArgumentNullException("namespaceResolver");
		}
		result = ParseListInternal(typeNameList, namespaceResolver.GetNamespace, out var _);
		return result != null;
	}

	internal static string ConvertListToStringInternal(IList<XamlTypeName> typeNameList, Func<string, string> prefixGenerator)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ConvertListToStringInternal(stringBuilder, typeNameList, prefixGenerator);
		return stringBuilder.ToString();
	}

	internal static void ConvertListToStringInternal(StringBuilder result, IList<XamlTypeName> typeNameList, Func<string, string> prefixGenerator)
	{
		bool flag = true;
		foreach (XamlTypeName typeName in typeNameList)
		{
			if (!flag)
			{
				result.Append(", ");
			}
			else
			{
				flag = false;
			}
			typeName.ConvertToStringInternal(result, prefixGenerator);
		}
	}

	internal static XamlTypeName ParseInternal(string typeName, Func<string, string> prefixResolver, out string error)
	{
		XamlTypeName xamlTypeName = GenericTypeNameParser.ParseIfTrivalName(typeName, prefixResolver, out error);
		if (xamlTypeName != null)
		{
			return xamlTypeName;
		}
		GenericTypeNameParser genericTypeNameParser = new GenericTypeNameParser(prefixResolver);
		return genericTypeNameParser.ParseName(typeName, out error);
	}

	internal static IList<XamlTypeName> ParseListInternal(string typeNameList, Func<string, string> prefixResolver, out string error)
	{
		GenericTypeNameParser genericTypeNameParser = new GenericTypeNameParser(prefixResolver);
		return genericTypeNameParser.ParseList(typeNameList, out error);
	}

	internal string ConvertToStringInternal(Func<string, string> prefixGenerator)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ConvertToStringInternal(stringBuilder, prefixGenerator);
		return stringBuilder.ToString();
	}

	internal void ConvertToStringInternal(StringBuilder result, Func<string, string> prefixGenerator)
	{
		if (Namespace == null)
		{
			throw new InvalidOperationException(SR.Get("XamlTypeNameNamespaceIsNull"));
		}
		if (string.IsNullOrEmpty(Name))
		{
			throw new InvalidOperationException(SR.Get("XamlTypeNameNameIsNullOrEmpty"));
		}
		if (prefixGenerator == null)
		{
			result.Append("{");
			result.Append(Namespace);
			result.Append("}");
		}
		else
		{
			string text = prefixGenerator(Namespace);
			if (text == null)
			{
				throw new InvalidOperationException(SR.Get("XamlTypeNameCannotGetPrefix", Namespace));
			}
			if (text != string.Empty)
			{
				result.Append(text);
				result.Append(":");
			}
		}
		if (HasTypeArgs)
		{
			string subscript;
			string value = GenericTypeNameScanner.StripSubscript(Name, out subscript);
			result.Append(value);
			result.Append("(");
			ConvertListToStringInternal(result, TypeArguments, prefixGenerator);
			result.Append(")");
			result.Append(subscript);
		}
		else
		{
			result.Append(Name);
		}
	}
}
