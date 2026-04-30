using System.Collections.Generic;
using System.Xaml;

namespace MS.Internal.Xaml.Parser;

internal class XamlScannerStack
{
	private Stack<XamlScannerFrame> _stack;

	public int Depth => _stack.Count - 1;

	public XamlType CurrentType
	{
		get
		{
			if (_stack.Count != 0)
			{
				return _stack.Peek().XamlType;
			}
			return null;
		}
	}

	public string CurrentTypeNamespace
	{
		get
		{
			if (_stack.Count != 0)
			{
				return _stack.Peek().TypeNamespace;
			}
			return null;
		}
	}

	public XamlMember CurrentProperty
	{
		get
		{
			if (_stack.Count != 0)
			{
				return _stack.Peek().XamlProperty;
			}
			return null;
		}
		set
		{
			_stack.Peek().XamlProperty = value;
		}
	}

	public bool CurrentXmlSpacePreserve
	{
		get
		{
			if (_stack.Count != 0)
			{
				return _stack.Peek().XmlSpacePreserve;
			}
			return false;
		}
		set
		{
			_stack.Peek().XmlSpacePreserve = value;
		}
	}

	public bool CurrentlyInContent
	{
		get
		{
			if (_stack.Count != 0)
			{
				return _stack.Peek().InContent;
			}
			return false;
		}
		set
		{
			_stack.Peek().InContent = value;
		}
	}

	public XamlScannerStack()
	{
		_stack = new Stack<XamlScannerFrame>();
		_stack.Push(new XamlScannerFrame(null, null));
	}

	public void Push(XamlType type, string ns)
	{
		bool currentXmlSpacePreserve = CurrentXmlSpacePreserve;
		_stack.Push(new XamlScannerFrame(type, ns));
		CurrentXmlSpacePreserve = currentXmlSpacePreserve;
	}

	public void Pop()
	{
		_stack.Pop();
	}
}
