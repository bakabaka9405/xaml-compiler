using System;
using System.Security;
using MS.Internal.WindowsBase;

namespace MS.Internal.Xaml;

[Serializable]
[FriendAccessAllowed]
internal struct SecurityCriticalDataForSet<T>
{
	[SecurityCritical]
	private T _value;

	internal T Value
	{
		[SecurityCritical]
		[SecuritySafeCritical]
		get
		{
			return _value;
		}
		[SecurityCritical]
		set
		{
			_value = value;
		}
	}

	[SecurityCritical]
	internal SecurityCriticalDataForSet(T value)
	{
		_value = value;
	}
}
