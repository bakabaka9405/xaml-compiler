using System.Collections.Generic;
using System.Windows.Markup;
using System.Xaml;
using MS.Internal.Xaml.Runtime;

namespace MS.Internal.Xaml.Context;

internal class NameFixupToken : IAddLineInfo
{
	private List<string> _names;

	private List<INameScopeDictionary> _nameScopeDictionaryList;

	private XamlRuntime _runtime;

	private ObjectWriterContext _targetContext;

	public bool CanAssignDirectly { get; set; }

	public FixupType FixupType { get; set; }

	public int LineNumber { get; set; }

	public int LinePosition { get; set; }

	public FixupTarget Target { get; set; }

	public XamlRuntime Runtime
	{
		get
		{
			return _runtime;
		}
		set
		{
			_runtime = value;
		}
	}

	public ObjectWriterContext TargetContext
	{
		get
		{
			if (_targetContext == null)
			{
				_targetContext = new ObjectWriterContext(SavedContext, null, null, Runtime);
			}
			return _targetContext;
		}
	}

	public XamlSavedContext SavedContext { get; set; }

	public List<INameScopeDictionary> NameScopeDictionaryList => _nameScopeDictionaryList;

	public List<string> NeededNames => _names;

	public object ReferencedObject { get; set; }

	public NameFixupToken()
	{
		_names = new List<string>();
		_nameScopeDictionaryList = new List<INameScopeDictionary>();
		Target = new FixupTarget();
		Target.TemporaryCollectionIndex = -1;
		Target.InstanceIsOnTheStack = true;
	}

	internal object ResolveName(string name)
	{
		object obj = null;
		if (CanAssignDirectly)
		{
			foreach (INameScopeDictionary nameScopeDictionary in NameScopeDictionaryList)
			{
				obj = nameScopeDictionary.FindName(name);
				if (obj != null)
				{
					break;
				}
			}
		}
		else
		{
			TargetContext.IsInitializedCallback = null;
			obj = TargetContext.ResolveName(name, out var _);
		}
		return obj;
	}

	XamlException IAddLineInfo.WithLineInfo(XamlException ex)
	{
		if (LineNumber > 0)
		{
			ex.SetLineInfo(LineNumber, LinePosition);
		}
		return ex;
	}
}
