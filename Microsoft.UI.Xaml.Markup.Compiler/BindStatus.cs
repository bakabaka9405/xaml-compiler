using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

[Flags]
public enum BindStatus
{
	None = 0,
	HasBinding = 1,
	TracksSource = 2,
	TracksTarget = 4,
	HasFallbackValue = 8,
	HasTargetNullValue = 0x10,
	HasConverter = 0x20,
	HasEventBinding = 0x40
}
