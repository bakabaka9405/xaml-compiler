using System.Collections.Generic;
using System.Reflection;
using System.Xaml;
using MS.Internal.Xaml.Parser;

namespace MS.Internal.Xaml.Context;

internal class XamlParserFrame : XamlCommonFrame
{
	public XamlType PreviousChildType { get; set; }

	public int CtorArgCount { get; set; }

	public bool ForcedToUseConstructor { get; set; }

	public bool InCollectionFromMember { get; set; }

	public bool InImplicitArray { get; set; }

	public bool InContainerDirective { get; set; }

	public string TypeNamespace { get; set; }

	public ParameterInfo[] LongestConstructorOfCurrentMarkupExtensionType { get; set; }

	public Dictionary<string, SpecialBracketCharacters> EscapeCharacterMapForMarkupExtension { get; set; }

	public BracketModeParseParameters BracketModeParseParameters { get; set; }

	public override void Reset()
	{
		base.Reset();
		PreviousChildType = null;
		CtorArgCount = 0;
		ForcedToUseConstructor = false;
		InCollectionFromMember = false;
		InImplicitArray = false;
		InContainerDirective = false;
		TypeNamespace = null;
		LongestConstructorOfCurrentMarkupExtensionType = null;
		EscapeCharacterMapForMarkupExtension = null;
		BracketModeParseParameters = null;
	}
}
