using Microsoft.Win32;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal sealed class ExperimentalFeatures
{
	private const string RegistryKey_ExperimentalFeatures = "SOFTWARE\\Microsoft\\WinUI\\XAML\\XamlCompiler";

	private static int GetIntValue(string key, int defaultValue)
	{
		int result = defaultValue;
		object value = GetValue(key);
		if (value != null)
		{
			int.TryParse(value.ToString(), out result);
		}
		return result;
	}

	private static object GetValue(string keyName)
	{
		using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\WinUI\\XAML\\XamlCompiler", writable: false);
		return registryKey?.GetValue(keyName);
	}
}
