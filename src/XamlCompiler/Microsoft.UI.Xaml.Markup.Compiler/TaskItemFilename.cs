using System;
using System.IO;
using Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class TaskItemFilename
{
	private bool _outputFileIsZeroLength;

	private bool _xbfFileIsZeroLength;

	private SourceFileManager _srcMgr;

	public string SourceXamlFullPath { get; private set; }

	public string XamlGivenPath { get; private set; }

	public string TargetFolder { get; private set; }

	public string FileNameNoExtension { get; private set; }

	public string XamlOutputFilename { get; private set; }

	public string XbfOutputFilename { get; private set; }

	public string RelativePathFromGeneratedCodeToXamlFile { get; private set; }

	public string ApparentRelativePath { get; private set; }

	public DateTime XamlOutputChangeTime { get; private set; }

	public DateTime XbfOutputChangeTime { get; private set; }

	public long XamlFileTimeAtLastCompile { get; set; }

	public DateTime XamlLastChangeTime { get; set; }

	public string GeneratedCodePathPrefix { get; set; }

	public bool IsApplication { get; private set; }

	public bool IsSdkXamlFile { get; private set; }

	public string TargetPathMetadata { get; set; }

	public string ClassFullName { get; set; }

	public bool IsForcedOutOfDate { get; set; }

	public string XamlResourceMapName { get; private set; }

	public string XamlComponentResourceLocation { get; private set; }

	public TaskItemFilename(IFileItem item, SourceFileManager srcMgr, bool isApplication, bool isSdkXaml)
	{
		IsApplication = isApplication;
		IsSdkXamlFile = isSdkXaml;
		_srcMgr = srcMgr;
		XamlResourceMapName = item.MSBuild_XamlResourceMapName;
		XamlComponentResourceLocation = item.MSBuild_XamlComponentResourceLocation;
		string text = item.MSBuild_Link;
		if (string.IsNullOrEmpty(text))
		{
			string fullPath = item.FullPath;
			text = CompileXamlInternal.GetDefaultXamlLinkMetadata(fullPath, item.ItemSpec, _srcMgr.ProjectFolderFullpath, _srcMgr.IncludeFolderList);
		}
		if (string.IsNullOrEmpty(text) || Path.IsPathRooted(text) || text.Contains("..\\"))
		{
			if (Path.IsPathRooted(item.ItemSpec) || item.ItemSpec.Contains("..\\"))
			{
				ApparentRelativePath = Path.GetFileName(item.ItemSpec);
			}
			else
			{
				ApparentRelativePath = item.ItemSpec;
			}
		}
		else
		{
			ApparentRelativePath = text;
		}
		TargetPathMetadata = item.MSBuild_TargetPath;
		XamlGivenPath = item.ItemSpec;
		SourceXamlFullPath = item.ItemSpec;
		if (!Path.IsPathRooted(SourceXamlFullPath))
		{
			SourceXamlFullPath = Path.Combine(srcMgr.ProjectFolderFullpath, item.ItemSpec);
		}
		SourceXamlFullPath = Path.GetFullPath(SourceXamlFullPath);
		string path = (isSdkXaml ? TargetPathMetadata : ApparentRelativePath);
		TargetFolder = Path.Combine(srcMgr.OutputFolderFullpath, Path.GetDirectoryName(path));
		FileNameNoExtension = Path.GetFileNameWithoutExtension(SourceXamlFullPath);
		XamlOutputFilename = Path.Combine(TargetFolder, FileNameNoExtension + ".xaml");
		XbfOutputFilename = Path.Combine(TargetFolder, FileNameNoExtension + ".xbf");
		if (_srcMgr.SavedState.XamlPerFileInfo.TryGetValue(XamlGivenPath, out var value))
		{
			GeneratedCodePathPrefix = value.GeneratedCodeFilePathPrefix;
		}
		RelativePathFromGeneratedCodeToXamlFile = FileHelpers.GetRelativePath(Path.GetDirectoryName(TargetFolder), ApparentRelativePath);
		Refresh(_srcMgr.SavedState);
	}

	public void Refresh(SavedStateManager saveState)
	{
		XamlOutputChangeTime = File.GetLastWriteTime(XamlOutputFilename);
		if (!_srcMgr.XbfGenerationIsDisabled)
		{
			XbfOutputChangeTime = File.GetLastWriteTime(XbfOutputFilename);
		}
		XamlFileTimeAtLastCompile = saveState.GetXamlFileTimeAtLastCompile(XamlGivenPath);
		_outputFileIsZeroLength = true;
		string text = Path.Combine(TargetFolder, FileNameNoExtension + _srcMgr.GeneratedFileExtension);
		if (!string.IsNullOrEmpty(GeneratedCodePathPrefix))
		{
			text = GeneratedCodePathPrefix + _srcMgr.GeneratedFileExtension;
		}
		if (File.Exists(text))
		{
			FileInfo fileInfo = new FileInfo(text);
			_outputFileIsZeroLength = fileInfo.Length == 0;
		}
		_xbfFileIsZeroLength = true;
		if (File.Exists(XbfOutputFilename))
		{
			FileInfo fileInfo2 = new FileInfo(XbfOutputFilename);
			_xbfFileIsZeroLength = fileInfo2.Length == 0;
		}
		IsForcedOutOfDate = false;
	}

	public bool OutOfDate()
	{
		if (_outputFileIsZeroLength)
		{
			return true;
		}
		if (IsForcedOutOfDate)
		{
			return true;
		}
		if (_srcMgr.IsPass1)
		{
			if (XamlLastChangeTime.Ticks != XamlFileTimeAtLastCompile)
			{
				return true;
			}
		}
		else
		{
			if (XamlLastChangeTime > XamlOutputChangeTime)
			{
				return true;
			}
			if (!_srcMgr.XbfGenerationIsDisabled && (_xbfFileIsZeroLength || XamlLastChangeTime > XbfOutputChangeTime))
			{
				return true;
			}
		}
		return false;
	}
}
