using System;
using System.Collections.Generic;
using System.Text;
using System.Xaml;
using System.Xaml.Context;
using System.Xaml.MS.Impl;
using System.Xaml.Schema;

namespace MS.Internal.Xaml.Context;

internal class NameFixupGraph
{
	private Dictionary<object, FrugalObjectList<NameFixupToken>> _dependenciesByParentObject;

	private Dictionary<object, NameFixupToken> _dependenciesByChildObject;

	private Dictionary<string, FrugalObjectList<NameFixupToken>> _dependenciesByName;

	private Queue<NameFixupToken> _resolvedTokensPendingProcessing;

	private NameFixupToken _deferredRootProvideValue;

	private System.Xaml.Context.HashSet<object> _uninitializedObjectsAtParseEnd;

	public bool HasResolvedTokensPendingProcessing => _resolvedTokensPendingProcessing.Count > 0;

	public NameFixupGraph()
	{
		ReferenceEqualityComparer<object> singleton = ReferenceEqualityComparer<object>.Singleton;
		_dependenciesByChildObject = new Dictionary<object, NameFixupToken>(singleton);
		_dependenciesByName = new Dictionary<string, FrugalObjectList<NameFixupToken>>(StringComparer.Ordinal);
		_dependenciesByParentObject = new Dictionary<object, FrugalObjectList<NameFixupToken>>(singleton);
		_resolvedTokensPendingProcessing = new Queue<NameFixupToken>();
		_uninitializedObjectsAtParseEnd = new System.Xaml.Context.HashSet<object>(singleton);
	}

	public void AddDependency(NameFixupToken fixupToken)
	{
		if (fixupToken.Target.Property == null)
		{
			_deferredRootProvideValue = fixupToken;
			return;
		}
		object instance = fixupToken.Target.Instance;
		AddToMultiDict(_dependenciesByParentObject, instance, fixupToken);
		if (fixupToken.ReferencedObject != null)
		{
			_dependenciesByChildObject.Add(fixupToken.ReferencedObject, fixupToken);
			return;
		}
		foreach (string neededName in fixupToken.NeededNames)
		{
			AddToMultiDict(_dependenciesByName, neededName, fixupToken);
		}
	}

	public bool HasUnresolvedChildren(object parent)
	{
		if (parent == null)
		{
			return false;
		}
		return _dependenciesByParentObject.ContainsKey(parent);
	}

	public bool HasUnresolvedOrPendingChildren(object instance)
	{
		if (HasUnresolvedChildren(instance))
		{
			return true;
		}
		foreach (NameFixupToken item in _resolvedTokensPendingProcessing)
		{
			if (item.Target.Instance == instance)
			{
				return true;
			}
		}
		return false;
	}

	public bool WasUninitializedAtEndOfParse(object instance)
	{
		return _uninitializedObjectsAtParseEnd.ContainsKey(instance);
	}

	public void GetDependentNames(object instance, List<string> result)
	{
		if (!_dependenciesByParentObject.TryGetValue(instance, out var value))
		{
			return;
		}
		for (int i = 0; i < value.Count; i++)
		{
			NameFixupToken nameFixupToken = value[i];
			if (nameFixupToken.FixupType == FixupType.MarkupExtensionFirstRun || nameFixupToken.FixupType == FixupType.UnresolvedChildren)
			{
				GetDependentNames(nameFixupToken.ReferencedObject, result);
			}
			else
			{
				if (nameFixupToken.NeededNames == null)
				{
					continue;
				}
				foreach (string neededName in nameFixupToken.NeededNames)
				{
					if (!result.Contains(neededName))
					{
						result.Add(neededName);
					}
				}
			}
		}
	}

	public void ResolveDependenciesTo(object instance, string name)
	{
		NameFixupToken value = null;
		if (instance != null && _dependenciesByChildObject.TryGetValue(instance, out value))
		{
			_dependenciesByChildObject.Remove(instance);
			RemoveTokenByParent(value);
			_resolvedTokensPendingProcessing.Enqueue(value);
		}
		if (name == null || !_dependenciesByName.TryGetValue(name, out var value2))
		{
			return;
		}
		int num = 0;
		while (num < value2.Count)
		{
			value = value2[num];
			object obj = value.ResolveName(name);
			if (instance != obj)
			{
				num++;
				continue;
			}
			if (value.CanAssignDirectly)
			{
				value.ReferencedObject = instance;
			}
			value.NeededNames.Remove(name);
			value2.RemoveAt(num);
			if (value2.Count == 0)
			{
				_dependenciesByName.Remove(name);
			}
			if (value.NeededNames.Count == 0)
			{
				RemoveTokenByParent(value);
				_resolvedTokensPendingProcessing.Enqueue(value);
			}
		}
	}

	public NameFixupToken GetNextResolvedTokenPendingProcessing()
	{
		return _resolvedTokensPendingProcessing.Dequeue();
	}

	public void IsOffTheStack(object instance, string name, int lineNumber, int linePosition)
	{
		if (_dependenciesByParentObject.TryGetValue(instance, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i].Target.InstanceIsOnTheStack = false;
				value[i].Target.InstanceName = name;
				value[i].Target.EndInstanceLineNumber = lineNumber;
				value[i].Target.EndInstanceLinePosition = linePosition;
			}
		}
	}

	public void AddEndOfParseDependency(object childThatHasUnresolvedChildren, FixupTarget parentObject)
	{
		NameFixupToken nameFixupToken = new NameFixupToken();
		nameFixupToken.Target = parentObject;
		nameFixupToken.FixupType = FixupType.UnresolvedChildren;
		nameFixupToken.ReferencedObject = childThatHasUnresolvedChildren;
		AddToMultiDict(_dependenciesByParentObject, parentObject.Instance, nameFixupToken);
	}

	public IEnumerable<NameFixupToken> GetRemainingSimpleFixups()
	{
		foreach (object key in _dependenciesByParentObject.Keys)
		{
			_uninitializedObjectsAtParseEnd.Add(key);
		}
		List<string> list = new List<string>(_dependenciesByName.Keys);
		foreach (string name in list)
		{
			FrugalObjectList<NameFixupToken> dependencies = _dependenciesByName[name];
			int i = 0;
			while (i < dependencies.Count)
			{
				NameFixupToken nameFixupToken = dependencies[i];
				if (!nameFixupToken.CanAssignDirectly)
				{
					i++;
					continue;
				}
				dependencies.RemoveAt(i);
				if (dependencies.Count == 0)
				{
					_dependenciesByName.Remove(name);
				}
				RemoveTokenByParent(nameFixupToken);
				yield return nameFixupToken;
			}
		}
	}

	public IEnumerable<NameFixupToken> GetRemainingReparses()
	{
		List<object> list = new List<object>(_dependenciesByParentObject.Keys);
		foreach (object parentObj in list)
		{
			FrugalObjectList<NameFixupToken> dependencies = _dependenciesByParentObject[parentObj];
			int i = 0;
			while (i < dependencies.Count)
			{
				NameFixupToken nameFixupToken = dependencies[i];
				if (nameFixupToken.FixupType == FixupType.MarkupExtensionFirstRun || nameFixupToken.FixupType == FixupType.UnresolvedChildren)
				{
					i++;
					continue;
				}
				dependencies.RemoveAt(i);
				if (dependencies.Count == 0)
				{
					_dependenciesByParentObject.Remove(parentObj);
				}
				foreach (string neededName in nameFixupToken.NeededNames)
				{
					FrugalObjectList<NameFixupToken> frugalObjectList = _dependenciesByName[neededName];
					if (frugalObjectList.Count == 1)
					{
						frugalObjectList.Remove(nameFixupToken);
					}
					else
					{
						_dependenciesByName.Remove(neededName);
					}
				}
				yield return nameFixupToken;
			}
		}
	}

	public IEnumerable<NameFixupToken> GetRemainingObjectDependencies()
	{
		List<NameFixupToken> markupExtensionTokens = new List<NameFixupToken>();
		foreach (NameFixupToken value in _dependenciesByChildObject.Values)
		{
			if (value.FixupType == FixupType.MarkupExtensionFirstRun)
			{
				markupExtensionTokens.Add(value);
			}
		}
		while (markupExtensionTokens.Count > 0)
		{
			bool flag = false;
			int i = 0;
			while (i < markupExtensionTokens.Count)
			{
				NameFixupToken inEdge = markupExtensionTokens[i];
				List<NameFixupToken> dependencies = new List<NameFixupToken>();
				if (!FindDependencies(inEdge, dependencies))
				{
					i++;
					continue;
				}
				for (int j = dependencies.Count - 1; j >= 0; j--)
				{
					NameFixupToken nameFixupToken = dependencies[j];
					RemoveTokenByParent(nameFixupToken);
					yield return nameFixupToken;
				}
				flag = true;
				markupExtensionTokens.RemoveAt(i);
			}
			if (!flag)
			{
				ThrowProvideValueCycle(markupExtensionTokens);
			}
		}
		while (_dependenciesByParentObject.Count > 0)
		{
			FrugalObjectList<NameFixupToken> startNodeOutEdges = null;
			using (Dictionary<object, FrugalObjectList<NameFixupToken>>.ValueCollection.Enumerator enumerator2 = _dependenciesByParentObject.Values.GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					FrugalObjectList<NameFixupToken> current2 = enumerator2.Current;
					startNodeOutEdges = current2;
				}
			}
			for (int i = 0; i < startNodeOutEdges.Count; i++)
			{
				List<NameFixupToken> dependencies = new List<NameFixupToken>();
				FindDependencies(startNodeOutEdges[i], dependencies);
				for (int j = dependencies.Count - 1; j >= 0; j--)
				{
					NameFixupToken nameFixupToken2 = dependencies[j];
					RemoveTokenByParent(nameFixupToken2);
					yield return nameFixupToken2;
				}
			}
		}
		if (_deferredRootProvideValue != null)
		{
			yield return _deferredRootProvideValue;
		}
	}

	private bool FindDependencies(NameFixupToken inEdge, List<NameFixupToken> alreadyTraversed)
	{
		if (alreadyTraversed.Contains(inEdge))
		{
			return true;
		}
		alreadyTraversed.Add(inEdge);
		if (inEdge.ReferencedObject == null || !_dependenciesByParentObject.TryGetValue(inEdge.ReferencedObject, out var value))
		{
			return true;
		}
		for (int i = 0; i < value.Count; i++)
		{
			NameFixupToken nameFixupToken = value[i];
			if (nameFixupToken.FixupType == FixupType.MarkupExtensionFirstRun)
			{
				return false;
			}
			if (!FindDependencies(nameFixupToken, alreadyTraversed))
			{
				return false;
			}
		}
		return true;
	}

	private void RemoveTokenByParent(NameFixupToken token)
	{
		object instance = token.Target.Instance;
		FrugalObjectList<NameFixupToken> frugalObjectList = _dependenciesByParentObject[instance];
		if (frugalObjectList.Count == 1)
		{
			_dependenciesByParentObject.Remove(instance);
		}
		else
		{
			frugalObjectList.Remove(token);
		}
	}

	private static void AddToMultiDict<TKey>(Dictionary<TKey, FrugalObjectList<NameFixupToken>> dict, TKey key, NameFixupToken value)
	{
		if (!dict.TryGetValue(key, out var value2))
		{
			value2 = new FrugalObjectList<NameFixupToken>(1);
			dict.Add(key, value2);
		}
		value2.Add(value);
	}

	private static void ThrowProvideValueCycle(IEnumerable<NameFixupToken> markupExtensionTokens)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(SR.Get("ProvideValueCycle"));
		foreach (NameFixupToken markupExtensionToken in markupExtensionTokens)
		{
			stringBuilder.AppendLine();
			string text = markupExtensionToken.ReferencedObject.ToString();
			if (markupExtensionToken.LineNumber != 0)
			{
				if (markupExtensionToken.LinePosition != 0)
				{
					stringBuilder.Append(SR.Get("LineNumberAndPosition", text, markupExtensionToken.LineNumber, markupExtensionToken.LinePosition));
				}
				else
				{
					stringBuilder.Append(SR.Get("LineNumberOnly", text, markupExtensionToken.LineNumber));
				}
			}
			else
			{
				stringBuilder.Append(text);
			}
		}
		throw new XamlObjectWriterException(stringBuilder.ToString());
	}
}
