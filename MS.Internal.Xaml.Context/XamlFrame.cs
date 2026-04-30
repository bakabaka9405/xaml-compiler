using System;

namespace MS.Internal.Xaml.Context;

internal abstract class XamlFrame
{
	private int _depth;

	private XamlFrame _previous;

	public int Depth => _depth;

	public XamlFrame Previous
	{
		get
		{
			return _previous;
		}
		set
		{
			_previous = value;
			_depth = ((_previous != null) ? (_previous._depth + 1) : 0);
		}
	}

	protected XamlFrame()
	{
		_depth = -1;
	}

	protected XamlFrame(XamlFrame source)
	{
		_depth = source._depth;
	}

	public virtual XamlFrame Clone()
	{
		throw new NotImplementedException();
	}

	public abstract void Reset();
}
