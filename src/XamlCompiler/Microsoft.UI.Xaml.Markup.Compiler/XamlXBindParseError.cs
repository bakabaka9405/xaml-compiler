using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindParseError : XamlCompileError
{
	public XamlXBindParseError(IXamlDomNode node, CompiledBindingParseException ex)
		: base(ErrorCode.WMC1110, node.SourceFilePath, node.StartLineNumber, ex.StartCharacterPosition)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_XamlXBindParseError, ex.ExpressionBeingParsed, ex.Message);
	}

	public XamlXBindParseError(BindAssignmentBase bindAssignment, CompiledBindingParseException ex)
		: this(bindAssignment, ex.StartCharacterPosition, ex.ExpressionBeingParsed, ex.Message)
	{
	}

	public XamlXBindParseError(BindAssignmentBase bindAssignment, int startCharacterPosition, string expressionBeingParsed, string exceptionMessage)
		: base(ErrorCode.WMC1110, bindAssignment.ConnectionIdElement.ParentFileCodeInfo.FullPathToXamlFile, bindAssignment.LineNumberInfo.StartLineNumber, startCharacterPosition)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_XamlXBindParseError, expressionBeingParsed, exceptionMessage);
	}
}
