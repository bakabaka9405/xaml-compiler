using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlFileCodeInfo : IXamlFileCodeInfo
{
	private List<ConnectionIdElement> connectionIdElements = new List<ConnectionIdElement>();

	private List<DataTypeAssignment> dataTypeAssignments = new List<DataTypeAssignment>();

	private List<StrippableMember> strippableMembers = new List<StrippableMember>();

	private List<StrippableObject> strippableObjects = new List<StrippableObject>();

	private List<StrippableNamespace> strippableNamespaces = new List<StrippableNamespace>();

	private List<StrippableMember> suppressTrimWarningsBindingMembers = new List<StrippableMember>();

	public string SourceXamlGivenPath { get; set; }

	public string XamlOutputFilename { get; set; }

	public bool HasEventAssignments { get; set; }

	public BindStatus BindStatus { get; set; }

	public xPropertyInfo XPropertyInfo { get; set; }

	public List<ConnectionIdElement> ConnectionIdElements => connectionIdElements;

	public List<DataTypeAssignment> DataTypeAssignments => dataTypeAssignments;

	public List<StrippableMember> StrippableMembers => strippableMembers;

	public List<StrippableObject> StrippableObjects => strippableObjects;

	public List<StrippableNamespace> StrippableNamespaces => strippableNamespaces;

	public List<StrippableMember> SuppressXamlTrimWarningsBindingMembers => suppressTrimWarningsBindingMembers;

	public string FullPathToXamlFile { get; set; }

	public string ApparentRelativePath { get; set; }

	public string RelativePathFromGeneratedCodeToXamlFile { get; set; }

	public bool HasPhaseAssignments { get; set; }

	public XamlFileCodeInfo()
	{
		HasEventAssignments = false;
		BindStatus = BindStatus.None;
	}
}
