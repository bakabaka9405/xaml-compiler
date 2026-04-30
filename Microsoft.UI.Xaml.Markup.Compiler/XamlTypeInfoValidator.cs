using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlTypeInfoValidator
{
	private DirectUISchemaContext schema;

	private List<XamlCompileError> errors = new List<XamlCompileError>();

	private List<XamlCompileWarning> warnings = new List<XamlCompileWarning>();

	public IEnumerable<XamlCompileError> Errors => errors;

	public IEnumerable<XamlCompileWarning> Warnings => warnings;

	public XamlTypeInfoValidator(DirectUISchemaContext duiSchema)
	{
		schema = duiSchema;
	}

	public void Validate(IEnumerable<InternalTypeEntry> typeTable)
	{
		ValidateCreateFromStringMethods(typeTable);
	}

	private void ValidateCreateFromStringMethods(IEnumerable<InternalTypeEntry> typeTable)
	{
		foreach (InternalTypeEntry item in typeTable.Where((InternalTypeEntry t) => t.UserTypeInfo != null && t.UserTypeInfo.CreateFromStringMethod.Exists))
		{
			ValidateCreateFromStringMethod(item);
		}
	}

	private void ValidateCreateFromStringMethod(InternalTypeEntry declaringType)
	{
		XamlCompileError xamlCompileError = schema.EnsureCreateFromStringResolved(declaringType.Name, declaringType.UserTypeInfo.CreateFromStringMethod, null);
		if (xamlCompileError != null)
		{
			errors.Add(xamlCompileError);
		}
	}
}
