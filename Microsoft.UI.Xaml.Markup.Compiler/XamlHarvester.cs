using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xaml;
using System.Xaml.Schema;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlHarvester
{
	private class XamlDomObjectConnectionIdPair
	{
		public XamlDomObject Obj { get; set; }

		public ConnectionIdElement ConnectionId { get; set; }

		public XamlType DataRootType { get; set; }

		public IDictionary<XamlDomObject, ConnectionIdElement> DomObjectToConnectionIdElement { get; }

		public XamlDomObjectConnectionIdPair(XamlDomObject obj, XamlType dataRootType)
		{
			Obj = obj;
			ConnectionId = null;
			DataRootType = dataRootType;
			DomObjectToConnectionIdElement = new Dictionary<XamlDomObject, ConnectionIdElement>();
		}

		public XamlDomObject FindParentWithConnectionIdElement(XamlDomObject domObject)
		{
			XamlDomObject obj = Obj;
			for (XamlDomObject xamlDomObject = domObject.Parent?.Parent; xamlDomObject != null; xamlDomObject = xamlDomObject.Parent?.Parent)
			{
				if (DomObjectToConnectionIdElement.ContainsKey(xamlDomObject))
				{
					return xamlDomObject;
				}
				if (xamlDomObject == obj)
				{
					return null;
				}
			}
			return null;
		}
	}

	private string _projectFolder;

	private bool _isPass1;

	private Platform _targPlat;

	private Dictionary<XamlDomObject, ConnectionIdElement> _collectedObjects;

	public bool SkipNameFieldsForRootElements { get; set; }

	public XamlHarvester(string projectFolder, bool isPass1, Platform targPlat)
	{
		_isPass1 = isPass1;
		_projectFolder = Path.GetFullPath(projectFolder);
		_targPlat = targPlat;
		_collectedObjects = new Dictionary<XamlDomObject, ConnectionIdElement>();
		DirectoryInfo directoryInfo = new DirectoryInfo(projectFolder);
		if (!directoryInfo.Exists)
		{
			throw new ArgumentException(XamlCompilerResources.Harvester_ProjectFolderIsNotADirectory, projectFolder);
		}
	}

	public XamlClassCodeInfo HarvestClassInfo(string classFullName, XamlDomObject domRoot, bool isApplication)
	{
		if (domRoot == null)
		{
			throw new ArgumentNullException("domRoot");
		}
		XamlClassCodeInfo xamlClassCodeInfo = new XamlClassCodeInfo(classFullName, isApplication);
		xamlClassCodeInfo.BaseTypeName = GetFullTypePath(domRoot.Type);
		xamlClassCodeInfo.BaseType = domRoot.Type;
		if (!_isPass1 && !string.IsNullOrEmpty(classFullName))
		{
			XamlTypeName xamlTypeNameFromFullName = XamlSchemaCodeInfo.GetXamlTypeNameFromFullName(classFullName);
			XamlType xamlType = domRoot.SchemaContext.GetXamlType(xamlTypeNameFromFullName);
			if (xamlType != null)
			{
				xamlClassCodeInfo.ClassXamlType = xamlType;
				xamlClassCodeInfo.ClassType = new TypeForCodeGen(xamlType);
			}
		}
		return xamlClassCodeInfo;
	}

	public XamlFileCodeInfo HarvestXamlFileInfo(XamlClassCodeInfo classCodeInfo, XamlDomObject domRoot)
	{
		if (domRoot == null)
		{
			throw new ArgumentNullException("domRoot");
		}
		XamlFileCodeInfo xamlFileCodeInfo = new XamlFileCodeInfo();
		if (!string.IsNullOrWhiteSpace(classCodeInfo.ClassName.FullName) && !CollectCodeBehindElements(domRoot, classCodeInfo, xamlFileCodeInfo))
		{
			return null;
		}
		return xamlFileCodeInfo;
	}

	private string GetFullTypePath(XamlType type)
	{
		string result = string.Empty;
		if (type.IsUnknown)
		{
			if (IsPossiblyALocalType(type, out var usingTypePath))
			{
				result = usingTypePath + "." + type.Name;
			}
		}
		else
		{
			result = type.UnderlyingType.FullName;
		}
		return result;
	}

	private bool IsACollectableCodeBehindElement(XamlDomObject domObject)
	{
		try
		{
			if (domObject.IsGetObject)
			{
				return false;
			}
			if (DomHelper.IsNamedCollectableObject(domObject, _isPass1))
			{
				return true;
			}
			using (IEnumerator<XamlDomMember> enumerator = domObject.MemberNodes.Where((XamlDomMember mem) => mem.Member.IsEvent).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					XamlDomMember current = enumerator.Current;
					return true;
				}
			}
			using IEnumerator<XamlDomMember> enumerator2 = domObject.MemberNodes.Where((XamlDomMember mem) => mem.Items.Count == 1 && mem.Item is XamlDomObject && domObject.SchemaContext is DirectUISchemaContext && ((XamlDomObject)mem.Item).Type == ((DirectUISchemaContext)domObject.SchemaContext).DirectUIXamlLanguage.BindExtension).GetEnumerator();
			if (enumerator2.MoveNext())
			{
				XamlDomMember current2 = enumerator2.Current;
				return true;
			}
		}
		catch (TypeLoadException ex)
		{
			if (!(domObject.Type.SchemaContext is DirectUISchemaContext directUISchemaContext))
			{
				throw;
			}
			directUISchemaContext.SchemaErrors.Add(new XamlSchemaError_TypeLoadException(domObject, domObject.Type.Name, ex.Message));
		}
		return false;
	}

	private bool IsAPotentialBindingRoot(XamlDomObject domObject)
	{
		try
		{
			if (domObject.Parent != null && domObject.Parent.Parent != null)
			{
				return DomHelper.IsDerivedFromDataTemplate(domObject.Parent.Parent) || DomHelper.IsDerivedFromControlTemplate(domObject.Parent.Parent);
			}
		}
		catch (TypeLoadException ex)
		{
			if (!(domObject.Type.SchemaContext is DirectUISchemaContext directUISchemaContext))
			{
				throw;
			}
			directUISchemaContext.SchemaErrors.Add(new XamlSchemaError_TypeLoadException(domObject, domObject.Type.Name, ex.Message));
		}
		return false;
	}

	internal static bool IsPossiblyALocalType(XamlType xamlType, out string usingTypePath)
	{
		foreach (string xamlNamespace in xamlType.GetXamlNamespaces())
		{
			if (xamlNamespace.HasUsingPrefix())
			{
				usingTypePath = xamlNamespace.StripUsingPrefix();
				if (usingTypePath.IsConditionalNamespace())
				{
					try
					{
						usingTypePath = ConditionalNamespace.Parse(usingTypePath).UnconditionalNamespace;
					}
					catch (ParseException)
					{
						return false;
					}
				}
				return true;
			}
			if (ClrNamespaceParser.TryParseUri(xamlNamespace, out usingTypePath, out var assemblyName, out var _, returnErrors: false) && string.IsNullOrEmpty(assemblyName))
			{
				return true;
			}
		}
		usingTypePath = null;
		return false;
	}

	private bool CollectCodeBehindElements(XamlDomObject domRoot, XamlClassCodeInfo classCodeInfo, XamlFileCodeInfo fileCodeInfo)
	{
		Stack<XamlDomObjectConnectionIdPair> roots = new Stack<XamlDomObjectConnectionIdPair>();
		XamlDomIteratorEnterNewScopeEvent xamlDomIteratorEnterNewScopeEvent = delegate(XamlDomObject obj2)
		{
			roots.Push(new XamlDomObjectConnectionIdPair(obj2, classCodeInfo.ClassXamlType));
		};
		XamlDomIteratorExitNewScopeEvent xamlDomIteratorExitNewScopeEvent = delegate
		{
			ConnectionIdElement connectionId = roots.Peek().ConnectionId;
			if (connectionId?.BindUniverse != null)
			{
				classCodeInfo.BindUniverses.Add(connectionId.BindUniverse);
			}
			roots.Pop();
			if (roots.Count > 0)
			{
				ConnectionIdElement connectionId2 = roots.Peek().ConnectionId;
				if (connectionId2?.BindUniverse != null && connectionId?.BindUniverse != null)
				{
					connectionId.BindUniverse.Parent = connectionId2.BindUniverse;
					connectionId2.BindUniverse.Children.Add(connectionId.BindUniverse);
				}
			}
		};
		XamlDomIterator xamlDomIterator = new XamlDomIterator(domRoot);
		xamlDomIterator.EnterNewScopeCallback += xamlDomIteratorEnterNewScopeEvent;
		xamlDomIterator.ExitScopeCallback += xamlDomIteratorExitNewScopeEvent;
		xamlDomIteratorEnterNewScopeEvent(domRoot);
		foreach (XamlDomObject item3 in from namedObject in xamlDomIterator.DescendantsAndSelf()
			where DomHelper.HasDefaultBindModeMember(namedObject)
			select namedObject)
		{
			fileCodeInfo.StrippableMembers.Add(new StrippableMember(DomHelper.GetDefaultBindModeMember(item3)));
		}
		if (domRoot.XPropertyInfo != null)
		{
			fileCodeInfo.StrippableObjects.Add(new StrippableObject(domRoot.XPropertyInfo.xPropertiesNode));
			fileCodeInfo.XPropertyInfo = domRoot.XPropertyInfo;
		}
		foreach (XamlDomObject item4 in from result in xamlDomIterator.DescendantsAndSelf()
			select (result))
		{
			if (DomHelper.IsObjectInvalidForPlatform(item4, _targPlat))
			{
				fileCodeInfo.StrippableObjects.Add(new StrippableObject(item4));
			}
			XamlDomObject xamlDomObject = item4;
			IList<XamlDomMember> memberNodes = xamlDomObject.MemberNodes;
			foreach (XamlDomMember memberNode in item4.MemberNodes)
			{
				if (DomHelper.IsMemberInvalidForPlatform(memberNode, _targPlat))
				{
					fileCodeInfo.StrippableMembers.Add(new StrippableMember(memberNode));
				}
			}
			KeyedCollection<string, XamlDomNamespace> namespaces = xamlDomObject.Namespaces;
			foreach (XamlDomNamespace @namespace in xamlDomObject.Namespaces)
			{
				string instance = @namespace.NamespaceDeclaration.Namespace;
				if (!instance.HasUsingPrefix())
				{
					continue;
				}
				string text = instance.StripUsingPrefix();
				if (text.IsConditionalNamespace())
				{
					Platform platform = Platform.Any;
					try
					{
						platform = ConditionalNamespace.Parse(text).PlatConditional;
					}
					catch (ParseException)
					{
					}
					if (platform != Platform.Any)
					{
						fileCodeInfo.StrippableNamespaces.Add(new StrippableNamespace(@namespace, !DomHelper.ConditionalValidForPlatform(platform, _targPlat)));
					}
				}
			}
			XamlDomMember suppressXamlTrimWarningsMember = DomHelper.GetSuppressXamlTrimWarningsMember(item4);
			if (suppressXamlTrimWarningsMember != null)
			{
				DirectUIXamlType obj = item4.Type as DirectUIXamlType;
				if ((object)obj != null && obj.IsAssignableToBinding)
				{
					fileCodeInfo.SuppressXamlTrimWarningsBindingMembers.Add(new StrippableMember(suppressXamlTrimWarningsMember));
				}
				else
				{
					fileCodeInfo.StrippableMembers.Add(new StrippableMember(suppressXamlTrimWarningsMember));
				}
			}
			if (_isPass1 || DomHelper.IsDerivedFromDataTemplate(item4) || DomHelper.IsDerivedFromControlTemplate(item4))
			{
				continue;
			}
			XamlDomMember dataTypeMember = DomHelper.GetDataTypeMember(item4, getDirectiveOnly: true);
			if (dataTypeMember != null)
			{
				string stringValueOfProperty = DomHelper.GetStringValueOfProperty(dataTypeMember);
				if (!string.IsNullOrEmpty(stringValueOfProperty))
				{
					DataTypeAssignment item = new DataTypeAssignment(dataTypeMember);
					fileCodeInfo.DataTypeAssignments.Add(item);
				}
			}
		}
		foreach (XamlDomObject item5 in from xamlDomObject3 in xamlDomIterator.DescendantsAndSelf()
			where IsACollectableCodeBehindElement(xamlDomObject3) || IsAPotentialBindingRoot(xamlDomObject3) || xamlDomObject3 == roots.Peek().Obj
			select xamlDomObject3)
		{
			XamlDomObjectConnectionIdPair xamlDomObjectConnectionIdPair = roots.Peek();
			bool flag = false;
			if (DomHelper.IsDerivedFromDataTemplate(xamlDomObjectConnectionIdPair.Obj))
			{
				if (item5.Parent != null && item5.Parent.Parent != null && xamlDomObjectConnectionIdPair.Obj.Equals(item5.Parent.Parent))
				{
					flag = true;
					XamlType xamlType = null;
					XamlDomMember dataTypeMember2 = DomHelper.GetDataTypeMember(xamlDomObjectConnectionIdPair.Obj);
					string stringValueOfProperty2 = DomHelper.GetStringValueOfProperty(dataTypeMember2);
					if (!string.IsNullOrEmpty(stringValueOfProperty2))
					{
						xamlType = xamlDomObjectConnectionIdPair.Obj.ResolveXmlName(stringValueOfProperty2);
						if (!_isPass1)
						{
							DataTypeAssignment item2 = new DataTypeAssignment(dataTypeMember2);
							fileCodeInfo.DataTypeAssignments.Add(item2);
						}
						ConnectionIdElement connectionIdElement = CollectElement(xamlDomObjectConnectionIdPair.Obj, xamlDomObjectConnectionIdPair.Obj, null, classCodeInfo, fileCodeInfo, xamlType);
						xamlDomObjectConnectionIdPair.DomObjectToConnectionIdElement[xamlDomObjectConnectionIdPair.Obj] = connectionIdElement;
						xamlDomObjectConnectionIdPair.ConnectionId = connectionIdElement;
						EnsureTemplateUniverse(connectionIdElement, xamlType, classCodeInfo);
					}
					else
					{
						xamlType = xamlDomObjectConnectionIdPair.Obj.Type;
					}
					xamlDomObjectConnectionIdPair.Obj = item5;
					xamlDomObjectConnectionIdPair.DataRootType = xamlType;
				}
			}
			else if (DomHelper.IsDerivedFromControlTemplate(xamlDomObjectConnectionIdPair.Obj) && item5.Parent != null && item5.Parent.Parent != null && xamlDomObjectConnectionIdPair.Obj.Equals(item5.Parent.Parent))
			{
				XamlType xamlType2 = null;
				XamlDomMember dataTypeMember3 = DomHelper.GetDataTypeMember(xamlDomObjectConnectionIdPair.Obj);
				string stringValueOfProperty3 = DomHelper.GetStringValueOfProperty(dataTypeMember3);
				if (!string.IsNullOrEmpty(stringValueOfProperty3))
				{
					xamlType2 = xamlDomObjectConnectionIdPair.Obj.ResolveXmlName(stringValueOfProperty3);
					ConnectionIdElement connectionIdElement2 = CollectElement(xamlDomObjectConnectionIdPair.Obj, xamlDomObjectConnectionIdPair.Obj, null, classCodeInfo, fileCodeInfo, xamlType2);
					EnsureTemplateUniverse(connectionIdElement2, xamlType2, classCodeInfo);
					xamlDomObjectConnectionIdPair.DomObjectToConnectionIdElement[xamlDomObjectConnectionIdPair.Obj] = connectionIdElement2;
					xamlDomObjectConnectionIdPair.ConnectionId = connectionIdElement2;
					xamlDomObjectConnectionIdPair.DataRootType = xamlType2;
				}
				else
				{
					xamlType2 = xamlDomObjectConnectionIdPair.Obj.Type;
				}
			}
			ConnectionIdElement connectionIdElement3 = CollectElement(xamlDomObjectConnectionIdPair.Obj, item5, (item5 == xamlDomObjectConnectionIdPair.Obj || xamlDomObjectConnectionIdPair.ConnectionId == null) ? null : xamlDomObjectConnectionIdPair.ConnectionId.BindUniverse, classCodeInfo, fileCodeInfo, xamlDomObjectConnectionIdPair.DataRootType);
			if (flag && !connectionIdElement3.BindUniverse.BoundElements.Contains(connectionIdElement3))
			{
				connectionIdElement3.BindUniverse.BoundElements.Add(connectionIdElement3);
			}
			if (item5 == xamlDomObjectConnectionIdPair.Obj)
			{
				xamlDomObjectConnectionIdPair.ConnectionId = connectionIdElement3;
			}
			xamlDomObjectConnectionIdPair.DomObjectToConnectionIdElement[item5] = connectionIdElement3;
			if (item5 != xamlDomObjectConnectionIdPair.Obj && item5.Parent != null)
			{
				XamlDomObject xamlDomObject2 = xamlDomObjectConnectionIdPair.FindParentWithConnectionIdElement(item5);
				if (xamlDomObject2 != null)
				{
					ConnectionIdElement connectionIdElement4 = xamlDomObjectConnectionIdPair.DomObjectToConnectionIdElement[xamlDomObject2];
					connectionIdElement4.Children.Add(connectionIdElement3);
				}
			}
			if (!_isPass1 && connectionIdElement3 != null && connectionIdElement3.HasBindAssignments && DomHelper.UnderANamescope(item5, _isPass1) && !(xamlDomObjectConnectionIdPair?.ConnectionId?.BindUniverse?.DataRootType != null))
			{
				LineNumberInfo lineNumberInfo = new LineNumberInfo(item5);
				if (!(item5.Type.SchemaContext is DirectUISchemaContext directUISchemaContext))
				{
					throw new XamlException($"Unexpected: Cannot get a schema for domObject {item5.Type.Name}");
				}
				if (xamlDomObjectConnectionIdPair != null && xamlDomObjectConnectionIdPair.Obj.Type.IsDerivedFromControlTemplate())
				{
					directUISchemaContext.SchemaErrors.Add(new XamlXBindControlTemplateDoesNotDefineTargetTypeError(item5));
				}
				else
				{
					directUISchemaContext.SchemaErrors.Add(new XamlXBindDataTemplateDoesNotDefineDataTypeError(item5));
				}
				return false;
			}
		}
		xamlDomIteratorExitNewScopeEvent();
		xamlDomIterator.EnterNewScopeCallback -= xamlDomIteratorEnterNewScopeEvent;
		xamlDomIterator.ExitScopeCallback -= xamlDomIteratorExitNewScopeEvent;
		return true;
	}

	private void EnsureTemplateUniverse(ConnectionIdElement templateConnectionIdElement, XamlType dataRootType, XamlClassCodeInfo classCodeInfo)
	{
		if (templateConnectionIdElement.BindUniverse.RootElement != templateConnectionIdElement)
		{
			BindUniverse bindUniverse = new BindUniverse(templateConnectionIdElement, dataRootType, isFileRoot: false, classCodeInfo.ClassName.ShortName);
			templateConnectionIdElement.BindUniverse = bindUniverse;
		}
	}

	private ConnectionIdElement CollectElement(XamlDomObject domRoot, XamlDomObject domObject, BindUniverse bindUniverse, XamlClassCodeInfo classCodeInfo, XamlFileCodeInfo fileCodeInfo, XamlType dataRootType)
	{
		bool skipFieldDefinition = (SkipNameFieldsForRootElements && domObject == domRoot) || DomHelper.UnderANamescope(domObject, _isPass1);
		ConnectionIdElement value = null;
		if (_collectedObjects.TryGetValue(domObject, out value))
		{
			return value;
		}
		if (domObject.Type.IsUnknown)
		{
			if (_isPass1 && IsPossiblyALocalType(domObject.Type, out var usingTypePath))
			{
				value = new ConnectionIdElement(domObject, bindUniverse, fileCodeInfo, classCodeInfo, dataRootType, skipFieldDefinition, usingTypePath);
			}
		}
		else
		{
			value = new ConnectionIdElement(domObject, bindUniverse, fileCodeInfo, classCodeInfo, dataRootType, skipFieldDefinition);
		}
		if (value != null)
		{
			fileCodeInfo.ConnectionIdElements.Add(value);
			_collectedObjects.Add(domObject, value);
		}
		return value;
	}

	public static string GetClassFullName(XamlDomObject domRoot)
	{
		string stringValueOfProperty = DomHelper.GetStringValueOfProperty(domRoot, XamlLanguage.Class);
		if (string.IsNullOrEmpty(stringValueOfProperty))
		{
			return null;
		}
		string[] array = stringValueOfProperty.Split('.');
		if (array.Length == 1)
		{
			string message = ResourceUtilities.FormatString(XamlCompilerResources.Harvester_ClassMustHaveANamespace, stringValueOfProperty);
			throw new XamlException(message, null, domRoot.StartLineNumber, domRoot.StartLinePosition);
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				string message2 = ResourceUtilities.FormatString(XamlCompilerResources.Harvester_ClassNameEmptyPathPart, stringValueOfProperty);
				throw new XamlException(message2, null, domRoot.StartLineNumber, domRoot.StartLinePosition);
			}
			if (text.Contains(' ') || text.Contains('\t') || text.Contains('\n'))
			{
				string message2 = ResourceUtilities.FormatString(XamlCompilerResources.Harvester_ClassNameNoWhiteSpace, stringValueOfProperty, text);
				throw new XamlException(message2, null, domRoot.StartLineNumber, domRoot.StartLinePosition);
			}
			if (!XamlDomValidator.IsValidIdentifierName(text))
			{
				string message2 = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_BadName, text, "Class", domRoot.Type.Name);
				throw new XamlException(message2, null, domRoot.StartLineNumber, domRoot.StartLinePosition);
			}
		}
		return stringValueOfProperty;
	}
}
