namespace Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;

public sealed class MSBuildItem : IFileItem, IItemBase, IAssemblyItem
{
	public string DependentUpon { get; set; }

	public string FullPath { get; set; }

	public string ItemSpec { get; set; }

	public bool IsSystemReference { get; set; }

	public bool IsNuGetReference { get; set; }

	public bool IsStaticLibraryReference { get; set; }

	public string MSBuild_Link { get; set; }

	public string MSBuild_TargetPath { get; set; }

	public string MSBuild_XamlResourceMapName { get; set; }

	public string MSBuild_XamlComponentResourceLocation { get; set; }
}
