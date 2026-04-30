using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler.RootLog;

internal class RootLogBuilder
{
	private Dictionary<string, RootTypeBuilder> _typeDict = new Dictionary<string, RootTypeBuilder>();

	private Dictionary<string, RootPropertyPathName> _propertyPathNameDict = new Dictionary<string, RootPropertyPathName>();

	private Dictionary<string, RootInterface> _interfaceDict = new Dictionary<string, RootInterface>();

	private bool _haveEntered_IValueConverter;

	private bool _haveEntered_IXamlMetadataProvider;

	public Roots GetRoots()
	{
		Roots roots = new Roots();
		foreach (RootTypeBuilder value in _typeDict.Values)
		{
			roots.RootTypes.Add(value.GetRootType());
		}
		roots.Interfaces.AddRange(_interfaceDict.Values);
		roots.PropertyPathNames.AddRange(_propertyPathNameDict.Values);
		return roots;
	}

	public RootInterface AddInterface(string name)
	{
		if (!_interfaceDict.TryGetValue(name, out var value))
		{
			value = new RootInterface
			{
				FullName = name
			};
			_interfaceDict.Add(name, value);
		}
		return value;
	}

	public RootPropertyPathName AddPropertyPathName(string name)
	{
		if (!_propertyPathNameDict.TryGetValue(name, out var value))
		{
			value = new RootPropertyPathName
			{
				Name = name
			};
			_propertyPathNameDict.Add(name, value);
		}
		return value;
	}

	public RootTypeBuilder AddTypeBuilder(DirectUIXamlType duiType)
	{
		CheckTypeForInterfaces(duiType);
		string fullName = duiType.UnderlyingType.FullName;
		if (!_typeDict.TryGetValue(fullName, out var value))
		{
			value = new RootTypeBuilder(fullName);
			_typeDict.Add(fullName, value);
		}
		return value;
	}

	public RootMember AddProperty(DirectUIXamlType duiType, string name)
	{
		RootTypeBuilder rootTypeBuilder = AddTypeBuilder(duiType);
		return rootTypeBuilder.AddProperty(name);
	}

	public RootMember AddEvent(DirectUIXamlType duiType, string name)
	{
		RootTypeBuilder rootTypeBuilder = AddTypeBuilder(duiType);
		return rootTypeBuilder.AddEvent(name);
	}

	private void CheckTypeForInterfaces(DirectUIXamlType duiType)
	{
		if (!_haveEntered_IValueConverter && duiType.IsValueConverter)
		{
			AddInterface("Microsoft.UI.Xaml.Data.IValueConverter");
			_haveEntered_IValueConverter = true;
		}
		if (!_haveEntered_IXamlMetadataProvider && duiType.IsMetadataProvider)
		{
			AddInterface("Microsoft.UI.Xaml.Markup.IXamlMetadataProvider");
			_haveEntered_IXamlMetadataProvider = true;
		}
	}
}
