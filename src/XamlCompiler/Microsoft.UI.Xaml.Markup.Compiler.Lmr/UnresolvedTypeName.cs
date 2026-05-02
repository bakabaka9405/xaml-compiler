using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;
using Microsoft.UI.Xaml.Markup.Compiler.Core;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[DebuggerDisplay("{m_TypeName},{m_AssemblyName}")]
internal class UnresolvedTypeName
{
	private readonly string m_TypeName;

	private readonly AssemblyName m_AssemblyName;

	private static InstanceCache<AssemblyName, string> _assemblyNameCache = new InstanceCache<AssemblyName, string>();

	public string TypeName => m_TypeName;

	public UnresolvedTypeName(string typeName, AssemblyName assemblyName)
	{
		m_TypeName = typeName;
		m_AssemblyName = assemblyName;
	}

	public Type ConvertToType(ITypeUniverse universe, Module moduleContext)
	{
		string value = null;
		if (!_assemblyNameCache.TryGetValue(m_AssemblyName, out value))
		{
			value = m_AssemblyName.FullName;
			_assemblyNameCache[m_AssemblyName] = value;
		}
		string input = string.Format(CultureInfo.InvariantCulture, "{0},{1}", m_TypeName, value);
		return TypeNameParser.ParseTypeName(universe, moduleContext, input, throwOnError: false);
	}
}
