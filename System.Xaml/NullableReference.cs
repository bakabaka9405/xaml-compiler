using System.Threading;

namespace System.Xaml;

internal struct NullableReference<T> where T : class
{
	private static object s_NullSentinel = new object();

	private static object s_NotPresentSentinel = new object();

	private object _value;

	public bool IsNotPresent
	{
		get
		{
			return _value == s_NotPresentSentinel;
		}
		set
		{
			_value = (value ? s_NotPresentSentinel : null);
		}
	}

	public bool IsSet => _value != null;

	public bool IsSetVolatile
	{
		get
		{
			object obj = Thread.VolatileRead(ref _value);
			return obj != null;
		}
	}

	public T Value
	{
		get
		{
			object value = _value;
			if (value != s_NullSentinel)
			{
				return (T)value;
			}
			return null;
		}
		set
		{
			_value = ((value == null) ? s_NullSentinel : value);
		}
	}

	public void SetIfNull(T value)
	{
		object value2 = ((value == null) ? s_NullSentinel : value);
		Interlocked.CompareExchange(ref _value, value2, null);
	}

	public void SetVolatile(T value)
	{
		object value2 = ((value == null) ? s_NullSentinel : value);
		Thread.VolatileWrite(ref _value, value2);
	}
}
