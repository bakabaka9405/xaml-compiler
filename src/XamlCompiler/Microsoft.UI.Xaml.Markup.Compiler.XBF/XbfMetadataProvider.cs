using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

internal class XbfMetadataProvider : IXbfMetadataProvider, IXmpXamlType
{
	private DirectUISchemaContext _schema;

	private Dictionary<string, IXbfType> _standardNames = new Dictionary<string, IXbfType>();

	private Dictionary<DirectUIXamlType, IXbfType> _typeMap = new Dictionary<DirectUIXamlType, IXbfType>();

	public XbfMetadataProvider(DirectUISchemaContext schema)
	{
		_schema = schema;
	}

	public IXbfType GetXamlType(string fullName)
	{
		IXbfType value = null;
		if (!_standardNames.TryGetValue(fullName, out value))
		{
			DirectUIXamlType xamlType = (DirectUIXamlType)_schema.GetXamlType(fullName);
			value = GetXmpXamlType(xamlType);
		}
		return value;
	}

	public IXbfType GetXamlType(Type type)
	{
		DirectUIXamlType xamlType = (DirectUIXamlType)_schema.GetXamlType(type);
		return GetXmpXamlType(xamlType);
	}

	public IXbfType GetXmpXamlType(DirectUIXamlType xamlType)
	{
		if (xamlType == null)
		{
			return null;
		}
		if (!_typeMap.TryGetValue(xamlType, out var value))
		{
			value = new XbfXamlType(xamlType, this);
			_typeMap.Add(xamlType, value);
			if (value.FullName != xamlType.UnderlyingType.FullName)
			{
				_standardNames.Add(value.FullName, value);
			}
		}
		return value;
	}

	public object[] GetXmlnsDefinitions()
	{
		throw new NotImplementedException();
	}
}
