using System.Collections.Generic;

namespace System.Xaml;

public interface IXamlNameResolver
{
	bool IsFixupTokenAvailable { get; }

	event EventHandler OnNameScopeInitializationComplete;

	object Resolve(string name);

	object Resolve(string name, out bool isFullyInitialized);

	object GetFixupToken(IEnumerable<string> names);

	object GetFixupToken(IEnumerable<string> names, bool canAssignDirectly);

	IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope();
}
