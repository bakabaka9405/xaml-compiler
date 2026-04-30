using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.UI.Xaml.Markup.Compiler.Core;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler.Utilities;

internal static class FileHelpers
{
	private static readonly char[] DirectorySeparators = new char[2]
	{
		Path.DirectorySeparatorChar,
		Path.AltDirectorySeparatorChar
	};

	private static InstanceCache<string, string> _platformAssemblyNameCache = new InstanceCache<string, string>();

	public static void BackupFile(string fullPathFileName)
	{
		string text = fullPathFileName + ".backup";
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		FileInfo fileInfo = new FileInfo(fullPathFileName);
		DateTime lastWriteTime = fileInfo.LastWriteTime;
		File.Move(fullPathFileName, text);
		File.SetLastWriteTime(text, lastWriteTime);
	}

	public static bool RestoreBackupFile(string fullPathFileName)
	{
		string text = fullPathFileName + ".backup";
		if (File.Exists(text))
		{
			if (File.Exists(fullPathFileName))
			{
				File.Delete(fullPathFileName);
			}
			FileInfo fileInfo = new FileInfo(text);
			DateTime lastWriteTime = fileInfo.LastWriteTime;
			File.Move(text, fullPathFileName);
			File.SetLastWriteTime(fullPathFileName, lastWriteTime);
			return true;
		}
		return false;
	}

	public static void BackupIfExistsAndTruncateToNull(string filename)
	{
		FileInfo fileInfo = new FileInfo(filename);
		if (fileInfo.Exists && fileInfo.Length > 0)
		{
			BackupFile(filename);
		}
		DateTime lastWriteTime = fileInfo.LastWriteTime;
		using (File.Create(filename))
		{
		}
		File.SetLastWriteTime(filename, lastWriteTime);
	}

	public static string GetSafeName(string ProjectName)
	{
		if (ProjectName == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder(ProjectName);
		for (int i = 0; i < stringBuilder.Length; i++)
		{
			UnicodeCategory unicodeCategory = char.GetUnicodeCategory(stringBuilder[i]);
			bool flag = unicodeCategory == UnicodeCategory.UppercaseLetter || unicodeCategory == UnicodeCategory.LowercaseLetter || unicodeCategory == UnicodeCategory.TitlecaseLetter || unicodeCategory == UnicodeCategory.OtherLetter || unicodeCategory == UnicodeCategory.LetterNumber || stringBuilder[i] == '_';
			bool flag2 = unicodeCategory == UnicodeCategory.NonSpacingMark || unicodeCategory == UnicodeCategory.SpacingCombiningMark || unicodeCategory == UnicodeCategory.ModifierLetter || unicodeCategory == UnicodeCategory.DecimalDigitNumber;
			if (i == 0)
			{
				if (!flag)
				{
					stringBuilder[i] = '_';
				}
			}
			else if (!(flag || flag2))
			{
				stringBuilder[i] = '_';
			}
		}
		return stringBuilder.ToString();
	}

	public static string GetRelativePath(string currentDir, string filePath)
	{
		string[] array = Path.GetFullPath(currentDir).Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
		string fileName = Path.GetFileName(filePath);
		string directoryName = Path.GetDirectoryName(Path.GetFullPath(filePath));
		string[] array2 = directoryName.Split(DirectorySeparators);
		if (array[0] != array2[0])
		{
			return Path.GetFullPath(filePath);
		}
		int num = Math.Min(array.Length, array2.Length);
		int i;
		for (i = 0; i < num && !(array[i] != array2[i]); i++)
		{
		}
		List<string> list = new List<string>();
		if (array.Length > i)
		{
			for (int j = i; j < array.Length; j++)
			{
				list.Add("..");
			}
		}
		if (array2.Length > i)
		{
			for (int k = i; k < array2.Length; k++)
			{
				list.Add(array2[k]);
			}
		}
		list.Add(fileName);
		return Path.Combine(list.ToArray());
	}

	public static string ComputeBaseFolder(string folder1, string folder2)
	{
		if (folder2.StartsWith(folder1))
		{
			return folder1;
		}
		if (folder1.StartsWith(folder2))
		{
			return folder2;
		}
		return ComputeBaseFolder(Path.GetDirectoryName(folder1), Path.GetDirectoryName(folder2));
	}

	public static bool IsPlatformAssembly(DirectUIAssembly assembly)
	{
		if (_platformAssemblyNameCache.TryGetValue("Windows.Foundation.HResult", out var value))
		{
			return value.Equals(assembly.GetName().FullName, StringComparison.OrdinalIgnoreCase);
		}
		if (assembly.GetType("Windows.Foundation.HResult") != null)
		{
			_platformAssemblyNameCache["Windows.Foundation.HResult"] = assembly.GetName().FullName;
			return true;
		}
		return false;
	}

	public static bool IsWinUIAssembly(DirectUIAssembly assembly)
	{
		if (_platformAssemblyNameCache.TryGetValue("Microsoft.UI.Xaml.DependencyObject", out var value))
		{
			return value.Equals(assembly.GetName().FullName, StringComparison.OrdinalIgnoreCase);
		}
		if (assembly.GetType("Microsoft.UI.Xaml.DependencyObject") != null)
		{
			_platformAssemblyNameCache["Microsoft.UI.Xaml.DependencyObject"] = assembly.GetName().FullName;
			return true;
		}
		return false;
	}

	public static bool IsFacadeWinmd(Assembly asm, string windowsSdkPath)
	{
		if (string.IsNullOrWhiteSpace(windowsSdkPath))
		{
			return false;
		}
		bool? flag = null;
		try
		{
			flag = (asm as DirectUIAssembly)?.Location?.StartsWith(Path.Combine(windowsSdkPath, "UnionMetadata\\facade\\Windows.winmd"), StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
		}
		if (flag.HasValue)
		{
			return flag.Value;
		}
		return false;
	}
}
