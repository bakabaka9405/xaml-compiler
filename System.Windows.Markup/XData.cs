using System.IO;
using System.Xml;

namespace System.Windows.Markup;

[ContentProperty("Text")]
public sealed class XData
{
	private XmlReader _reader;

	private string _text;

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
			_reader = null;
		}
	}

	public object XmlReader
	{
		get
		{
			if (_reader == null)
			{
				StringReader input = new StringReader(Text);
				_reader = System.Xml.XmlReader.Create(input);
			}
			return _reader;
		}
		set
		{
			_reader = value as XmlReader;
			_text = null;
		}
	}
}
