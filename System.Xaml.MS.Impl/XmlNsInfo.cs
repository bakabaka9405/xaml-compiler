using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Markup;
using System.Xaml.Schema;

namespace System.Xaml.MS.Impl;

internal class XmlNsInfo
{
	private class NamespaceComparer
	{
		private XmlNsInfo _nsInfo;

		private IDictionary<string, int> _subsumeCount;

		public NamespaceComparer(XmlNsInfo nsInfo, Assembly assembly)
		{
			_nsInfo = nsInfo;
			_subsumeCount = new Dictionary<string, int>(nsInfo.OldToNewNs.Count);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (string value in nsInfo.OldToNewNs.Values)
			{
				dictionary.Clear();
				string text = value;
				do
				{
					if (dictionary.ContainsKey(text))
					{
						throw new XamlSchemaException(SR.Get("XmlnsCompatCycle", assembly.FullName, text));
					}
					dictionary.Add(text, null);
					IncrementSubsumeCount(text);
					text = GetNewNs(text);
				}
				while (text != null);
			}
		}

		public int CompareNamespacesByPreference(string ns1, string ns2)
		{
			if (KS.Eq(ns1, ns2))
			{
				return 0;
			}
			for (string newNs = GetNewNs(ns1); newNs != null; newNs = GetNewNs(newNs))
			{
				if (newNs == ns2)
				{
					return 1;
				}
			}
			for (string newNs = GetNewNs(ns2); newNs != null; newNs = GetNewNs(newNs))
			{
				if (newNs == ns1)
				{
					return -1;
				}
			}
			if (GetNewNs(ns1) == null)
			{
				if (GetNewNs(ns2) != null)
				{
					return -1;
				}
			}
			else if (GetNewNs(ns2) == null)
			{
				return 1;
			}
			int value = 0;
			int value2 = 0;
			_subsumeCount.TryGetValue(ns1, out value);
			_subsumeCount.TryGetValue(ns2, out value2);
			if (value > value2)
			{
				return -1;
			}
			if (value2 > value)
			{
				return 1;
			}
			_nsInfo.Prefixes.TryGetValue(ns1, out var value3);
			_nsInfo.Prefixes.TryGetValue(ns2, out var value4);
			if (string.IsNullOrEmpty(value3))
			{
				if (!string.IsNullOrEmpty(value4))
				{
					return 1;
				}
			}
			else
			{
				if (string.IsNullOrEmpty(value4))
				{
					return -1;
				}
				if (value3.Length < value4.Length)
				{
					return -1;
				}
				if (value4.Length < value3.Length)
				{
					return 1;
				}
			}
			return StringComparer.Ordinal.Compare(ns1, ns2);
		}

		private string GetNewNs(string oldNs)
		{
			_nsInfo.OldToNewNs.TryGetValue(oldNs, out var value);
			return value;
		}

		private void IncrementSubsumeCount(string ns)
		{
			_subsumeCount.TryGetValue(ns, out var value);
			value++;
			_subsumeCount[ns] = value;
		}
	}

	internal class XmlNsDefinition
	{
		public string ClrNamespace { get; set; }

		public string XmlNamespace { get; set; }
	}

	private IList<XmlNsDefinition> _nsDefs;

	private ConcurrentDictionary<string, IList<string>> _clrToXmlNs;

	private ICollection<AssemblyName> _internalsVisibleTo;

	private Dictionary<string, string> _oldToNewNs;

	private Dictionary<string, string> _prefixes;

	private string _rootNamespace;

	private WeakReference _assembly;

	private IList<CustomAttributeData> _attributeData;

	private bool _fullyQualifyAssemblyName;

	internal Assembly Assembly => (Assembly)_assembly.Target;

	internal IList<XmlNsDefinition> NsDefs
	{
		get
		{
			if (_nsDefs == null)
			{
				_nsDefs = LoadNsDefs();
			}
			return _nsDefs;
		}
	}

	internal ConcurrentDictionary<string, IList<string>> ClrToXmlNs
	{
		get
		{
			if (_clrToXmlNs == null)
			{
				_clrToXmlNs = LoadClrToXmlNs();
			}
			return _clrToXmlNs;
		}
	}

	internal ICollection<AssemblyName> InternalsVisibleTo
	{
		get
		{
			if (_internalsVisibleTo == null)
			{
				_internalsVisibleTo = LoadInternalsVisibleTo();
			}
			return _internalsVisibleTo;
		}
	}

	internal Dictionary<string, string> OldToNewNs
	{
		get
		{
			if (_oldToNewNs == null)
			{
				_oldToNewNs = LoadOldToNewNs();
			}
			return _oldToNewNs;
		}
	}

	internal Dictionary<string, string> Prefixes
	{
		get
		{
			if (_prefixes == null)
			{
				_prefixes = LoadPrefixes();
			}
			return _prefixes;
		}
	}

	internal string RootNamespace
	{
		get
		{
			if (_rootNamespace == null)
			{
				_rootNamespace = LoadRootNamespace() ?? string.Empty;
			}
			return _rootNamespace;
		}
	}

	internal XmlNsInfo(Assembly assembly, bool fullyQualifyAssemblyName)
	{
		_assembly = new WeakReference(assembly);
		_fullyQualifyAssemblyName = fullyQualifyAssemblyName;
	}

	private void EnsureReflectionOnlyAttributeData()
	{
		if (_attributeData == null)
		{
			_attributeData = Assembly.GetCustomAttributesData();
		}
	}

	internal static string GetPreferredPrefix(string prefix1, string prefix2)
	{
		if (prefix1.Length < prefix2.Length)
		{
			return prefix1;
		}
		if (prefix2.Length < prefix1.Length)
		{
			return prefix2;
		}
		if (StringComparer.Ordinal.Compare(prefix1, prefix2) < 0)
		{
			return prefix1;
		}
		return prefix2;
	}

	private IList<XmlNsDefinition> LoadNsDefs()
	{
		IList<XmlNsDefinition> result = new List<XmlNsDefinition>();
		Assembly assembly = Assembly;
		if (assembly == null)
		{
			return result;
		}
		if (assembly.ReflectionOnly)
		{
			EnsureReflectionOnlyAttributeData();
			foreach (CustomAttributeData attributeDatum in _attributeData)
			{
				if (LooseTypeExtensions.AssemblyQualifiedNameEquals(attributeDatum.Constructor.DeclaringType, typeof(XmlnsDefinitionAttribute)))
				{
					string xmlns = attributeDatum.ConstructorArguments[0].Value as string;
					string clrns = attributeDatum.ConstructorArguments[1].Value as string;
					LoadNsDefHelper(result, xmlns, clrns, assembly);
				}
			}
		}
		else
		{
			Attribute[] customAttributes = Attribute.GetCustomAttributes(assembly, typeof(XmlnsDefinitionAttribute));
			Attribute[] array = customAttributes;
			foreach (Attribute attribute in array)
			{
				XmlnsDefinitionAttribute xmlnsDefinitionAttribute = (XmlnsDefinitionAttribute)attribute;
				string xmlNamespace = xmlnsDefinitionAttribute.XmlNamespace;
				string clrNamespace = xmlnsDefinitionAttribute.ClrNamespace;
				LoadNsDefHelper(result, xmlNamespace, clrNamespace, assembly);
			}
		}
		return result;
	}

	private void LoadNsDefHelper(IList<XmlNsDefinition> result, string xmlns, string clrns, Assembly assembly)
	{
		if (string.IsNullOrEmpty(xmlns) || clrns == null)
		{
			throw new XamlSchemaException(SR.Get("BadXmlnsDefinition", assembly.FullName));
		}
		result.Add(new XmlNsDefinition
		{
			ClrNamespace = clrns,
			XmlNamespace = xmlns
		});
	}

	private ConcurrentDictionary<string, IList<string>> LoadClrToXmlNs()
	{
		ConcurrentDictionary<string, IList<string>> concurrentDictionary = XamlSchemaContext.CreateDictionary<string, IList<string>>();
		Assembly assembly = Assembly;
		if (assembly == null)
		{
			return concurrentDictionary;
		}
		foreach (XmlNsDefinition nsDef in NsDefs)
		{
			if (!concurrentDictionary.TryGetValue(nsDef.ClrNamespace, out var value))
			{
				value = new List<string>();
				concurrentDictionary.TryAdd(nsDef.ClrNamespace, value);
			}
			value.Add(nsDef.XmlNamespace);
		}
		string assemblyName = (_fullyQualifyAssemblyName ? assembly.FullName : XamlSchemaContext.GetAssemblyShortName(assembly));
		foreach (KeyValuePair<string, IList<string>> item in concurrentDictionary)
		{
			List<string> list = (List<string>)item.Value;
			NamespaceComparer namespaceComparer = new NamespaceComparer(this, assembly);
			list.Sort(namespaceComparer.CompareNamespacesByPreference);
			string uri = ClrNamespaceUriParser.GetUri(item.Key, assemblyName);
			list.Add(uri);
		}
		MakeListsImmutable(concurrentDictionary);
		return concurrentDictionary;
	}

	private ICollection<AssemblyName> LoadInternalsVisibleTo()
	{
		List<AssemblyName> result = new List<AssemblyName>();
		Assembly assembly = Assembly;
		if (assembly == null)
		{
			return result;
		}
		if (assembly.ReflectionOnly)
		{
			EnsureReflectionOnlyAttributeData();
			foreach (CustomAttributeData attributeDatum in _attributeData)
			{
				if (LooseTypeExtensions.AssemblyQualifiedNameEquals(attributeDatum.Constructor.DeclaringType, typeof(InternalsVisibleToAttribute)))
				{
					string assemblyName = attributeDatum.ConstructorArguments[0].Value as string;
					LoadInternalsVisibleToHelper(result, assemblyName, assembly);
				}
			}
		}
		else
		{
			Attribute[] customAttributes = Attribute.GetCustomAttributes(assembly, typeof(InternalsVisibleToAttribute));
			for (int i = 0; i < customAttributes.Length; i++)
			{
				InternalsVisibleToAttribute internalsVisibleToAttribute = (InternalsVisibleToAttribute)customAttributes[i];
				LoadInternalsVisibleToHelper(result, internalsVisibleToAttribute.AssemblyName, assembly);
			}
		}
		return result;
	}

	private void LoadInternalsVisibleToHelper(List<AssemblyName> result, string assemblyName, Assembly assembly)
	{
		if (assemblyName == null)
		{
			throw new XamlSchemaException(SR.Get("BadInternalsVisibleTo1", assembly.FullName));
		}
		try
		{
			result.Add(new AssemblyName(assemblyName));
		}
		catch (ArgumentException innerException)
		{
			throw new XamlSchemaException(SR.Get("BadInternalsVisibleTo2", assemblyName, assembly.FullName), innerException);
		}
		catch (FileLoadException innerException2)
		{
			throw new XamlSchemaException(SR.Get("BadInternalsVisibleTo2", assemblyName, assembly.FullName), innerException2);
		}
	}

	private Dictionary<string, string> LoadOldToNewNs()
	{
		Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
		Assembly assembly = Assembly;
		if (assembly == null)
		{
			return result;
		}
		if (assembly.ReflectionOnly)
		{
			EnsureReflectionOnlyAttributeData();
			foreach (CustomAttributeData attributeDatum in _attributeData)
			{
				if (LooseTypeExtensions.AssemblyQualifiedNameEquals(attributeDatum.Constructor.DeclaringType, typeof(XmlnsCompatibleWithAttribute)))
				{
					string oldns = attributeDatum.ConstructorArguments[0].Value as string;
					string newns = attributeDatum.ConstructorArguments[1].Value as string;
					LoadOldToNewNsHelper(result, oldns, newns, assembly);
				}
			}
		}
		else
		{
			Attribute[] customAttributes = Attribute.GetCustomAttributes(assembly, typeof(XmlnsCompatibleWithAttribute));
			Attribute[] array = customAttributes;
			foreach (Attribute attribute in array)
			{
				XmlnsCompatibleWithAttribute xmlnsCompatibleWithAttribute = (XmlnsCompatibleWithAttribute)attribute;
				LoadOldToNewNsHelper(result, xmlnsCompatibleWithAttribute.OldNamespace, xmlnsCompatibleWithAttribute.NewNamespace, assembly);
			}
		}
		return result;
	}

	private void LoadOldToNewNsHelper(Dictionary<string, string> result, string oldns, string newns, Assembly assembly)
	{
		if (string.IsNullOrEmpty(newns) || string.IsNullOrEmpty(oldns))
		{
			throw new XamlSchemaException(SR.Get("BadXmlnsCompat", assembly.FullName));
		}
		if (result.ContainsKey(oldns))
		{
			throw new XamlSchemaException(SR.Get("DuplicateXmlnsCompat", assembly.FullName, oldns));
		}
		result.Add(oldns, newns);
	}

	private Dictionary<string, string> LoadPrefixes()
	{
		Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
		Assembly assembly = Assembly;
		if (assembly == null)
		{
			return result;
		}
		if (assembly.ReflectionOnly)
		{
			EnsureReflectionOnlyAttributeData();
			foreach (CustomAttributeData attributeDatum in _attributeData)
			{
				if (LooseTypeExtensions.AssemblyQualifiedNameEquals(attributeDatum.Constructor.DeclaringType, typeof(XmlnsPrefixAttribute)))
				{
					string xmlns = attributeDatum.ConstructorArguments[0].Value as string;
					string prefix = attributeDatum.ConstructorArguments[1].Value as string;
					LoadPrefixesHelper(result, xmlns, prefix, assembly);
				}
			}
		}
		else
		{
			Attribute[] customAttributes = Attribute.GetCustomAttributes(assembly, typeof(XmlnsPrefixAttribute));
			Attribute[] array = customAttributes;
			foreach (Attribute attribute in array)
			{
				XmlnsPrefixAttribute xmlnsPrefixAttribute = (XmlnsPrefixAttribute)attribute;
				LoadPrefixesHelper(result, xmlnsPrefixAttribute.XmlNamespace, xmlnsPrefixAttribute.Prefix, assembly);
			}
		}
		return result;
	}

	private void LoadPrefixesHelper(Dictionary<string, string> result, string xmlns, string prefix, Assembly assembly)
	{
		if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(xmlns))
		{
			throw new XamlSchemaException(SR.Get("BadXmlnsPrefix", assembly.FullName));
		}
		if (!result.TryGetValue(xmlns, out var value) || GetPreferredPrefix(value, prefix) == prefix)
		{
			result[xmlns] = prefix;
		}
	}

	private string LoadRootNamespace()
	{
		Assembly assembly = Assembly;
		if (assembly == null)
		{
			return null;
		}
		if (assembly.ReflectionOnly)
		{
			EnsureReflectionOnlyAttributeData();
			foreach (CustomAttributeData attributeDatum in _attributeData)
			{
				if (LooseTypeExtensions.AssemblyQualifiedNameEquals(attributeDatum.Constructor.DeclaringType, typeof(RootNamespaceAttribute)))
				{
					return attributeDatum.ConstructorArguments[0].Value as string;
				}
			}
			return null;
		}
		return ((RootNamespaceAttribute)Attribute.GetCustomAttribute(assembly, typeof(RootNamespaceAttribute)))?.Namespace;
	}

	private void MakeListsImmutable(IDictionary<string, IList<string>> dict)
	{
		string[] array = new string[dict.Count];
		dict.Keys.CopyTo(array, 0);
		string[] array2 = array;
		foreach (string key in array2)
		{
			dict[key] = new ReadOnlyCollection<string>(dict[key]);
		}
	}
}
