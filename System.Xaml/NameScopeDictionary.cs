using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Markup;
using System.Xaml.MS.Impl;

namespace System.Xaml;

internal class NameScopeDictionary : INameScopeDictionary, INameScope, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
	private class Enumerator : IEnumerator<KeyValuePair<string, object>>, IDisposable, IEnumerator
	{
		private int index;

		private IDictionaryEnumerator dictionaryEnumerator;

		private HybridDictionary _nameMap;

		private INameScope _underlyingNameScope;

		private FrugalObjectList<string> _names;

		public KeyValuePair<string, object> Current
		{
			get
			{
				if (_underlyingNameScope != null)
				{
					string text = _names[index];
					return new KeyValuePair<string, object>(text, _underlyingNameScope.FindName(text));
				}
				if (_nameMap != null)
				{
					return new KeyValuePair<string, object>((string)dictionaryEnumerator.Key, dictionaryEnumerator.Value);
				}
				return default(KeyValuePair<string, object>);
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(NameScopeDictionary nameScopeDictionary)
		{
			_nameMap = nameScopeDictionary._nameMap;
			_underlyingNameScope = nameScopeDictionary._underlyingNameScope;
			_names = nameScopeDictionary._names;
			if (_underlyingNameScope != null)
			{
				index = -1;
			}
			else if (_nameMap != null)
			{
				dictionaryEnumerator = _nameMap.GetEnumerator();
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public bool MoveNext()
		{
			if (_underlyingNameScope != null)
			{
				if (index == _names.Count - 1)
				{
					return false;
				}
				index++;
				return true;
			}
			if (_nameMap != null)
			{
				return dictionaryEnumerator.MoveNext();
			}
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_underlyingNameScope != null)
			{
				index = -1;
			}
			else
			{
				dictionaryEnumerator.Reset();
			}
		}
	}

	private HybridDictionary _nameMap;

	private INameScope _underlyingNameScope;

	private FrugalObjectList<string> _names;

	internal INameScope UnderlyingNameScope => _underlyingNameScope;

	int ICollection<KeyValuePair<string, object>>.Count
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	bool ICollection<KeyValuePair<string, object>>.IsReadOnly
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	object IDictionary<string, object>.this[string key]
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	ICollection<string> IDictionary<string, object>.Keys
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	ICollection<object> IDictionary<string, object>.Values
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public NameScopeDictionary()
	{
	}

	public NameScopeDictionary(INameScope underlyingNameScope)
	{
		if (underlyingNameScope == null)
		{
			throw new ArgumentNullException("underlyingNameScope");
		}
		_names = new FrugalObjectList<string>();
		_underlyingNameScope = underlyingNameScope;
	}

	public void RegisterName(string name, object scopedElement)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (scopedElement == null)
		{
			throw new ArgumentNullException("scopedElement");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException(SR.Get("NameScopeNameNotEmptyString"));
		}
		if (!NameValidationHelper.IsValidIdentifierName(name))
		{
			throw new ArgumentException(SR.Get("NameScopeInvalidIdentifierName", name));
		}
		if (_underlyingNameScope != null)
		{
			_names.Add(name);
			_underlyingNameScope.RegisterName(name, scopedElement);
			return;
		}
		if (_nameMap == null)
		{
			_nameMap = new HybridDictionary();
			_nameMap[name] = scopedElement;
			return;
		}
		object obj = _nameMap[name];
		if (obj == null)
		{
			_nameMap[name] = scopedElement;
		}
		else if (scopedElement != obj)
		{
			throw new ArgumentException(SR.Get("NameScopeDuplicateNamesNotAllowed", name));
		}
	}

	public void UnregisterName(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException(SR.Get("NameScopeNameNotEmptyString"));
		}
		if (_underlyingNameScope != null)
		{
			_underlyingNameScope.UnregisterName(name);
			_names.Remove(name);
			return;
		}
		if (_nameMap != null && _nameMap[name] != null)
		{
			_nameMap.Remove(name);
			return;
		}
		throw new ArgumentException(SR.Get("NameScopeNameNotFound", name));
	}

	public object FindName(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException(SR.Get("NameScopeNameNotEmptyString"));
		}
		if (_underlyingNameScope != null)
		{
			return _underlyingNameScope.FindName(name);
		}
		if (_nameMap == null)
		{
			return null;
		}
		return _nameMap[name];
	}

	private IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	void ICollection<KeyValuePair<string, object>>.Clear()
	{
		throw new NotImplementedException();
	}

	void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}

	bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
	{
		throw new NotImplementedException();
	}

	void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
	{
		throw new NotImplementedException();
	}

	bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
	{
		throw new NotImplementedException();
	}

	void IDictionary<string, object>.Add(string key, object value)
	{
		throw new NotImplementedException();
	}

	bool IDictionary<string, object>.ContainsKey(string key)
	{
		throw new NotImplementedException();
	}

	bool IDictionary<string, object>.Remove(string key)
	{
		throw new NotImplementedException();
	}

	bool IDictionary<string, object>.TryGetValue(string key, out object value)
	{
		throw new NotImplementedException();
	}
}
