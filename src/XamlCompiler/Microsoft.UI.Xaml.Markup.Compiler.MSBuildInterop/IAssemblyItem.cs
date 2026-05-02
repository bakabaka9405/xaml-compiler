namespace Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;

public interface IAssemblyItem : IItemBase
{
	bool IsSystemReference { get; }

	bool IsNuGetReference { get; }

	bool IsStaticLibraryReference { get; }
}
