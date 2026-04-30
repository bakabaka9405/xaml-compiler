using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class PageDefinition(XamlProjectInfo projectInfo, XamlSchemaCodeInfo schemaInfo) : Definition(projectInfo, schemaInfo)
{
	private List<ConnectionIdElement> _allConnectionIdElements;

	private List<ForwardDeclaringNamespace> _forwardDeclarations;

	private HashSet<string> _neededLocalXamlHeaderFiles = new HashSet<string>();

	private List<string> _neededCppWinRTProjectionHeaderFiles = new List<string>();

	private bool _neededXamlHeaderFilesCalculated;

	private string _checksumAlgorithmGuid;

	private IEnumerable<ApiInformation> _allApiInformations;

	private List<FileNameAndChecksumPair> _XamlFileFullPathAndCheckSums;

	private List<xProperty> _xProperties;

	public XamlClassCodeInfo CodeInfo { get; set; }

	public string ChecksumAlgorithmGuid
	{
		get
		{
			if (_checksumAlgorithmGuid == null)
			{
				string text = new Guid(2284441615u, 4536, 16915, 135, 139, 119, 14, 133, 151, 172, 22).ToString();
				_checksumAlgorithmGuid = "{" + text + "}";
			}
			return _checksumAlgorithmGuid;
		}
	}

	public IEnumerable<xProperty> XProperties
	{
		get
		{
			if (_xProperties == null)
			{
				_xProperties = new List<xProperty>();
				foreach (XamlFileCodeInfo item in CodeInfo.PerXamlFileInfo)
				{
					List<xProperty> list = item?.XPropertyInfo?.xProperties;
					if (list == null)
					{
						continue;
					}
					foreach (xProperty item2 in list)
					{
						_xProperties.Add(item2);
					}
				}
			}
			return _xProperties;
		}
	}

	public IEnumerable<FileNameAndChecksumPair> XamlFileFullPathAndCheckSums
	{
		get
		{
			if (_XamlFileFullPathAndCheckSums == null)
			{
				_XamlFileFullPathAndCheckSums = LookupXamlFileFullPathAndCheckSums();
			}
			return _XamlFileFullPathAndCheckSums;
		}
	}

	public IEnumerable<string> NeededLocalXamlHeaderFiles
	{
		get
		{
			EnsureNeededXamlHeaderFilesCalculated();
			return _neededLocalXamlHeaderFiles;
		}
	}

	public IEnumerable<string> NeededCppWinRTProjectionHeaderFiles
	{
		get
		{
			EnsureNeededXamlHeaderFilesCalculated();
			return _neededCppWinRTProjectionHeaderFiles;
		}
	}

	public IEnumerable<ForwardDeclaringNamespace> ForwardDeclarations
	{
		get
		{
			if (_forwardDeclarations == null)
			{
				_forwardDeclarations = LookupForwardDeclarations();
			}
			return _forwardDeclarations;
		}
	}

	public IEnumerable<ConnectionIdElement> AllConnectionIdElements
	{
		get
		{
			if (_allConnectionIdElements == null)
			{
				List<ConnectionIdElement> list = new List<ConnectionIdElement>();
				list.AddRange(CodeInfo.PerXamlFileInfo.SelectMany((XamlFileCodeInfo f) => f.ConnectionIdElements));
				_allConnectionIdElements = list;
			}
			return _allConnectionIdElements;
		}
	}

	public IEnumerable<ConnectionIdElement> ConnectableElements => AllConnectionIdElements.Where((ConnectionIdElement x) => x.NeedsConnectCase);

	public IEnumerable<ConnectionIdElement> UnloadableFields => from e in ConnectableElements
		where e.IsUnloadableRoot
		where e.HasFieldDefinition
		select e;

	public IEnumerable<ConnectionIdElement> DeferrableElements => ConnectableElements.Where((ConnectionIdElement e) => e.CanBeInstantiatedLater);

	public IEnumerable<ApiInformation> ApiInformationDeclarations
	{
		get
		{
			if (_allApiInformations == null)
			{
				List<ApiInformation> list = new List<ApiInformation>();
				foreach (ConnectionIdElement allConnectionIdElement in AllConnectionIdElements)
				{
					if (allConnectionIdElement.ApiInformation != null)
					{
						list.Add(allConnectionIdElement.ApiInformation);
					}
					list.AddRange(from e in allConnectionIdElement.EventAssignments
						where e.ApiInformation != null
						select e.ApiInformation);
				}
				foreach (BindUniverse bindUniverse in CodeInfo.BindUniverses)
				{
					list.AddRange(from e in bindUniverse.BoundElements
						where e.ApiInformation != null
						select e.ApiInformation);
					list.AddRange(from e in bindUniverse.BindAssignments
						where e.ApiInformation != null
						select e.ApiInformation);
					list.AddRange(from e in bindUniverse.BoundEventAssignments
						where e.ApiInformation != null
						select e.ApiInformation);
				}
				_allApiInformations = from a in list.Distinct()
					orderby a.UniqueName
					select a;
			}
			return _allApiInformations;
		}
	}

	public string GetLoadComponentUri(string priIndexName, string xamlRelativePath)
	{
		string text = ((priIndexName != null) ? (priIndexName + "/") : string.Empty);
		string text2 = xamlRelativePath.Replace('\\', '/');
		string text3 = "ms-appx:///";
		if (!string.IsNullOrEmpty(CodeInfo.XamlResourceMapName))
		{
			text3 = "ms-appx://" + CodeInfo.XamlResourceMapName + "/";
		}
		return text3 + text + text2;
	}

	private void EnsureNeededXamlHeaderFilesCalculated()
	{
		if (_neededXamlHeaderFilesCalculated)
		{
			return;
		}
		HashSet<string> neededCppWinRTProjectionHeaderFiles = new HashSet<string>();
		if (base.ProjectInfo.ClassToHeaderFileMap.TryGetValue(CodeInfo.ClassName.FullName, out var value))
		{
			_neededLocalXamlHeaderFiles.Add(value);
		}
		foreach (XamlFileCodeInfo item in CodeInfo.PerXamlFileInfo)
		{
			if (item.ConnectionIdElements.Any())
			{
				neededCppWinRTProjectionHeaderFiles.Add("winrt/Microsoft.UI.Xaml.Markup.h");
			}
			foreach (FieldDefinition item2 in from c in item.ConnectionIdElements
				where c.FieldDefinition != null
				select c.FieldDefinition)
			{
				if (base.ProjectInfo.ClassToHeaderFileMap.TryGetValue(item2.FieldTypeName, out value))
				{
					_neededLocalXamlHeaderFiles.Add(value);
				}
				else
				{
					addCppWinRTHeaderForTypeIfNecessary(item2.FieldXamlType?.UnderlyingType);
				}
			}
			foreach (List<EventAssignment> item3 in from c in item.ConnectionIdElements
				where c.HasEventAssignments
				select c.EventAssignments)
			{
				foreach (EventAssignment item4 in item3)
				{
					if (base.ProjectInfo.ClassToHeaderFileMap.TryGetValue(item4.EventType.StandardName, out value))
					{
						_neededLocalXamlHeaderFiles.Add(value);
					}
					else
					{
						addCppWinRTHeaderForTypeIfNecessary(item4.EventType?.UnderlyingType);
					}
					if (base.ProjectInfo.ClassToHeaderFileMap.TryGetValue(item4.DeclaringType.StandardName, out value))
					{
						_neededLocalXamlHeaderFiles.Add(value);
					}
					else
					{
						addCppWinRTHeaderForTypeIfNecessary(item4.DeclaringType?.UnderlyingType);
					}
				}
			}
			foreach (List<BindAssignment> item5 in from c in item.ConnectionIdElements
				where c.HasBindAssignments
				select c.BindAssignments)
			{
				foreach (BindAssignment item6 in item5)
				{
					addCppWinRTHeaderForTypeIfNecessary(item6.MemberType?.UnderlyingType);
					addCppWinRTHeaderForTypeIfNecessary(item6.MemberDeclaringType?.UnderlyingType);
				}
			}
		}
		_neededCppWinRTProjectionHeaderFiles = neededCppWinRTProjectionHeaderFiles.OrderBy((string text) => text.EndsWith(".h") ? text.Substring(0, text.Length - 2) : text).ToList();
		_neededXamlHeaderFilesCalculated = true;
		void addCppWinRTHeaderForTypeIfNecessary(Type type)
		{
			Type type2 = ((type != null && type.IsArray) ? type.GetElementType() : type);
			if (type2 != null && !XamlSchemaCodeInfo.IsProjectedPrimitiveCppType(type2.FullName))
			{
				neededCppWinRTProjectionHeaderFiles.Add("winrt/" + type2.Namespace + ".h");
				if (type2.IsGenericType)
				{
					Type[] genericArguments = type2.GetGenericArguments();
					foreach (Type type3 in genericArguments)
					{
						addCppWinRTHeaderForTypeIfNecessary(type3);
					}
				}
			}
		}
	}

	private List<FileNameAndChecksumPair> LookupXamlFileFullPathAndCheckSums()
	{
		List<FileNameAndChecksumPair> list = new List<FileNameAndChecksumPair>();
		foreach (XamlFileCodeInfo item2 in CodeInfo.PerXamlFileInfo)
		{
			string contents = ChecksumHelper.Instance.ComputeCheckSumForXamlFile(item2.FullPathToXamlFile);
			FileNameAndChecksumPair item = new FileNameAndChecksumPair(item2.FullPathToXamlFile, contents);
			list.Add(item);
		}
		return list;
	}

	private List<ForwardDeclaringNamespace> LookupForwardDeclarations()
	{
		List<ForwardDeclaringNamespace> list = new List<ForwardDeclaringNamespace>();
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		foreach (XamlFileCodeInfo item2 in CodeInfo.PerXamlFileInfo)
		{
			foreach (FieldDefinition item3 in from c in item2.ConnectionIdElements
				where c.FieldDefinition != null
				select c.FieldDefinition)
			{
				if (!item3.IsValueType)
				{
					if (!dictionary.TryGetValue(item3.FieldTypePath, out var value))
					{
						value = new List<string>();
						dictionary.Add(item3.FieldTypePath, value);
					}
					if (!value.Contains(item3.FieldTypeShortName))
					{
						value.Add(item3.FieldTypeShortName);
					}
				}
			}
			foreach (string key in dictionary.Keys)
			{
				ForwardDeclaringNamespace item = new ForwardDeclaringNamespace(key, dictionary[key]);
				list.Add(item);
			}
		}
		return list;
	}
}
