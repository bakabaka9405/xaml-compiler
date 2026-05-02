using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

/// <summary>
/// 官方 System.Xaml.dll 中不存在的 WinUI 自定义 XAML 指令。
/// 原反编译代码中该指令定义于 System.Xaml.XamlLanguage 上。
/// </summary>
internal static class SuppressXamlTrimWarningsDirective
{
	public static readonly XamlDirective Value = new XamlDirective(
		"http://schemas.microsoft.com/winfx/2006/xaml",
		"SuppressXamlTrimWarnings");
}
