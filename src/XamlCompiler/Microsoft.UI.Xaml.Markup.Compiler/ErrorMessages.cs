using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.UI.Xaml.Markup.Compiler;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class ErrorMessages
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("Microsoft.UI.Xaml.Markup.Compiler.ErrorMessages", typeof(ErrorMessages).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string CastCannotStartWithAttachedProperty => ResourceManager.GetString("CastCannotStartWithAttachedProperty", resourceCulture);

	internal static string ExpectingDigit => ResourceManager.GetString("ExpectingDigit", resourceCulture);

	internal static string ExpectingMethod => ResourceManager.GetString("ExpectingMethod", resourceCulture);

	internal static string ExpectingStaticFunction => ResourceManager.GetString("ExpectingStaticFunction", resourceCulture);

	internal static string ExpectingStaticProperty => ResourceManager.GetString("ExpectingStaticProperty", resourceCulture);

	internal static string FunctionAsParameter => ResourceManager.GetString("FunctionAsParameter", resourceCulture);

	internal static string FunctionNotLeaf => ResourceManager.GetString("FunctionNotLeaf", resourceCulture);

	internal static string InvalidCast => ResourceManager.GetString("InvalidCast", resourceCulture);

	internal static string InvalidParameter => ResourceManager.GetString("InvalidParameter", resourceCulture);

	internal static string MissmatchedParameterCount => ResourceManager.GetString("MissmatchedParameterCount", resourceCulture);

	internal static string MultipleNamespaceConditionalStatements => ResourceManager.GetString("MultipleNamespaceConditionalStatements", resourceCulture);

	internal static string MultipleTargetPlatforms => ResourceManager.GetString("MultipleTargetPlatforms", resourceCulture);

	internal static string NoMatchingOverload => ResourceManager.GetString("NoMatchingOverload", resourceCulture);

	internal static string PropertyNotFound => ResourceManager.GetString("PropertyNotFound", resourceCulture);

	internal static string PropertyWithoutGet => ResourceManager.GetString("PropertyWithoutGet", resourceCulture);

	internal static string SyntaxError => ResourceManager.GetString("SyntaxError", resourceCulture);

	internal static string TypeNotFound => ResourceManager.GetString("TypeNotFound", resourceCulture);

	internal static string UnbindableMemberConflict => ResourceManager.GetString("UnbindableMemberConflict", resourceCulture);

	internal static string UnexpectedArrayIndexer => ResourceManager.GetString("UnexpectedArrayIndexer", resourceCulture);

	internal static string UnmatchedApiInformationParameters => ResourceManager.GetString("UnmatchedApiInformationParameters", resourceCulture);

	internal static string UnrecognizedApiInformation => ResourceManager.GetString("UnrecognizedApiInformation", resourceCulture);

	internal static string UnsuportedOutParameter => ResourceManager.GetString("UnsuportedOutParameter", resourceCulture);

	internal static string UsingNamedElement => ResourceManager.GetString("UsingNamedElement", resourceCulture);

	internal ErrorMessages()
	{
	}
}
