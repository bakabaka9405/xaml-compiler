using System.Collections.Generic;
using System.Diagnostics;
using System.Xaml;
using MS.Internal.Xaml.Context;

namespace MS.Internal.Xaml.Parser;

internal class MePullParser
{
	[DebuggerDisplay("{found}")]
	private class Found
	{
		public bool found;
	}

	private XamlParserContext _context;

	private string _originalText;

	private MeScanner _tokenizer;

	private string _brokenRule;

	private int LineNumber => _tokenizer.LineNumber;

	private int LinePosition => _tokenizer.LinePosition;

	public MePullParser(XamlParserContext stack)
	{
		_context = stack;
	}

	public IEnumerable<XamlNode> Parse(string text, int lineNumber, int linePosition)
	{
		_tokenizer = new MeScanner(_context, text, lineNumber, linePosition);
		_originalText = text;
		Found f = new Found();
		NextToken();
		foreach (XamlNode item in P_MarkupExtension(f))
		{
			yield return item;
		}
		if (!f.found)
		{
			string brokenRule = _brokenRule;
			_brokenRule = null;
			throw new XamlParseException(_tokenizer, brokenRule);
		}
		if (_tokenizer.Token != MeTokenType.None)
		{
			throw new XamlParseException(_tokenizer, SR.Get("UnexpectedTokenAfterME"));
		}
		if (_tokenizer.HasTrailingWhitespace)
		{
			throw new XamlParseException(_tokenizer, SR.Get("WhitespaceAfterME"));
		}
	}

	private void SetBrokenRuleString(string ruleString)
	{
		if (string.IsNullOrEmpty(_brokenRule))
		{
			_brokenRule = SR.Get("UnexpectedToken", _tokenizer.Token, ruleString, _originalText);
		}
	}

	private bool Expect(MeTokenType token, string ruleString)
	{
		if (_tokenizer.Token != token)
		{
			SetBrokenRuleString(ruleString);
			return false;
		}
		return true;
	}

	private IEnumerable<XamlNode> P_MarkupExtension(Found f)
	{
		if (!Expect(MeTokenType.Open, "MarkupExtension ::= @'{' Expr '}'"))
		{
			yield break;
		}
		NextToken();
		if (_tokenizer.Token == MeTokenType.TypeName)
		{
			XamlType tokenType = _tokenizer.TokenType;
			yield return Logic_StartElement(tokenType, _tokenizer.Namespace);
			NextToken();
			Found f2 = new Found();
			switch (_tokenizer.Token)
			{
			case MeTokenType.Close:
				yield return Logic_EndObject();
				NextToken();
				f.found = true;
				break;
			case MeTokenType.PropertyName:
			case MeTokenType.String:
			case MeTokenType.QuotedMarkupExtension:
			case MeTokenType.Open:
				foreach (XamlNode item in P_Arguments(f2))
				{
					yield return item;
				}
				break;
			default:
				SetBrokenRuleString("MarkupExtension ::= '{' TYPENAME @(Arguments)? '}'");
				break;
			}
			if (f2.found && Expect(MeTokenType.Close, "MarkupExtension ::= '{' TYPENAME (Arguments)? @'}'"))
			{
				yield return Logic_EndObject();
				f.found = true;
				NextToken();
			}
		}
		else
		{
			SetBrokenRuleString("MarkupExtension ::= '{' @TYPENAME (Arguments)? '}'");
		}
	}

	private IEnumerable<XamlNode> P_Arguments(Found f)
	{
		Found f2 = new Found();
		switch (_tokenizer.Token)
		{
		case MeTokenType.String:
		case MeTokenType.QuotedMarkupExtension:
		case MeTokenType.Open:
			foreach (XamlNode item in P_PositionalArgs(f2))
			{
				yield return item;
			}
			f.found = f2.found;
			if (f.found && _context.CurrentArgCount > 0)
			{
				yield return Logic_EndPositionalParameters();
			}
			while (_tokenizer.Token == MeTokenType.Comma)
			{
				NextToken();
				foreach (XamlNode item2 in P_NamedArgs(f2))
				{
					yield return item2;
				}
			}
			break;
		case MeTokenType.PropertyName:
			foreach (XamlNode item3 in P_NamedArgs(f2))
			{
				yield return item3;
			}
			f.found = f2.found;
			break;
		default:
			SetBrokenRuleString("Arguments ::= @ (PositionalArgs ( ',' NamedArgs)?) | NamedArgs");
			break;
		case MeTokenType.Close:
			break;
		}
	}

	private IEnumerable<XamlNode> P_PositionalArgs(Found f)
	{
		Found f2 = new Found();
		switch (_tokenizer.Token)
		{
		case MeTokenType.String:
		case MeTokenType.QuotedMarkupExtension:
		case MeTokenType.Open:
		{
			if (_context.CurrentArgCount++ == 0)
			{
				yield return Logic_StartPositionalParameters();
			}
			foreach (XamlNode item in P_Value(f2))
			{
				yield return item;
			}
			if (!f2.found)
			{
				SetBrokenRuleString("PositionalArgs ::= (NamedArg | (@Value (',' PositionalArgs)?)");
				break;
			}
			f.found = f2.found;
			if (_tokenizer.Token != MeTokenType.Comma)
			{
				break;
			}
			Found f3 = new Found();
			NextToken();
			foreach (XamlNode item2 in P_PositionalArgs(f3))
			{
				yield return item2;
			}
			if (!f3.found)
			{
				SetBrokenRuleString("PositionalArgs ::= (Value (',' @ PositionalArgs)?) | NamedArg");
			}
			break;
		}
		case MeTokenType.PropertyName:
			if (_context.CurrentArgCount > 0)
			{
				yield return Logic_EndPositionalParameters();
			}
			foreach (XamlNode item3 in P_NamedArg(f2))
			{
				yield return item3;
			}
			if (!f2.found)
			{
				SetBrokenRuleString("PositionalArgs ::= (Value (',' PositionalArgs)?) | @ NamedArg");
			}
			f.found = f2.found;
			break;
		default:
			SetBrokenRuleString("PositionalArgs ::= @ (Value (',' PositionalArgs)?) | NamedArg");
			break;
		}
	}

	private IEnumerable<XamlNode> P_NamedArgs(Found f)
	{
		Found f2 = new Found();
		MeTokenType token = _tokenizer.Token;
		if (token == MeTokenType.PropertyName)
		{
			foreach (XamlNode item in P_NamedArg(f2))
			{
				yield return item;
			}
			f.found = f2.found;
			while (_tokenizer.Token == MeTokenType.Comma)
			{
				NextToken();
				foreach (XamlNode item2 in P_NamedArg(f2))
				{
					yield return item2;
				}
			}
		}
		else
		{
			SetBrokenRuleString("NamedArgs ::= @NamedArg ( ',' NamedArg )*");
		}
	}

	private IEnumerable<XamlNode> P_Value(Found f)
	{
		Found f2 = new Found();
		switch (_tokenizer.Token)
		{
		case MeTokenType.String:
			yield return Logic_Text();
			f.found = true;
			NextToken();
			break;
		case MeTokenType.QuotedMarkupExtension:
		{
			MePullParser mePullParser = new MePullParser(_context);
			foreach (XamlNode item in mePullParser.Parse(_tokenizer.TokenText, LineNumber, LinePosition))
			{
				yield return item;
			}
			f.found = true;
			NextToken();
			break;
		}
		case MeTokenType.Open:
			foreach (XamlNode item2 in P_MarkupExtension(f2))
			{
				yield return item2;
			}
			f.found = f2.found;
			break;
		}
	}

	private IEnumerable<XamlNode> P_NamedArg(Found f)
	{
		Found f2 = new Found();
		if (_tokenizer.Token != MeTokenType.PropertyName)
		{
			yield break;
		}
		_ = _tokenizer.TokenProperty;
		yield return Logic_StartMember();
		NextToken();
		Expect(MeTokenType.EqualSign, "NamedArg ::= PROPERTYNAME @'=' Value");
		NextToken();
		switch (_tokenizer.Token)
		{
		case MeTokenType.String:
			yield return Logic_Text();
			f.found = true;
			NextToken();
			break;
		case MeTokenType.QuotedMarkupExtension:
		{
			MePullParser mePullParser = new MePullParser(_context);
			foreach (XamlNode item in mePullParser.Parse(_tokenizer.TokenText, LineNumber, LinePosition))
			{
				yield return item;
			}
			f.found = true;
			NextToken();
			break;
		}
		case MeTokenType.Open:
			foreach (XamlNode item2 in P_Value(f2))
			{
				yield return item2;
			}
			f.found = f2.found;
			break;
		case MeTokenType.PropertyName:
			throw new XamlParseException(message: (!(_context.CurrentMember == null)) ? SR.Get("MissingComma2", _context.CurrentMember.Name, _tokenizer.TokenText) : SR.Get("MissingComma1", _tokenizer.TokenText), meScanner: _tokenizer);
		default:
			SetBrokenRuleString("NamedArg ::= PROPERTYNAME '=' @(STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)");
			break;
		}
		yield return Logic_EndMember();
	}

	private void NextToken()
	{
		_tokenizer.Read();
	}

	private XamlNode Logic_StartElement(XamlType xamlType, string xamlNamespace)
	{
		_context.PushScope();
		_context.CurrentType = xamlType;
		_context.CurrentTypeNamespace = xamlNamespace;
		_context.InitLongestConstructor(xamlType);
		_context.InitBracketCharacterCacheForType(xamlType);
		_context.CurrentBracketModeParseParameters = new BracketModeParseParameters(_context);
		return new XamlNode(XamlNodeType.StartObject, xamlType);
	}

	private XamlNode Logic_EndObject()
	{
		_context.PopScope();
		return new XamlNode(XamlNodeType.EndObject);
	}

	private XamlNode Logic_StartMember()
	{
		XamlMember tokenProperty = _tokenizer.TokenProperty;
		_context.CurrentMember = tokenProperty;
		return new XamlNode(XamlNodeType.StartMember, tokenProperty);
	}

	private XamlNode Logic_EndMember()
	{
		_context.CurrentMember = null;
		return new XamlNode(XamlNodeType.EndMember);
	}

	private XamlNode Logic_StartPositionalParameters()
	{
		_context.CurrentMember = XamlLanguage.PositionalParameters;
		return new XamlNode(XamlNodeType.StartMember, XamlLanguage.PositionalParameters);
	}

	private XamlNode Logic_EndPositionalParameters()
	{
		XamlType currentType = _context.CurrentType;
		_context.CurrentArgCount = 0;
		_context.CurrentMember = null;
		return new XamlNode(XamlNodeType.EndMember);
	}

	private XamlNode Logic_Text()
	{
		string tokenText = _tokenizer.TokenText;
		return new XamlNode(XamlNodeType.Value, tokenText);
	}
}
