using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal interface IXamlFileCodeInfo
{
	string ApparentRelativePath { get; set; }

	BindStatus BindStatus { get; set; }

	List<ConnectionIdElement> ConnectionIdElements { get; }

	string FullPathToXamlFile { get; set; }

	bool HasEventAssignments { get; set; }

	string RelativePathFromGeneratedCodeToXamlFile { get; set; }

	string SourceXamlGivenPath { get; set; }

	List<DataTypeAssignment> DataTypeAssignments { get; }

	bool HasPhaseAssignments { get; set; }

	List<StrippableMember> StrippableMembers { get; }

	List<StrippableObject> StrippableObjects { get; }

	List<StrippableNamespace> StrippableNamespaces { get; }

	List<StrippableMember> SuppressXamlTrimWarningsBindingMembers { get; }

	xPropertyInfo XPropertyInfo { get; set; }
}
