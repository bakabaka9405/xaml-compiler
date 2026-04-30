using System.Threading;

namespace System.Xaml;

internal class GCNotificationToken
{
	private WaitCallback callback;

	private object state;

	private GCNotificationToken(WaitCallback callback, object state)
	{
		this.callback = callback;
		this.state = state;
	}

	~GCNotificationToken()
	{
		ThreadPool.QueueUserWorkItem(callback, state);
	}

	internal static void RegisterCallback(WaitCallback callback, object state)
	{
		new GCNotificationToken(callback, state);
	}
}
