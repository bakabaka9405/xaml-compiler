using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.UI.Xaml.Markup.Compiler.Utilities;

internal static class ResourceUtilities
{
	private static readonly Regex msbuildMessageCodePattern = new Regex("^\\s*(?<CODE>MSB\\d\\d\\d\\d):\\s*(?<MESSAGE>.*)$", RegexOptions.Singleline);

	internal static string ExtractMessageCode(Regex messageCodePattern, string messageWithCode, out string code)
	{
		code = null;
		string result = messageWithCode;
		if (messageCodePattern == null)
		{
			messageCodePattern = msbuildMessageCodePattern;
		}
		Match match = messageCodePattern.Match(messageWithCode);
		if (match.Success)
		{
			code = match.Groups["CODE"].Value;
			result = match.Groups["MESSAGE"].Value;
		}
		return result;
	}

	internal static string FormatString(string unformatted, params object[] args)
	{
		string result = unformatted;
		if (args != null && args.Length != 0)
		{
			result = string.Format(CultureInfo.CurrentCulture, unformatted, args);
		}
		return result;
	}
}
