using System;
using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler.Core;

public class InstanceCacheManager
{
	private static List<Action> clearActions = new List<Action>();

	public static void Register(Action action)
	{
		clearActions.Add(action);
	}

	public static void ClearCache()
	{
		foreach (Action clearAction in clearActions)
		{
			clearAction();
		}
	}
}
