using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xaml;

namespace System.Windows.Markup;

[TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[ContentProperty("Items")]
[MarkupExtensionReturnType(typeof(Array))]
public class ArrayExtension : MarkupExtension
{
	private ArrayList _arrayList = new ArrayList();

	private Type _arrayType;

	[ConstructorArgument("type")]
	public Type Type
	{
		get
		{
			return _arrayType;
		}
		set
		{
			_arrayType = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public IList Items => _arrayList;

	public ArrayExtension()
	{
	}

	public ArrayExtension(Type arrayType)
	{
		if (arrayType == null)
		{
			throw new ArgumentNullException("arrayType");
		}
		_arrayType = arrayType;
	}

	public ArrayExtension(Array elements)
	{
		if (elements == null)
		{
			throw new ArgumentNullException("elements");
		}
		_arrayList.AddRange(elements);
		_arrayType = elements.GetType().GetElementType();
	}

	public void AddChild(object value)
	{
		_arrayList.Add(value);
	}

	public void AddText(string text)
	{
		AddChild(text);
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (_arrayType == null)
		{
			throw new InvalidOperationException(SR.Get("MarkupExtensionArrayType"));
		}
		try
		{
			return _arrayList.ToArray(_arrayType);
		}
		catch (InvalidCastException)
		{
			throw new InvalidOperationException(SR.Get("MarkupExtensionArrayBadType", _arrayType.Name));
		}
	}
}
