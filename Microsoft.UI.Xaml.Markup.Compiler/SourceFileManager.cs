using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class SourceFileManager
{
	private Dictionary<string, ClassCodeGenFile> _classXamlFilesMap;

	public string ProjectFolderFullpath { get; private set; }

	public string OutputFolderFullpath { get; private set; }

	public List<string> IncludeFolderList { get; private set; }

	public bool IsPass1 { get; private set; }

	public bool XbfGenerationIsDisabled { get; private set; }

	public List<TaskItemFilename> ProjectXamlTaskItems { get; private set; }

	public List<TaskItemFilename> Sdk80XamlTaskItems { get; private set; }

	public List<TaskItemFilename> SdkNon80XamlTaskItems { get; private set; }

	public List<TaskItemFilename> SdkXamlTaskItems { get; private set; }

	public BuildTaskFileService TaskFileService { get; private set; }

	public SavedStateManager SavedState { get; private set; }

	public List<TaskItemFilename> ClasslessXamlFiles { get; private set; }

	public string GeneratedFileExtension { get; private set; }

	public IEnumerable<ClassCodeGenFile> CodeGenFiles
	{
		get
		{
			if (_classXamlFilesMap == null)
			{
				return new List<ClassCodeGenFile>();
			}
			return _classXamlFilesMap.Values;
		}
	}

	public SourceFileManager(CompileXamlInternal compiler)
	{
		IsPass1 = compiler.IsPass1;
		SavedState = compiler.SaveState;
		XbfGenerationIsDisabled = compiler.DisableXbfGeneration;
		ProjectFolderFullpath = compiler.ProjectFolderFullpath;
		OutputFolderFullpath = compiler.OutputFolderFullpath;
		IncludeFolderList = compiler.IncludeFolderList;
		TaskFileService = compiler.TaskFileService;
		GeneratedFileExtension = compiler.GeneratedExtension;
		ClasslessXamlFiles = new List<TaskItemFilename>();
		ProjectXamlTaskItems = new List<TaskItemFilename>();
		if (compiler.XamlApplications != null)
		{
			foreach (IFileItem xamlApplication in compiler.XamlApplications)
			{
				LoadTaskItem(ProjectXamlTaskItems, xamlApplication, isApplication: true, isSdkXaml: false);
			}
		}
		if (compiler.XamlPages != null)
		{
			foreach (IFileItem xamlPage in compiler.XamlPages)
			{
				LoadTaskItem(ProjectXamlTaskItems, xamlPage, isApplication: false, isSdkXaml: false);
			}
		}
		if (compiler.SdkXamlPages == null)
		{
			return;
		}
		Sdk80XamlTaskItems = new List<TaskItemFilename>();
		SdkNon80XamlTaskItems = new List<TaskItemFilename>();
		SdkXamlTaskItems = new List<TaskItemFilename>();
		foreach (IFileItem sdkXamlPage in compiler.SdkXamlPages)
		{
			TaskItemFilename item = LoadTaskItem(SdkXamlTaskItems, sdkXamlPage, isApplication: false, isSdkXaml: true);
			SdkNon80XamlTaskItems.Add(item);
		}
	}

	public void SaveState()
	{
		PrepareToSaveState();
		SavedState.Save();
	}

	private void PrepareToSaveState()
	{
		if (_classXamlFilesMap == null)
		{
			return;
		}
		foreach (ClassCodeGenFile value in _classXamlFilesMap.Values)
		{
			foreach (TaskItemFilename xamlTaskItem in value.XamlTaskItems)
			{
				SavedState.SetGeneratedCodeFilePathPrefix(xamlTaskItem.XamlGivenPath, Path.Combine(value.TargetFolderFullPath, value.BaseFileName));
			}
		}
	}

	public void PropagateOutOfDateStatus(DirectUISchemaContext context)
	{
		HashSet<string> hashSet = null;
		foreach (TaskItemFilename projectXamlTaskItem in ProjectXamlTaskItems)
		{
			if (!projectXamlTaskItem.OutOfDate())
			{
				continue;
			}
			if (hashSet == null)
			{
				hashSet = new HashSet<string>();
			}
			if (!string.IsNullOrEmpty(projectXamlTaskItem.ClassFullName))
			{
				hashSet.Add(projectXamlTaskItem.ClassFullName);
			}
			string text = null;
			using (TextReader fileStream = TaskFileService.GetFileContents(projectXamlTaskItem.SourceXamlFullPath))
			{
				text = XamlNodeStreamHelper.ReadXClassFromXamlFileStream(fileStream, context);
			}
			if (text != projectXamlTaskItem.ClassFullName)
			{
				UnregisterClassOfTaskItem(projectXamlTaskItem);
				projectXamlTaskItem.ClassFullName = text;
				RegisterClassOfTaskItem(projectXamlTaskItem);
				if (!string.IsNullOrEmpty(projectXamlTaskItem.ClassFullName))
				{
					hashSet.Add(projectXamlTaskItem.ClassFullName);
				}
			}
		}
		if (hashSet == null)
		{
			return;
		}
		foreach (string item in hashSet)
		{
			IReadOnlyCollection<TaskItemFilename> xamlTaskItems = _classXamlFilesMap[item].XamlTaskItems;
			if (xamlTaskItems.Count == 0)
			{
				_classXamlFilesMap.Remove(item);
				continue;
			}
			foreach (TaskItemFilename item2 in xamlTaskItems)
			{
				item2.IsForcedOutOfDate = true;
				if (File.Exists(item2.XamlOutputFilename))
				{
					File.Delete(item2.XamlOutputFilename);
				}
			}
		}
	}

	public TaskItemFilename FindTaskItemByFullPath(string fullPath)
	{
		foreach (TaskItemFilename projectXamlTaskItem in ProjectXamlTaskItems)
		{
			if (fullPath == projectXamlTaskItem.SourceXamlFullPath)
			{
				return projectXamlTaskItem;
			}
		}
		return null;
	}

	private DateTime GetSourceFileChanged(TaskItemFilename tif)
	{
		return TaskFileService.GetLastChangeTime(tif.SourceXamlFullPath);
	}

	private void AddToXamlClassFileMap(string classFullName, TaskItemFilename tif)
	{
		if (_classXamlFilesMap == null)
		{
			_classXamlFilesMap = new Dictionary<string, ClassCodeGenFile>();
		}
		if (!_classXamlFilesMap.TryGetValue(classFullName, out var value))
		{
			value = new ClassCodeGenFile(classFullName);
			value.TargetFolderFullPath = tif.TargetFolder;
			_classXamlFilesMap.Add(classFullName, value);
		}
		else
		{
			value.TargetFolderFullPath = FileHelpers.ComputeBaseFolder(value.TargetFolderFullPath, tif.TargetFolder);
		}
		value.AddTaskItem(tif);
	}

	private void RemoveFromXamlClassFileMap(string classFullName, TaskItemFilename tif)
	{
		ClassCodeGenFile classCodeGenFile = _classXamlFilesMap[tif.ClassFullName];
		classCodeGenFile.RemoveTaskItem(tif);
	}

	public string GetTargetFolderOfClass(string classFullName)
	{
		if (_classXamlFilesMap != null)
		{
			return null;
		}
		if (_classXamlFilesMap.TryGetValue(classFullName, out var value))
		{
			return value.TargetFolderFullPath;
		}
		return null;
	}

	private TaskItemFilename LoadTaskItem(List<TaskItemFilename> tifList, IFileItem item, bool isApplication, bool isSdkXaml)
	{
		TaskItemFilename taskItemFilename = new TaskItemFilename(item, this, isApplication, isSdkXaml);
		taskItemFilename.XamlLastChangeTime = GetSourceFileChanged(taskItemFilename);
		SavedState.LoadSavedTaskItemInfo(taskItemFilename);
		RegisterClassOfTaskItem(taskItemFilename);
		tifList.Add(taskItemFilename);
		return taskItemFilename;
	}

	private void RegisterClassOfTaskItem(TaskItemFilename tif)
	{
		if (string.IsNullOrEmpty(tif.ClassFullName))
		{
			ClasslessXamlFiles.Add(tif);
		}
		else
		{
			AddToXamlClassFileMap(tif.ClassFullName, tif);
		}
	}

	private void UnregisterClassOfTaskItem(TaskItemFilename tif)
	{
		if (string.IsNullOrEmpty(tif.ClassFullName))
		{
			ClasslessXamlFiles.Remove(tif);
		}
		else
		{
			RemoveFromXamlClassFileMap(tif.ClassFullName, tif);
		}
	}
}
