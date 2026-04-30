using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Markup;

namespace System.Xaml;

internal class NameScope : INameScopeDictionary, INameScope, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
	private class Enumerator : IEnumerator<KeyValuePair<string, object>>, IDisposable, IEnumerator
	{
		private IDictionaryEnumerator _enumerator;

		public KeyValuePair<string, object> Current
		{
			get
			{
				if (_enumerator == null)
				{
					return default(KeyValuePair<string, object>);
				}
				return new KeyValuePair<string, object>((string)_enumerator.Key, _enumerator.Value);
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(HybridDictionary nameMap)
		{
			_enumerator = null;
			if (nameMap != null)
			{
				_enumerator = nameMap.GetEnumerator();
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public bool MoveNext()
		{
			if (_enumerator == null)
			{
				return false;
			}
			return _enumerator.MoveNext();
		}

		void IEnumerator.Reset()
		{
			if (_enumerator != null)
			{
				_enumerator.Reset();
			}
		}
	}

	private HybridDictionary _nameMap;

	public int Count
	{
		get
		{
			if (_nameMap == null)
			{
				return 0;
			}
			return _nameMap.Count;
		}
	}

	public bool IsReadOnly => false;

	public object this[string key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			return FindName(key);
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			RegisterName(key, value);
		}
	}

	public ICollection<string> Keys
	{
		get
		{
			if (_nameMap == null)
			{
				return null;
			}
			List<string> list = new List<string>();
			foreach (string key in _nameMap.Keys)
			{
				list.Add(key);
			}
			return list;
		}
	}

	public ICollection<object> Values
	{
		get
		{
			if (_nameMap == null)
			{
				return null;
			}
			List<object> list = new List<object>();
			foreach (object value in _nameMap.Values)
			{
				list.Add(value);
			}
			return list;
		}
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
		if (_nameMap != null && _nameMap[name] != null)
		{
			_nameMap.Remove(name);
			return;
		}
		throw new ArgumentException(SR.Get("NameScopeNameNotFound", name));
	}

	public object FindName(string name)
	{
		if (_nameMap == null || name == null || name == string.Empty)
		{
			return null;
		}
		return _nameMap[name];
	}

	private IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		return new Enumerator(_nameMap);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Clear()
	{
		_nameMap = null;
	}

	public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
	{
		if (_nameMap == null)
		{
			array = null;
			return;
		}
		foreach (DictionaryEntry item in _nameMap)
		{
			array[arrayIndex++] = new KeyValuePair<string, object>((string)item.Key, item.Value);
		}
	}

	public bool Remove(KeyValuePair<string, object> item)
	{
		if (!Contains(item))
		{
			return false;
		}
		if (item.Value != this[item.Key])
		{
			return false;
		}
		return Remove(item.Key);
	}

	public void Add(KeyValuePair<string, object> item)
	{
		if (item.Key == null)
		{
			throw new ArgumentException(SR.Get("ReferenceIsNull", "item.Key"), "item");
		}
		if (item.Value == null)
		{
			throw new ArgumentException(SR.Get("ReferenceIsNull", "item.Value"), "item");
		}
		Add(item.Key, item.Value);
	}

	public bool Contains(KeyValuePair<string, object> item)
	{
		if (item.Key == null)
		{
			throw new ArgumentException(SR.Get("ReferenceIsNull", "item.Key"), "item");
		}
		return ContainsKey(item.Key);
	}

	public void Add(string key, object value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		RegisterName(key, value);
	}

	public bool ContainsKey(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		object obj = FindName(key);
		return obj != null;
	}

	public bool Remove(string key)
	{
		if (!ContainsKey(key))
		{
			return false;
		}
		UnregisterName(key);
		return true;
	}

	public bool TryGetValue(string key, out object value)
	{
		if (!ContainsKey(key))
		{
			value = null;
			return false;
		}
		value = FindName(key);
		return true;
	}
}
