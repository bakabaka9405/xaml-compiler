using System.Threading;

namespace Antlr4.Runtime.Sharpen;

public class AtomicReference<T> where T : class
{
	private volatile T _value;

	public AtomicReference()
	{
	}

	public AtomicReference(T value)
	{
		_value = value;
	}

	public T Get()
	{
		return _value;
	}

	public void Set(T value)
	{
		_value = value;
	}

	public bool CompareAndSet(T expect, T update)
	{
		return Interlocked.CompareExchange(ref _value, update, expect) == expect;
	}

	public T GetAndSet(T value)
	{
		return Interlocked.Exchange(ref _value, value);
	}
}
