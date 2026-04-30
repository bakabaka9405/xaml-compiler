using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xaml;

namespace System.Windows.Markup;

[TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[TypeConverter(typeof(TypeExtensionConverter))]
[MarkupExtensionReturnType(typeof(Type))]
public class TypeExtension : MarkupExtension
{
	private string _typeName;

	private Type _type;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string TypeName
	{
		get
		{
			return _typeName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_typeName = value;
			_type = null;
		}
	}

	[DefaultValue(null)]
	[ConstructorArgument("type")]
	public Type Type
	{
		get
		{
			return _type;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_type = value;
			_typeName = null;
		}
	}

	public TypeExtension()
	{
	}

	public TypeExtension(string typeName)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		_typeName = typeName;
	}

	public TypeExtension(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		_type = type;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (_type != null)
		{
			return _type;
		}
		if (_typeName == null)
		{
			throw new InvalidOperationException(SR.Get("MarkupExtensionTypeName"));
		}
		if (serviceProvider == null)
		{
			throw new ArgumentNullException("serviceProvider");
		}
		if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver xamlTypeResolver))
		{
			throw new InvalidOperationException(SR.Get("MarkupExtensionNoContext", GetType().Name, "IXamlTypeResolver"));
		}
		_type = xamlTypeResolver.Resolve(_typeName);
		if (_type == null)
		{
			throw new InvalidOperationException(SR.Get("MarkupExtensionTypeNameBad", _typeName));
		}
		return _type;
	}
}
