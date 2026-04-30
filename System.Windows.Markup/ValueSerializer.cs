using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xaml;
using System.Xaml.Replacements;
using MS.Internal.Serialization;

namespace System.Windows.Markup;

[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public abstract class ValueSerializer
{
	private static List<Type> Empty;

	private static object _valueSerializersLock;

	private static Hashtable _valueSerializers;

	public virtual bool CanConvertToString(object value, IValueSerializerContext context)
	{
		return false;
	}

	public virtual bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return false;
	}

	public virtual string ConvertToString(object value, IValueSerializerContext context)
	{
		throw GetConvertToException(value, typeof(string));
	}

	public virtual object ConvertFromString(string value, IValueSerializerContext context)
	{
		throw GetConvertFromException(value);
	}

	public virtual IEnumerable<Type> TypeReferences(object value, IValueSerializerContext context)
	{
		return Empty;
	}

	public static ValueSerializer GetSerializerFor(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		object obj = _valueSerializers[type];
		if (obj != null)
		{
			if (obj != _valueSerializersLock)
			{
				return obj as ValueSerializer;
			}
			return null;
		}
		AttributeCollection attributes = TypeDescriptor.GetAttributes(type);
		ValueSerializerAttribute valueSerializerAttribute = attributes[typeof(ValueSerializerAttribute)] as ValueSerializerAttribute;
		ValueSerializer valueSerializer = null;
		if (valueSerializerAttribute != null)
		{
			valueSerializer = (ValueSerializer)Activator.CreateInstance(valueSerializerAttribute.ValueSerializerType);
		}
		if (valueSerializer == null)
		{
			if (type == typeof(string))
			{
				valueSerializer = new StringValueSerializer();
			}
			else
			{
				TypeConverter typeConverter = TypeConverterHelper.GetTypeConverter(type);
				if (typeConverter.GetType() == typeof(DateTimeConverter2))
				{
					valueSerializer = new DateTimeValueSerializer();
				}
				else if (typeConverter.CanConvertTo(typeof(string)) && typeConverter.CanConvertFrom(typeof(string)) && !(typeConverter is ReferenceConverter))
				{
					valueSerializer = new TypeConverterValueSerializer(typeConverter);
				}
			}
		}
		lock (_valueSerializersLock)
		{
			_valueSerializers[type] = ((valueSerializer == null) ? _valueSerializersLock : valueSerializer);
			return valueSerializer;
		}
	}

	public static ValueSerializer GetSerializerFor(PropertyDescriptor descriptor)
	{
		if (descriptor == null)
		{
			throw new ArgumentNullException("descriptor");
		}
		ValueSerializer valueSerializer;
		if (descriptor.Attributes[typeof(ValueSerializerAttribute)] is ValueSerializerAttribute valueSerializerAttribute)
		{
			valueSerializer = (ValueSerializer)Activator.CreateInstance(valueSerializerAttribute.ValueSerializerType);
		}
		else
		{
			valueSerializer = GetSerializerFor(descriptor.PropertyType);
			if (valueSerializer == null || valueSerializer is TypeConverterValueSerializer)
			{
				TypeConverter converter = descriptor.Converter;
				if (converter != null && converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)) && !(converter is ReferenceConverter))
				{
					valueSerializer = new TypeConverterValueSerializer(converter);
				}
			}
		}
		return valueSerializer;
	}

	public static ValueSerializer GetSerializerFor(Type type, IValueSerializerContext context)
	{
		if (context != null)
		{
			ValueSerializer valueSerializerFor = context.GetValueSerializerFor(type);
			if (valueSerializerFor != null)
			{
				return valueSerializerFor;
			}
		}
		return GetSerializerFor(type);
	}

	public static ValueSerializer GetSerializerFor(PropertyDescriptor descriptor, IValueSerializerContext context)
	{
		if (context != null)
		{
			ValueSerializer valueSerializerFor = context.GetValueSerializerFor(descriptor);
			if (valueSerializerFor != null)
			{
				return valueSerializerFor;
			}
		}
		return GetSerializerFor(descriptor);
	}

	protected Exception GetConvertToException(object value, Type destinationType)
	{
		string text = ((value != null) ? value.GetType().FullName : SR.Get("ToStringNull"));
		return new NotSupportedException(SR.Get("ConvertToException", GetType().Name, text, destinationType.FullName));
	}

	protected Exception GetConvertFromException(object value)
	{
		string text = ((value != null) ? value.GetType().FullName : SR.Get("ToStringNull"));
		return new NotSupportedException(SR.Get("ConvertFromException", GetType().Name, text));
	}

	private static void TypeDescriptorRefreshed(RefreshEventArgs args)
	{
		_valueSerializers = new Hashtable();
	}

	static ValueSerializer()
	{
		Empty = new List<Type>();
		_valueSerializersLock = new object();
		_valueSerializers = new Hashtable();
		TypeDescriptor.Refreshed += TypeDescriptorRefreshed;
	}
}
