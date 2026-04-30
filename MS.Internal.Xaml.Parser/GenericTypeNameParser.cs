using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xaml;
using System.Xaml.Schema;

namespace MS.Internal.Xaml.Parser;

internal class GenericTypeNameParser
{
	[Serializable]
	private class TypeNameParserException : Exception
	{
		public TypeNameParserException(string message)
			: base(message)
		{
		}

		protected TypeNameParserException(SerializationInfo si, StreamingContext sc)
			: base(si, sc)
		{
		}
	}

	private GenericTypeNameScanner _scanner;

	private string _inputText;

	private Func<string, string> _prefixResolver;

	private Stack<TypeNameFrame> _stack;

	public GenericTypeNameParser(Func<string, string> prefixResolver)
	{
		_prefixResolver = prefixResolver;
	}

	public static XamlTypeName ParseIfTrivalName(string text, Func<string, string> prefixResolver, out string error)
	{
		int num = text.IndexOf('(');
		int num2 = text.IndexOf('[');
		if (num != -1 || num2 != -1)
		{
			error = string.Empty;
			return null;
		}
		error = string.Empty;
		if (!XamlQualifiedName.Parse(text, out var prefix, out var name))
		{
			error = SR.Get("InvalidTypeString", text);
			return null;
		}
		string text2 = prefixResolver(prefix);
		if (string.IsNullOrEmpty(text2))
		{
			error = SR.Get("PrefixNotFound", prefix);
			return null;
		}
		return new XamlTypeName(text2, name);
	}

	public XamlTypeName ParseName(string text, out string error)
	{
		error = string.Empty;
		_scanner = new GenericTypeNameScanner(text);
		_inputText = text;
		StartStack();
		try
		{
			_scanner.Read();
			P_XamlTypeName();
			if (_scanner.Token != GenericTypeNameScannerToken.NONE)
			{
				ThrowOnBadInput();
			}
		}
		catch (TypeNameParserException ex)
		{
			error = ex.Message;
		}
		XamlTypeName result = null;
		if (string.IsNullOrEmpty(error))
		{
			result = CollectNameFromStack();
		}
		return result;
	}

	public IList<XamlTypeName> ParseList(string text, out string error)
	{
		_scanner = new GenericTypeNameScanner(text);
		_inputText = text;
		StartStack();
		error = string.Empty;
		try
		{
			_scanner.Read();
			P_XamlTypeNameList();
			if (_scanner.Token != GenericTypeNameScannerToken.NONE)
			{
				ThrowOnBadInput();
			}
		}
		catch (TypeNameParserException ex)
		{
			error = ex.Message;
		}
		IList<XamlTypeName> result = null;
		if (string.IsNullOrEmpty(error))
		{
			result = CollectNameListFromStack();
		}
		return result;
	}

	private void P_XamlTypeName()
	{
		if (_scanner.Token != GenericTypeNameScannerToken.NAME)
		{
			ThrowOnBadInput();
		}
		P_SimpleTypeName();
		if (_scanner.Token == GenericTypeNameScannerToken.OPEN)
		{
			P_TypeParameters();
		}
		if (_scanner.Token == GenericTypeNameScannerToken.SUBSCRIPT)
		{
			P_RepeatingSubscript();
		}
		Callout_EndOfType();
	}

	private void P_SimpleTypeName()
	{
		string prefix = string.Empty;
		string multiCharTokenText = _scanner.MultiCharTokenText;
		_scanner.Read();
		if (_scanner.Token == GenericTypeNameScannerToken.COLON)
		{
			prefix = multiCharTokenText;
			_scanner.Read();
			if (_scanner.Token != GenericTypeNameScannerToken.NAME)
			{
				ThrowOnBadInput();
			}
			multiCharTokenText = _scanner.MultiCharTokenText;
			_scanner.Read();
		}
		Callout_FoundName(prefix, multiCharTokenText);
	}

	private void P_TypeParameters()
	{
		_scanner.Read();
		P_XamlTypeNameList();
		if (_scanner.Token != GenericTypeNameScannerToken.CLOSE)
		{
			ThrowOnBadInput();
		}
		_scanner.Read();
	}

	private void P_XamlTypeNameList()
	{
		P_XamlTypeName();
		while (_scanner.Token == GenericTypeNameScannerToken.COMMA)
		{
			P_NameListExt();
		}
	}

	private void P_NameListExt()
	{
		_scanner.Read();
		P_XamlTypeName();
	}

	private void P_RepeatingSubscript()
	{
		do
		{
			Callout_Subscript(_scanner.MultiCharTokenText);
			_scanner.Read();
		}
		while (_scanner.Token == GenericTypeNameScannerToken.SUBSCRIPT);
	}

	private void ThrowOnBadInput()
	{
		throw new TypeNameParserException(SR.Get("InvalidCharInTypeName", _scanner.ErrorCurrentChar, _inputText));
	}

	private void StartStack()
	{
		_stack = new Stack<TypeNameFrame>();
		TypeNameFrame item = new TypeNameFrame();
		_stack.Push(item);
	}

	private void Callout_FoundName(string prefix, string name)
	{
		TypeNameFrame typeNameFrame = new TypeNameFrame();
		typeNameFrame.Name = name;
		string text = _prefixResolver(prefix);
		if (text == null)
		{
			throw new TypeNameParserException(SR.Get("PrefixNotFound", prefix));
		}
		typeNameFrame.Namespace = text;
		_stack.Push(typeNameFrame);
	}

	private void Callout_EndOfType()
	{
		TypeNameFrame typeNameFrame = _stack.Pop();
		XamlTypeName item = new XamlTypeName(typeNameFrame.Namespace, typeNameFrame.Name, typeNameFrame.TypeArgs);
		typeNameFrame = _stack.Peek();
		if (typeNameFrame.TypeArgs == null)
		{
			typeNameFrame.AllocateTypeArgs();
		}
		typeNameFrame.TypeArgs.Add(item);
	}

	private void Callout_Subscript(string subscript)
	{
		_stack.Peek().Name += subscript;
	}

	private XamlTypeName CollectNameFromStack()
	{
		if (_stack.Count != 1)
		{
			throw new TypeNameParserException(SR.Get("InvalidTypeString", _inputText));
		}
		TypeNameFrame typeNameFrame = _stack.Peek();
		if (typeNameFrame.TypeArgs.Count != 1)
		{
			throw new TypeNameParserException(SR.Get("InvalidTypeString", _inputText));
		}
		return typeNameFrame.TypeArgs[0];
	}

	private IList<XamlTypeName> CollectNameListFromStack()
	{
		if (_stack.Count != 1)
		{
			throw new TypeNameParserException(SR.Get("InvalidTypeString", _inputText));
		}
		TypeNameFrame typeNameFrame = _stack.Peek();
		return typeNameFrame.TypeArgs;
	}
}
