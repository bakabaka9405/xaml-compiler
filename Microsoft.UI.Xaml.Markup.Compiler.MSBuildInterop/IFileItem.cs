namespace Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;

public interface IFileItem : IItemBase
{
	string DependentUpon { get; }

	string FullPath { get; }

	string MSBuild_Link { get; }

	string MSBuild_TargetPath { get; }

	string MSBuild_XamlResourceMapName { get; }

	string MSBuild_XamlComponentResourceLocation { get; }
}
