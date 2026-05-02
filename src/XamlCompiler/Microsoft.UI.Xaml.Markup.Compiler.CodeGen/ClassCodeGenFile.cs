using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class ClassCodeGenFile
{
	private string _classFullName;

	private List<TaskItemFilename> _xamlTaskItems;

	public string TargetFolderFullPath { get; set; }

	public string BaseFileName { get; set; }

	public ReadOnlyCollection<TaskItemFilename> XamlTaskItems
	{
		get
		{
			EnsureXamlTaskItems();
			return _xamlTaskItems.AsReadOnly();
		}
	}

	public ClassCodeGenFile(string classFullName)
	{
		_classFullName = classFullName;
	}

	private void EnsureXamlTaskItems()
	{
		if (_xamlTaskItems == null)
		{
			_xamlTaskItems = new List<TaskItemFilename>();
		}
	}

	private void RecalculateBaseFileNameWithTif(TaskItemFilename tif)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tif.SourceXamlFullPath);
		if (string.IsNullOrEmpty(BaseFileName) || BaseFileName.Length > fileNameWithoutExtension.Length)
		{
			BaseFileName = fileNameWithoutExtension;
		}
	}

	private void ResetBaseFileName()
	{
		BaseFileName = null;
		foreach (TaskItemFilename xamlTaskItem in _xamlTaskItems)
		{
			RecalculateBaseFileNameWithTif(xamlTaskItem);
		}
	}

	public void AddTaskItem(TaskItemFilename tif)
	{
		EnsureXamlTaskItems();
		_xamlTaskItems.Add(tif);
		RecalculateBaseFileNameWithTif(tif);
	}

	public void RemoveTaskItem(TaskItemFilename tif)
	{
		_xamlTaskItems.Remove(tif);
		ResetBaseFileName();
	}
}
