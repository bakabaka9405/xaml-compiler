using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.UI.Xaml.Markup.Compiler.Lmr;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

internal class TypeResolver
{
	private HashSet<string> knownAssemblies = new HashSet<string>();

	private Dictionary<string, string> fullNameToAsmQName;

	private Dictionary<string, string> directUiToAsmQName;

	private XamlTypeUniverse typeUniverse;

	public bool IsInitialized => typeUniverse != null;

	public TypeResolver(XamlTypeUniverse typeUniverse)
	{
		this.typeUniverse = typeUniverse;
	}

	public void InitializeTypeNameMap()
	{
		fullNameToAsmQName = new Dictionary<string, string>();
		directUiToAsmQName = new Dictionary<string, string>();
		GetSortedReferenceAssemblies(out var clrAssemblies, out var winmdAssemblies);
		AddClrAssemblies(clrAssemblies);
		AddWinmdAssemblies(winmdAssemblies);
	}

	public void AddLocalAssemblyToTypeNameMap(Assembly localAssembly)
	{
		AddWinmdAssembly(localAssembly, isLocalAssembly: true);
	}

	public Type GetTypeByFullName(string fullName)
	{
		if (fullNameToAsmQName.TryGetValue(fullName, out var value))
		{
			return typeUniverse.GetTypeXFromName(value);
		}
		return null;
	}

	public Type GetDirectUIType(string name)
	{
		if (directUiToAsmQName.TryGetValue(name, out var value))
		{
			return typeUniverse.GetTypeXFromName(value);
		}
		return null;
	}

	private void AddClrAssemblies(IEnumerable<Assembly> clrAssemblies)
	{
		foreach (Assembly clrAssembly in clrAssemblies)
		{
			knownAssemblies.Add(clrAssembly.ToString());
			Type[] types = clrAssembly.GetTypes();
			foreach (Type type in types)
			{
				if (type.IsPublic)
				{
					string fullName = type.FullName;
					string assemblyQualifiedName = type.AssemblyQualifiedName;
					if (!fullNameToAsmQName.ContainsKey(fullName))
					{
						fullNameToAsmQName.Add(fullName, assemblyQualifiedName);
					}
					else
					{
						}
				}
			}
		}
	}

	private void AddWinmdAssemblies(IEnumerable<Assembly> winmdAssemblies)
	{
		foreach (Assembly winmdAssembly in winmdAssemblies)
		{
			AddWinmdAssembly(winmdAssembly, isLocalAssembly: false);
		}
	}

	private void AddWinmdAssembly(Assembly winmdAsm, bool isLocalAssembly)
	{
		string item = winmdAsm.ToString();
		bool flag = false;
		if (knownAssemblies.Contains(item))
		{
			flag = true;
		}
		else
		{
			knownAssemblies.Add(item);
		}
		Type[] types = winmdAsm.GetTypes();
		foreach (Type type in types)
		{
			if (!IsClrImplementationOfWinRTType(type) && (type.IsPublic || isLocalAssembly) && !(type.IsPublic && flag))
			{
				string fullName = type.FullName;
				string assemblyQualifiedName = type.AssemblyQualifiedName;
				bool flag2 = false;
				if (!fullNameToAsmQName.ContainsKey(fullName))
				{
					fullNameToAsmQName.Add(fullName, assemblyQualifiedName);
					flag2 = true;
				}
				else
				{
				}
				if (flag2 && IsDirectUIType(fullName, out var name))
				{
					directUiToAsmQName.Add(name, assemblyQualifiedName);
				}
			}
		}
	}

	private bool IsDirectUIType(string fullName, out string name)
	{
		name = null;
		if (fullName.StartsWith("Windows.UI"))
		{
			int num = fullName.LastIndexOf('.');
			string s = fullName.Substring(0, num);
			if (KS.ContainsString(DirectUISchemaContext.DirectUI2010Paths, s))
			{
				name = fullName.Substring(num + 1);
				return true;
			}
		}
		if (fullName.StartsWith("Windows.Foundation"))
		{
			string text = fullName.Substring("Windows.Foundation.".Length);
			if (KS.ContainsString(DirectUIXamlType.WindowsFoundationSystemTypes, text))
			{
				name = text;
				return true;
			}
		}
		return false;
	}

	private void GetSortedReferenceAssemblies(out List<Assembly> clrAssemblies, out List<Assembly> winmdAssemblies)
	{
		clrAssemblies = new List<Assembly>();
		winmdAssemblies = new List<Assembly>();
		foreach (Assembly assembly in typeUniverse.Assemblies)
		{
			AssemblyName name = assembly.GetName();
			if (name.ContentType == AssemblyContentType.WindowsRuntime)
			{
				if (!name.Name.Equals("platform", StringComparison.OrdinalIgnoreCase))
				{
					winmdAssemblies.Add(assembly);
				}
			}
			else
			{
				clrAssemblies.Add(assembly);
			}
		}
	}

	private Type ResolveTypeByReflectionFromAssemblyQualifiedName(string assemblyQualifiedTypeName)
	{
		if (string.IsNullOrEmpty(assemblyQualifiedTypeName) || assemblyQualifiedTypeName.IndexOf(',') < 0)
		{
			return null;
		}
		return GetTypeFromUniverse(assemblyQualifiedTypeName);
	}

	private Type GetTypeFromUniverse(string name)
	{
		try
		{
			return typeUniverse.GetTypeXFromName(name);
		}
		catch (TypeLoadException)
		{
			name.StartsWith("Platform.", StringComparison.OrdinalIgnoreCase);
		}
		catch (BadImageFormatException)
		{
		}
		catch (ArgumentException)
		{
		}
		return null;
	}

	private static bool IsClrImplementationOfWinRTType(Type type)
	{
		return type.Name.StartsWith("<CLR>", StringComparison.Ordinal);
	}
}
