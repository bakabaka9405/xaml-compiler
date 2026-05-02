using System;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal static class XamlSourceInfoFixer
{
	public static string ReadMarkup(SourcePos start, SourcePos end, string[] xamlLines)
	{
		int num = start.Row - 1;
		int num2 = end.Row - 1;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = num; i <= num2; i++)
		{
			int num3 = 0;
			int num4 = xamlLines[i].Length - 1;
			bool flag = false;
			if (i == num)
			{
				flag = true;
				num3 = start.Col - 1;
			}
			if (i == num2)
			{
				flag = true;
				num4 = end.Col - 1;
			}
			string text = null;
			text = (flag ? xamlLines[i].Substring(num3, num4 - num3 + 1) : xamlLines[i]);
			stringBuilder.AppendLine(text);
		}
		return stringBuilder.ToString();
	}

	public static FixedSourceInfo GetFixedSourceInfo(ILineNumberAndErrorInfo element, string[] xamlLines)
	{
		LineNumberInfo lineNumberInfo = element.LineNumberInfo;
		int num = lineNumberInfo.StartLineNumber - 1;
		int num2 = lineNumberInfo.StartLinePosition - 2;
		int num3 = lineNumberInfo.EndLineNumber - 1;
		int num4 = lineNumberInfo.EndLinePosition - 1;
		bool flag = false;
		bool flag2 = false;
		FixedSourceInfo fixedSourceInfo = new FixedSourceInfo();
		fixedSourceInfo.StartOpeningTag.Row = lineNumberInfo.StartLineNumber;
		fixedSourceInfo.StartOpeningTag.Col = lineNumberInfo.StartLinePosition - 1;
		bool flag3 = false;
		for (int i = num; i <= num3; i++)
		{
			int num5 = 0;
			int num6 = xamlLines[i].Length - 1;
			string text = xamlLines[i];
			if (i == num)
			{
				num5 = num2;
			}
			if (i == num3)
			{
				num6 = num4;
			}
			if (!flag3)
			{
				int num7 = text.IndexOf('>', num5);
				if (num7 != -1)
				{
					flag3 = true;
					fixedSourceInfo.StartClosingTag.Row = i + 1;
					fixedSourceInfo.StartClosingTag.Col = num7 + 1;
				}
			}
			if (!flag)
			{
				int num8 = num5;
				if (i == num)
				{
					num8++;
				}
				int num9 = text.IndexOf('<', num8);
				int num10 = text.IndexOf("/>", StringComparison.OrdinalIgnoreCase);
				if (num9 != -1)
				{
					flag = true;
					flag2 = num10 != -1 && num10 < num9;
					break;
				}
				if (num10 != -1)
				{
					flag = true;
					flag2 = true;
					break;
				}
			}
		}
		fixedSourceInfo.SelfClosing = flag2;
		if (flag2)
		{
			fixedSourceInfo.EndOpeningTag = fixedSourceInfo.StartOpeningTag;
			fixedSourceInfo.EndClosingTag = fixedSourceInfo.StartClosingTag;
		}
		else
		{
			fixedSourceInfo.EndOpeningTag.Row = lineNumberInfo.EndLineNumber;
			fixedSourceInfo.EndOpeningTag.Col = lineNumberInfo.EndLinePosition - 2;
			fixedSourceInfo.EndClosingTag.Row = lineNumberInfo.EndLineNumber;
			fixedSourceInfo.EndClosingTag.Col = xamlLines[fixedSourceInfo.EndClosingTag.Row - 1].IndexOf('>', fixedSourceInfo.EndOpeningTag.Col - 1) + 1;
		}
		return fixedSourceInfo;
	}
}
