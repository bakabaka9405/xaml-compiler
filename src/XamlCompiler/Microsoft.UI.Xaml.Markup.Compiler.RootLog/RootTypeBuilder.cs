using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler.RootLog;

internal class RootTypeBuilder
{
	private Dictionary<string, RootProperty> _propertyDict = new Dictionary<string, RootProperty>();

	private Dictionary<string, RootMethod> _MethodDict = new Dictionary<string, RootMethod>();

	private Dictionary<string, RootEvent> _EventDict = new Dictionary<string, RootEvent>();

	public string FullName { get; private set; }

	public RootTypeBuilder(string standardName)
	{
		FullName = standardName;
	}

	public RootMember AddProperty(string name)
	{
		if (!_propertyDict.TryGetValue(name, out var value))
		{
			value = new RootProperty
			{
				Name = name
			};
			_propertyDict.Add(name, value);
		}
		return value;
	}

	public RootMember AddEvent(string name)
	{
		if (!_EventDict.TryGetValue(name, out var value))
		{
			value = new RootEvent
			{
				Name = name
			};
			_EventDict.Add(name, value);
		}
		return value;
	}

	public RootMember AddMethod(string name)
	{
		if (!_MethodDict.TryGetValue(name, out var value))
		{
			value = new RootMethod
			{
				Name = name
			};
			_MethodDict.Add(name, value);
		}
		return value;
	}

	public RootType GetRootType()
	{
		RootType rootType = new RootType();
		rootType.FullName = FullName;
		rootType.Members.AddRange(_propertyDict.Values);
		rootType.Members.AddRange(_EventDict.Values);
		rootType.Members.AddRange(_MethodDict.Values);
		return rootType;
	}
}
