using System;
using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler.Core;

public class InstanceCache<TKey, TValue> : Dictionary<TKey, TValue>
{
	public InstanceCache()
	{
		InstanceCacheManager.Register(delegate
		{
			Clear();
		});
	}

	public InstanceCache(Action clearAction)
	{
		InstanceCacheManager.Register(clearAction);
	}
}
