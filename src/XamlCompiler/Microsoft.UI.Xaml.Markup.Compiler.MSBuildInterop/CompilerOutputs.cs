using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;

public sealed class CompilerOutputs
{
	public IList<string> GeneratedCodeFiles { get; set; }

	public IList<string> GeneratedXamlFiles { get; set; }

	public IList<string> GeneratedXbfFiles { get; set; }

	public IList<string> GeneratedXamlPagesFiles { get; set; }

	public IList<MSBuildLogEntry> MSBuildLogEntries { get; set; }
}
