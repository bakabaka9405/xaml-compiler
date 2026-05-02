using System.Collections.Generic;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal interface IXamlClassCodeInfo
{
	string BaseFileName { get; }

	XamlType BaseType { get; set; }

	string BaseTypeName { get; set; }

	BindStatus BindStatus { get; }

	List<BindUniverse> BindUniverses { get; }

	ClassName ClassName { get; }

	TypeForCodeGen ClassType { get; set; }

	XamlType ClassXamlType { get; set; }

	IEnumerable<FieldDefinition> FieldDeclarations { get; }

	bool HasEventAssignments { get; }

	bool HasFieldDefinitions { get; }

	bool IsApplication { get; }

	bool IsResourceDictionary { get; }

	List<XamlFileCodeInfo> PerXamlFileInfo { get; }

	string PriIndexName { get; set; }

	string RootNamespace { get; set; }

	string TargetFolder { get; set; }

	void AddXamlFileInfo(XamlFileCodeInfo fileCodeInfo);
}
