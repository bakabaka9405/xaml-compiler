using System.Collections.Generic;
using System.IO;

namespace Microsoft.UI.Xaml.Markup.Compiler.Utilities;

internal class XamlHelper
{
	internal static List<string> SplitAndEnsureFullpaths(string path, string projectFolderFullpath)
	{
		if (path == null)
		{
			return new List<string>();
		}
		return EnsureFullpaths(path.Split(';'), projectFolderFullpath);
	}

	internal static List<string> EnsureFullpaths(string[] paths, string projectFolderFullpath)
	{
		List<string> list = new List<string>();
		if (paths != null)
		{
			foreach (string path in paths)
			{
				string item = EnsureFullpath(path, projectFolderFullpath);
				list.Add(item);
			}
		}
		return list;
	}

	private static string EnsureFullpath(string path, string projectFolderFullpath)
	{
		string path2 = path;
		if (!Path.IsPathRooted(path2))
		{
			path2 = Path.Combine(projectFolderFullpath, path);
		}
		return Path.GetFullPath(path2);
	}
}
