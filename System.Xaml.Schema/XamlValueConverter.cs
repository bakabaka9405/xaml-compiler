using System.ComponentModel;
using System.Threading;

namespace System.Xaml.Schema;

public class XamlValueConverter<TConverterBase> : IEquatable<XamlValueConverter<TConverterBase>> where TConverterBase : class
{
	private TConverterBase _instance;

	private ThreeValuedBool _isPublic;

	private volatile bool _instanceIsSet;

	public string Name { get; private set; }

	public Type ConverterType { get; private set; }

	public XamlType TargetType { get; private set; }

	public TConverterBase ConverterInstance
	{
		get
		{
			if (!_instanceIsSet)
			{
				Interlocked.CompareExchange(ref _instance, CreateInstance(), null);
				_instanceIsSet = true;
			}
			return _instance;
		}
	}

	internal virtual bool IsPublic
	{
		get
		{
			if (_isPublic == ThreeValuedBool.NotSet)
			{
				_isPublic = ((!(ConverterType == null) && !ConverterType.IsVisible) ? ThreeValuedBool.False : ThreeValuedBool.True);
			}
			return _isPublic == ThreeValuedBool.True;
		}
	}

	public XamlValueConverter(Type converterType, XamlType targetType)
		: this(converterType, targetType, (string)null)
	{
	}

	public XamlValueConverter(Type converterType, XamlType targetType, string name)
	{
		if (converterType == null && targetType == null && name == null)
		{
			throw new ArgumentException(SR.Get("ArgumentRequired", "converterType, targetType, name"));
		}
		ConverterType = converterType;
		TargetType = targetType;
		Name = name ?? GetDefaultName();
	}

	public override string ToString()
	{
		return Name;
	}

	protected virtual TConverterBase CreateInstance()
	{
		if (ConverterType == typeof(EnumConverter) && TargetType.UnderlyingType != null && TargetType.UnderlyingType.IsEnum)
		{
			return (TConverterBase)(object)new EnumConverter(TargetType.UnderlyingType);
		}
		if (ConverterType != null)
		{
			if (!typeof(TConverterBase).IsAssignableFrom(ConverterType))
			{
				throw new XamlSchemaException(SR.Get("ConverterMustDeriveFromBase", ConverterType, typeof(TConverterBase)));
			}
			return (TConverterBase)SafeReflectionInvoker.CreateInstance(ConverterType, null);
		}
		return null;
	}

	private string GetDefaultName()
	{
		if (ConverterType != null)
		{
			if (TargetType != null)
			{
				return ConverterType.Name + "(" + TargetType.Name + ")";
			}
			return ConverterType.Name;
		}
		return TargetType.Name;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is XamlValueConverter<TConverterBase> xamlValueConverter))
		{
			return false;
		}
		return this == xamlValueConverter;
	}

	public override int GetHashCode()
	{
		int num = Name.GetHashCode();
		if (ConverterType != null)
		{
			num ^= ConverterType.GetHashCode();
		}
		if (TargetType != null)
		{
			num ^= TargetType.GetHashCode();
		}
		return num;
	}

	public bool Equals(XamlValueConverter<TConverterBase> other)
	{
		return this == other;
	}

	public static bool operator ==(XamlValueConverter<TConverterBase> converter1, XamlValueConverter<TConverterBase> converter2)
	{
		if ((object)converter1 == null)
		{
			return (object)converter2 == null;
		}
		if ((object)converter2 == null)
		{
			return false;
		}
		if (converter1.ConverterType == converter2.ConverterType && converter1.TargetType == converter2.TargetType)
		{
			return converter1.Name == converter2.Name;
		}
		return false;
	}

	public static bool operator !=(XamlValueConverter<TConverterBase> converter1, XamlValueConverter<TConverterBase> converter2)
	{
		return !(converter1 == converter2);
	}
}
