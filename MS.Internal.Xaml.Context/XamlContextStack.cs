using System;
using System.Globalization;
using System.Text;

namespace MS.Internal.Xaml.Context;

internal class XamlContextStack<T> where T : XamlFrame
{
	private int _depth = -1;

	private T _currentFrame;

	private T _recycledFrame;

	private Func<T> _creationDelegate;

	public T CurrentFrame => _currentFrame;

	public T PreviousFrame => (T)_currentFrame.Previous;

	public T PreviousPreviousFrame => (T)_currentFrame.Previous.Previous;

	public int Depth
	{
		get
		{
			return _depth;
		}
		set
		{
			_depth = value;
		}
	}

	public string Frames
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			T currentFrame = _currentFrame;
			stringBuilder.AppendLine("Stack: " + ((_currentFrame == null) ? (-1) : (_currentFrame.Depth + 1)).ToString(CultureInfo.InvariantCulture) + " frames");
			ShowFrame(stringBuilder, _currentFrame);
			return stringBuilder.ToString();
		}
	}

	public XamlContextStack(Func<T> creationDelegate)
	{
		_creationDelegate = creationDelegate;
		Grow();
		_depth = 0;
	}

	public XamlContextStack(XamlContextStack<T> source, bool copy)
	{
		_creationDelegate = source._creationDelegate;
		_depth = source.Depth;
		if (!copy)
		{
			_currentFrame = source.CurrentFrame;
			return;
		}
		T val = source.CurrentFrame;
		T val2 = null;
		while (val != null)
		{
			T val3 = (T)val.Clone();
			if (_currentFrame == null)
			{
				_currentFrame = val3;
			}
			if (val2 != null)
			{
				val2.Previous = val3;
			}
			val2 = val3;
			val = (T)val.Previous;
		}
	}

	private void Grow()
	{
		T currentFrame = _currentFrame;
		_currentFrame = _creationDelegate();
		_currentFrame.Previous = currentFrame;
	}

	public T GetFrame(int depth)
	{
		T val = _currentFrame;
		while (val.Depth > depth)
		{
			val = (T)val.Previous;
		}
		return val;
	}

	public void PushScope()
	{
		if (_recycledFrame == null)
		{
			Grow();
		}
		else
		{
			T currentFrame = _currentFrame;
			_currentFrame = _recycledFrame;
			_recycledFrame = (T)_recycledFrame.Previous;
			_currentFrame.Previous = currentFrame;
		}
		_depth++;
	}

	public void PopScope()
	{
		_depth--;
		T currentFrame = _currentFrame;
		_currentFrame = (T)_currentFrame.Previous;
		currentFrame.Previous = _recycledFrame;
		_recycledFrame = currentFrame;
		currentFrame.Reset();
	}

	public void Trim()
	{
		_recycledFrame = null;
	}

	private void ShowFrame(StringBuilder sb, T iteratorFrame)
	{
		if (iteratorFrame != null)
		{
			if (iteratorFrame.Previous != null)
			{
				ShowFrame(sb, (T)iteratorFrame.Previous);
			}
			sb.AppendLine("  " + iteratorFrame.Depth + " " + iteratorFrame.ToString());
		}
	}
}
