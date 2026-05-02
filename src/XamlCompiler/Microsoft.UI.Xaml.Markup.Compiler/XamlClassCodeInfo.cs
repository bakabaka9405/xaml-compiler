using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlClassCodeInfo : IXamlClassCodeInfo
{
	private Dictionary<string, FieldDefinition> fieldDefinitions;

	private List<FieldDefinition> fieldDeclarations;

	private List<XamlFileCodeInfo> xamlFileCodeInfoList = new List<XamlFileCodeInfo>();

	private List<BindUniverse> bindUniverses;

	private bool? hasEventAssignments;

	private BindStatus? bindStatus;

	private bool? hasPhaseAssignments;

	private string baseFileName;

	private int lastConnectionId;

	public bool IsApplication { get; private set; }

	public string RootNamespace { get; set; }

	public ClassName ClassName { get; set; }

	public string BaseFileName
	{
		get
		{
			if (string.IsNullOrEmpty(baseFileName))
			{
				baseFileName = ComputeBaseFileName();
			}
			return baseFileName;
		}
	}

	public string BaseApparentRelativeFolder { get; private set; }

	public string BaseApparentRelativePath => Path.Combine(BaseApparentRelativeFolder, BaseFileName) + ".xaml";

	public string TargetFolder { get; set; }

	public string PriIndexName { get; set; }

	public XamlType ClassXamlType { get; set; }

	public TypeForCodeGen ClassType { get; set; }

	public string BaseTypeName { get; set; }

	public XamlType BaseType { get; set; }

	public string XamlResourceMapName { get; set; }

	public string XamlComponentResourceLocation { get; set; }

	public bool IsResourceDictionary
	{
		get
		{
			if (ClassXamlType != null)
			{
				return ClassXamlType.IsDerivedFromResourceDictionary();
			}
			return false;
		}
	}

	public bool HasFieldDefinitions
	{
		get
		{
			if (fieldDefinitions != null)
			{
				return fieldDefinitions.Count > 0;
			}
			return false;
		}
	}

	public bool HasEventAssignments
	{
		get
		{
			if (!hasEventAssignments.HasValue)
			{
				hasEventAssignments = xamlFileCodeInfoList.Any((XamlFileCodeInfo x) => x.HasEventAssignments);
			}
			return hasEventAssignments.Value;
		}
	}

	public BindStatus BindStatus
	{
		get
		{
			if (!bindStatus.HasValue)
			{
				bindStatus = BindStatus.None;
				foreach (XamlFileCodeInfo xamlFileCodeInfo in xamlFileCodeInfoList)
				{
					bindStatus |= xamlFileCodeInfo.BindStatus;
				}
			}
			return bindStatus.Value;
		}
	}

	public bool HasPhaseAssignments
	{
		get
		{
			if (!hasPhaseAssignments.HasValue)
			{
				hasPhaseAssignments = xamlFileCodeInfoList.Any((XamlFileCodeInfo x) => x.HasPhaseAssignments);
			}
			return hasPhaseAssignments.Value;
		}
	}

	public List<BindUniverse> BindUniverses
	{
		get
		{
			if (bindUniverses == null)
			{
				bindUniverses = new List<BindUniverse>();
			}
			return bindUniverses;
		}
	}

	public IEnumerable<FieldDefinition> FieldDeclarations
	{
		get
		{
			if (fieldDeclarations == null)
			{
				fieldDeclarations = new List<FieldDefinition>();
				if (fieldDefinitions != null)
				{
					fieldDeclarations.AddRange(fieldDefinitions.Values);
				}
			}
			return fieldDeclarations;
		}
	}

	public List<XamlFileCodeInfo> PerXamlFileInfo => xamlFileCodeInfoList;

	public bool IsUsingCompiledBinding
	{
		get
		{
			if (!HasBindAssignments)
			{
				return HasBoundEventAssignments;
			}
			return true;
		}
	}

	public bool HasBindingSetters => (from ba in BindUniverses.SelectMany((BindUniverse bu) => bu.BindAssignments)
		where ba.HasSetValueHelper
		select ba).Any();

	public bool HasBindAssignments => BindStatus.HasFlag(BindStatus.HasBinding);

	public bool HasBoundEventAssignments => BindStatus.HasFlag(BindStatus.HasEventBinding);

	public bool HasInComponentBase => DomHelper.IsLocalType(BaseType);

	public int NextConnectionId => ++lastConnectionId;

	public XamlClassCodeInfo(string classFullName, bool isApplication)
	{
		ClassName = new ClassName(classFullName);
		IsApplication = isApplication;
	}

	public void AddXamlFileInfo(XamlFileCodeInfo fileCodeInfo)
	{
		xamlFileCodeInfoList.Add(fileCodeInfo);
		foreach (ConnectionIdElement connectionIdElement in fileCodeInfo.ConnectionIdElements)
		{
			if (connectionIdElement.FieldDefinition != null)
			{
				bool flag = AddFieldDefinition(connectionIdElement.FieldDefinition);
			}
		}
		string directoryName = Path.GetDirectoryName(fileCodeInfo.ApparentRelativePath);
		if (BaseApparentRelativeFolder == null)
		{
			BaseApparentRelativeFolder = directoryName;
		}
		else
		{
			BaseApparentRelativeFolder = FileHelpers.ComputeBaseFolder(BaseApparentRelativeFolder, directoryName);
		}
	}

	private bool AddFieldDefinition(FieldDefinition newFieldDef)
	{
		if (fieldDefinitions == null)
		{
			fieldDefinitions = new Dictionary<string, FieldDefinition>();
		}
		bool result = false;
		string fieldName = newFieldDef.FieldName;
		if (fieldDefinitions.TryGetValue(fieldName, out var value))
		{
			if (value.HasSameAttributes(newFieldDef))
			{
				return false;
			}
			if (!value.CanBeMerged(newFieldDef))
			{
				return false;
			}
			fieldDefinitions.Remove(fieldName);
			FieldDefinition fieldDefinition = FieldDefinition.CreateMerged(value, newFieldDef);
			newFieldDef = fieldDefinition;
			result = true;
		}
		fieldDefinitions.Add(fieldName, newFieldDef);
		return result;
	}

	private string ComputeBaseFileName()
	{
		int index = 0;
		List<string[]> fileNamesTokens = new List<string[]>();
		StringBuilder strBuilder = new StringBuilder();
		foreach (XamlFileCodeInfo xamlFileCodeInfo in xamlFileCodeInfoList)
		{
			fileNamesTokens.Add(Path.GetFileNameWithoutExtension(xamlFileCodeInfo.ApparentRelativePath).Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries));
		}
		Action action = delegate
		{
			string text;
			do
			{
				text = null;
				foreach (string[] item in fileNamesTokens)
				{
					if (item.Length <= index)
					{
						return;
					}
					if (text == null)
					{
						text = item[index];
					}
					else if (text != item[index])
					{
						if (index == 0)
						{
							throw new XamlException(string.Format(XamlCompilerResources.XamlCompiler_BaseFilenamesMustBeTheSame, ClassName.FullName, text, item[index]));
						}
						return;
					}
				}
				strBuilder.Append(((index == 0) ? "" : ".") + text);
				index++;
			}
			while (text != null);
		};
		action();
		return strBuilder.ToString();
	}
}
