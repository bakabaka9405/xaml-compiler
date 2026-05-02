using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class SavedStateManager
{
	private string _filename;

	private Queue<XamlFileCodeInfo> _fileCodeInfosToProcess = new Queue<XamlFileCodeInfo>();

	private const string XMLNAME_RootNode = "xml";

	private const string XMLNAME_XamlCompilerSaveState = "XamlCompilerSaveState";

	private const string XMLNAME_ReferenceAssemblyList = "ReferenceAssemblyList";

	private const string XMLNAME_LocalAssembly = "LocalAssembly";

	private const string XMLNAME_ReferenceAssembly = "ReferenceAssembly";

	private const string XMLNAME_PathName = "PathName";

	private const string XMLNAME_HashGuid = "HashGuid";

	private const string XMLNAME_XamlSourceFileDataList = "XamlSourceFileDataList";

	private const string XMLNAME_XamlSourceFileData = "XamlSourceFileData";

	private const string XMLNAME_XamlFeatureControlFlags = "XamlFeatureControlFlags";

	public string LocalAssemblyName { get; set; }

	public string XamlFeatureControlFlags { get; set; }

	public HashSet<string> ReferenceAssemblyList { get; private set; }

	public Dictionary<string, Guid> ReferenceAssemblyGuids { get; private set; }

	public Dictionary<string, SaveStatePerXamlFile> XamlPerFileInfo { get; private set; }

	public SavedStateManager()
	{
		LocalAssemblyName = string.Empty;
		ReferenceAssemblyList = new HashSet<string>();
		ReferenceAssemblyGuids = new Dictionary<string, Guid>();
		XamlPerFileInfo = new Dictionary<string, SaveStatePerXamlFile>(StringComparer.InvariantCultureIgnoreCase);
	}

	public static SavedStateManager Load(string fileName)
	{
		SavedStateManager savedStateManager = new SavedStateManager();
		try
		{
			savedStateManager.LoadFile(fileName);
		}
		catch (Exception)
		{
			savedStateManager = new SavedStateManager();
			savedStateManager._filename = fileName;
		}
		return savedStateManager;
	}

	public void Save()
	{
		SaveFile(_filename);
	}

	public void LoadSavedTaskItemInfo(TaskItemFilename tif)
	{
		if (!XamlPerFileInfo.TryGetValue(tif.XamlGivenPath, out var value))
		{
			tif.IsForcedOutOfDate = true;
			return;
		}
		tif.ClassFullName = value.ClassFullName;
		tif.XamlFileTimeAtLastCompile = value.XamlFileTimeAtLastCompile;
		if (value.XamlFileTimeAtLastCompile != tif.XamlLastChangeTime.Ticks)
		{
			tif.IsForcedOutOfDate = true;
		}
	}

	public void SetXamlFileTimeAtLastCompile(string fileName, long fileTime)
	{
		if (!XamlPerFileInfo.TryGetValue(fileName, out var value))
		{
			value = new SaveStatePerXamlFile(fileName);
			XamlPerFileInfo.Add(value.FileName, value);
		}
		value.XamlFileTimeAtLastCompile = fileTime;
	}

	public void SetClassFullName(string fileName, string classFullName)
	{
		if (!XamlPerFileInfo.TryGetValue(fileName, out var value))
		{
			value = new SaveStatePerXamlFile(fileName);
			XamlPerFileInfo.Add(value.FileName, value);
		}
		value.ClassFullName = classFullName;
	}

	public void SetGeneratedCodeFilePathPrefix(string fileName, string namePrefix)
	{
		if (!XamlPerFileInfo.TryGetValue(fileName, out var value))
		{
			value = new SaveStatePerXamlFile(fileName);
			XamlPerFileInfo.Add(value.FileName, value);
		}
		value.GeneratedCodeFilePathPrefix = namePrefix;
	}

	internal void AddBindingInfo(XamlFileCodeInfo fileCodeInfo)
	{
		_fileCodeInfosToProcess.Enqueue(fileCodeInfo);
	}

	internal void ProcessBindingInfo()
	{
		while (_fileCodeInfosToProcess.Count > 0)
		{
			XamlFileCodeInfo xamlFileCodeInfo = _fileCodeInfosToProcess.Dequeue();
			if (!XamlPerFileInfo.TryGetValue(xamlFileCodeInfo.SourceXamlGivenPath, out var value))
			{
				value = new SaveStatePerXamlFile(xamlFileCodeInfo.SourceXamlGivenPath);
				XamlPerFileInfo.Add(value.FileName, value);
			}
			value.HasBoundEventAssignments = xamlFileCodeInfo.BindStatus.HasFlag(BindStatus.HasEventBinding);
			foreach (ConnectionIdElement connectionIdElement in xamlFileCodeInfo.ConnectionIdElements)
			{
				foreach (BindPathStep value2 in connectionIdElement.BindUniverse.BindPathSteps.Values)
				{
					if (value2.ImplementsIObservableVector)
					{
						SaveStateXamlType saveStateXamlType = new SaveStateXamlType(value2.ValueType.ItemType);
						if (!value.BindingObservableVectorTypes.ContainsKey(saveStateXamlType.ToString()))
						{
							value.BindingObservableVectorTypes.Add(saveStateXamlType.ToString(), saveStateXamlType);
						}
					}
					if (value2.ImplementsIObservableMap)
					{
						SaveStateXamlType saveStateXamlType2 = new SaveStateXamlType(value2.ValueType.ItemType);
						if (!value.BindingObservableMapTypes.ContainsKey(saveStateXamlType2.ToString()))
						{
							value.BindingObservableMapTypes.Add(saveStateXamlType2.ToString(), saveStateXamlType2);
						}
					}
					foreach (BindAssignment item in value2.BindAssignments.Where((IBindAssignment ba) => ba.HasSetValueHelper))
					{
						SaveStateXamlMember saveStateXamlMember = new SaveStateXamlMember(item);
						if (!value.BindingSetters.ContainsKey(item.MemberFullName))
						{
							value.BindingSetters.Add(saveStateXamlMember.ToString(), saveStateXamlMember);
						}
					}
				}
			}
		}
	}

	public long GetXamlFileTimeAtLastCompile(string fileName)
	{
		if (!XamlPerFileInfo.TryGetValue(fileName, out var value))
		{
			return 0L;
		}
		return value.XamlFileTimeAtLastCompile;
	}

	public string GetClassFullName(string fileName)
	{
		if (!XamlPerFileInfo.TryGetValue(fileName, out var value))
		{
			return string.Empty;
		}
		return value.ClassFullName;
	}

	private void LoadFile(string fileName)
	{
		_filename = fileName;
		if (!File.Exists(fileName))
		{
			return;
		}
		using XmlTextReader reader = new XmlTextReader(fileName);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(reader);
		foreach (XmlNode childNode in xmlDocument.ChildNodes)
		{
			string name = childNode.Name;
			if (name == "xml" || !(name == "XamlCompilerSaveState"))
			{
				continue;
			}
			foreach (XmlNode childNode2 in childNode.ChildNodes)
			{
				switch (childNode2.Name)
				{
				case "XamlFeatureControlFlags":
					XamlFeatureControlFlags = childNode.InnerText;
					break;
				case "ReferenceAssemblyList":
					ReadReferenceAssemblyList(childNode2);
					break;
				case "XamlSourceFileDataList":
					ReadSourceFileDataList(childNode2);
					break;
				}
			}
		}
	}

	private void ReadReferenceAssemblyList(XmlNode listNode)
	{
		foreach (XmlNode childNode in listNode.ChildNodes)
		{
			string text = null;
			Guid empty = Guid.Empty;
			XmlNode xmlNode2 = null;
			xmlNode2 = childNode.Attributes.GetNamedItem("PathName");
			text = xmlNode2.Value;
			xmlNode2 = childNode.Attributes.GetNamedItem("HashGuid");
			empty = Guid.Parse(xmlNode2.Value);
			string name = childNode.Name;
			if (!(name == "LocalAssembly"))
			{
				if (name == "ReferenceAssembly")
				{
					ReferenceAssemblyList.Add(text);
				}
			}
			else
			{
				LocalAssemblyName = text;
			}
			ReferenceAssemblyGuids.Add(text, empty);
		}
	}

	private void ReadSourceFileDataList(XmlNode listNode)
	{
		foreach (XmlNode childNode in listNode.ChildNodes)
		{
			string name = childNode.Name;
			if (name == "XamlSourceFileData")
			{
				SaveStatePerXamlFile saveStatePerXamlFile = new SaveStatePerXamlFile(childNode);
				XamlPerFileInfo.Add(saveStatePerXamlFile.FileName, saveStatePerXamlFile);
			}
		}
	}

	private void SaveFile(string fileName)
	{
		XmlWriter xmlWriter = XmlWriter.Create(fileName);
		using (xmlWriter)
		{
			xmlWriter.WriteStartElement("XamlCompilerSaveState");
			xmlWriter.WriteElementString("XamlFeatureControlFlags", XamlFeatureControlFlags);
			xmlWriter.WriteStartElement("ReferenceAssemblyList");
			if (!string.IsNullOrEmpty(LocalAssemblyName) && ReferenceAssemblyGuids.TryGetValue(LocalAssemblyName, out var value))
			{
				xmlWriter.WriteStartElement("LocalAssembly");
				xmlWriter.WriteAttributeString("PathName", LocalAssemblyName);
				xmlWriter.WriteAttributeString("HashGuid", value.ToString());
				xmlWriter.WriteEndElement();
			}
			foreach (string referenceAssembly in ReferenceAssemblyList)
			{
				if (ReferenceAssemblyGuids.TryGetValue(referenceAssembly, out var value2))
				{
					xmlWriter.WriteStartElement("ReferenceAssembly");
					xmlWriter.WriteAttributeString("PathName", referenceAssembly);
					xmlWriter.WriteAttributeString("HashGuid", value2.ToString());
					xmlWriter.WriteEndElement();
				}
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("XamlSourceFileDataList");
			foreach (string key in XamlPerFileInfo.Keys)
			{
				SaveStatePerXamlFile saveStatePerXamlFile = XamlPerFileInfo[key];
				saveStatePerXamlFile.WriteXmlElement(xmlWriter, "XamlSourceFileData");
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
		}
	}
}
