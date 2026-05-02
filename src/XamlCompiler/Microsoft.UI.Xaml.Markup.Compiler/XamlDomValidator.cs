using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Adds;
using System.Xaml;
using System.Xaml.Schema;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class XamlDomValidator
{
	private Lazy<List<XamlCompileError>> _errors = new Lazy<List<XamlCompileError>>(() => new List<XamlCompileError>());

	private Lazy<List<XamlCompileWarning>> _warnings = new Lazy<List<XamlCompileWarning>>(() => new List<XamlCompileWarning>());

	private NamedElementsStore _namedElementsHash = new NamedElementsStore();

	private DirectUISchemaContext _schemaContext;

	private bool _domRootHasCodeBehind;

	private bool _skipMinSdkValidation = true;

	private HashSet<string> _minVersionTypeCache = new HashSet<string>();

	private HashSet<Tuple<string, string>> _minVersionMemberCache = new HashSet<Tuple<string, string>>();

	private Dictionary<string, Version> _contractCache = new Dictionary<string, Version>();

	private static readonly Dictionary<(string, string), List<string>> UnsupportedEnumValues = new Dictionary<(string, string), List<string>>
	{
		{
			("TextBox", "TextWrapping"),
			new List<string> { "WrapWholeWords", "3" }
		},
		{
			("RichEditBox", "TextWrapping"),
			new List<string> { "WrapWholeWords", "3" }
		}
	};

	public bool IsPass1 { get; set; }

	public bool HasUnknownChildren { get; set; }

	public Version TargetPlatformMinVersion { get; set; }

	public Platform XamlPlatform { get; set; }

	public bool HasErrors
	{
		get
		{
			if (_errors.IsValueCreated)
			{
				return _errors.Value.Count > 0;
			}
			return false;
		}
	}

	public List<XamlCompileError> Errors => _errors.Value;

	public bool HasWarnings => _warnings.IsValueCreated;

	public List<XamlCompileWarning> Warnings => _warnings.Value;

	public bool Validate(XamlDomObject domRoot)
	{
		_schemaContext = (DirectUISchemaContext)domRoot.Type.SchemaContext;
		_domRootHasCodeBehind = HasStringAttribute(domRoot, XamlLanguage.Class);
		CheckXaml(domRoot);
		return HasErrors;
	}

	private void CheckXaml(XamlDomObject domRoot)
	{
		if (TryGetStringAttribute(domRoot, XamlLanguage.Class, out var value, out var domMember))
		{
			ProcessClassName(domRoot, domMember, value);
		}
		XamlDomIterator xamlDomIterator = new XamlDomIterator(domRoot);
		try
		{
			xamlDomIterator.EnterNewScopeCallback += _namedElementsHash.EnterNewScope;
			xamlDomIterator.ExitScopeCallback += _namedElementsHash.ExitCurrentScope;
			foreach (XamlDomObject item in xamlDomIterator.DescendantsAndSelf())
			{
				string errorMessage;
				if (DomHelper.IsObjectInvalidForPlatform(item, XamlPlatform))
				{
					if (XamlPlatform == Platform.Any)
					{
						Errors.Add(new XamlValidationPlatformConditionalStrict(item));
					}
				}
				else if (DomHelper.IsObjectInvalidForPlatform(item, Platform.Any))
				{
					Warnings.Add(new XamlValidationWarningExperimental(ErrorCode.WMC1504, item, "Platform Conditionals"));
				}
				else if (CheckIsUnresolvedForwardedType(item, out errorMessage))
				{
					Errors.Add(new XamlValidationErrorUnresolvedForwardedTypeAssembly(item, errorMessage));
				}
				else if (item.IsGetObject)
				{
					ProcessGetObjectNode(item);
				}
				else if (item.Type.IsUnknown)
				{
					ProcessUnknownObjectNode(item);
					if (item.Type.IsUnknown)
					{
						HasUnknownChildren = true;
					}
				}
				else
				{
					ProcessObjectNode(item, item == domRoot);
				}
			}
		}
		finally
		{
			xamlDomIterator.ExitScopeCallback -= _namedElementsHash.ExitCurrentScope;
			xamlDomIterator.EnterNewScopeCallback -= _namedElementsHash.EnterNewScope;
		}
	}

	private bool CheckIsUnresolvedForwardedType(XamlDomObject domObject, out string errorMessage)
	{
		bool result = false;
		errorMessage = null;
		try
		{
			ForceTypeResolution(domObject?.Type);
		}
		catch (UnresolvedAssemblyException ex)
		{
			result = true;
			errorMessage = ex.Message;
		}
		return result;
	}

	private bool CheckIsUnresolvedForwardedType(XamlDomMember domMember, out string errorMessage)
	{
		bool result = false;
		errorMessage = null;
		try
		{
			ForceTypeResolution(domMember?.Member?.Type);
		}
		catch (UnresolvedAssemblyException ex)
		{
			result = true;
			errorMessage = ex.Message;
		}
		return result;
	}

	private void ForceTypeResolution(XamlType type)
	{
		if (type != null)
		{
			type.CanAssignTo(_schemaContext.DirectUIXamlLanguage.UIElement);
		}
	}

	private bool ProcessUnknownObjectNode(XamlDomObject domObject)
	{
		ErrorOnUnknownType(domObject);
		foreach (XamlDomMember memberNode in domObject.MemberNodes)
		{
			XamlMember member = memberNode.Member;
			if (member.IsDirective)
			{
				XamlDirective xamlDirective = (XamlDirective)member;
				CheckIdDirectiveUsage(domObject, memberNode, xamlDirective);
			}
		}
		return true;
	}

	private void ErrorOnUnknownType(XamlDomObject domObject)
	{
		XamlType type = domObject.Type;
		string preferredXamlNamespace = type.PreferredXamlNamespace;
		XamlTypeName xamlTypeName = new XamlTypeName(preferredXamlNamespace, type.Name);
		XamlType xamlType = type.SchemaContext.GetXamlType(xamlTypeName);
		if (xamlType != null && !xamlType.IsPublic)
		{
			Errors.Add(new XamlValidationErrorNonPublicType(domObject));
		}
		else if (!IsPass1 || !IsPossiblyALocalType(type))
		{
			if (KS.Eq(preferredXamlNamespace, "using:"))
			{
				Errors.Add(new XamlCompilerTypeMustHaveANamespace(domObject));
			}
			else
			{
				Errors.Add(new XamlValidationErrorUnknownObject(domObject));
			}
		}
	}

	private void ProcessObjectNode(XamlDomObject domObject, bool isRoot)
	{
		bool flag = false;
		bool flag2 = false;
		List<string> list = new List<string>();
		if (!isRoot || !HasStringAttribute(domObject, XamlLanguage.Class))
		{
			CheckDomObjectIsConstructibleOrTypeConvertable(domObject);
		}
		if (!IsPass1 && _schemaContext.DirectUISystem.Style.IsAssignableFrom(domObject.Type.UnderlyingType))
		{
			XamlType andResolveStyleTargetTypeProperty = GetAndResolveStyleTargetTypeProperty(domObject, reportErrors: true);
			if (andResolveStyleTargetTypeProperty == null)
			{
				return;
			}
			XamlDomMember memberNode = domObject.GetMemberNode("BasedOn");
			if (memberNode != null)
			{
				CheckBasedOnIsCorrect(domObject, andResolveStyleTargetTypeProperty, memberNode);
			}
		}
		XamlMember contentProperty = domObject.Type.ContentProperty;
		if (contentProperty != null && contentProperty.IsUnknown)
		{
			Errors.Add(new XamlValidationErrorBadCPA(domObject, contentProperty.Name));
		}
		if (!IsPass1)
		{
			DirectUIXamlType directUIXamlType = (DirectUIXamlType)domObject.Type;
			if (directUIXamlType.IsDeprecated)
			{
				if (directUIXamlType.IsHardDeprecated)
				{
					XamlValidationErrorDeprecated item = new XamlValidationErrorDeprecated(domObject, directUIXamlType.Name, directUIXamlType.DeprecatedMessage);
					Errors.Add(item);
				}
				else
				{
					XamlValidationWarningDeprecated item2 = new XamlValidationWarningDeprecated(domObject, directUIXamlType.Name, directUIXamlType.DeprecatedMessage);
					Warnings.Add(item2);
				}
			}
			if (directUIXamlType.IsExperimental)
			{
				XamlValidationWarningExperimental item3 = new XamlValidationWarningExperimental(ErrorCode.WMC1501, domObject, directUIXamlType.Name);
				Warnings.Add(item3);
			}
		}
		if (!IsPass1)
		{
			ValidateTypePresentInMinVersion(domObject.Type.UnderlyingType, domObject, null);
		}
		ValidateNamespaces(domObject.Namespaces);
		foreach (XamlDomMember memberNode3 in domObject.MemberNodes)
		{
			XamlMember member = memberNode3.Member;
			if (member.IsUnknown)
			{
				ProcessUnknownMemberNode(domObject, memberNode3);
				if (DomHelper.IsDeferLoadStrategyMember(memberNode3))
				{
					flag = true;
				}
				else if (DomHelper.IsLoadMember(memberNode3))
				{
					flag2 = true;
				}
				if (flag && flag2)
				{
					Errors.Add(new XamlValidationError_LoadConflict(memberNode3));
				}
				continue;
			}
			if (list.Contains(member.NameWithApiInformation()))
			{
				Errors.Add(new XamlValidationErrorDuplicateAssigment(memberNode3));
				continue;
			}
			list.Add(member.NameWithApiInformation());
			if (member == ((DirectUIXamlType)domObject.Type).GetAliasedProperty(XamlLanguage.Name))
			{
				EnsureUniqueElementName(domObject, DomHelper.GetStringValueOfProperty(memberNode3));
			}
			if (member.IsDirective)
			{
				XamlDirective xamlDirective = (XamlDirective)member;
				CheckIdDirectiveUsage(domObject, memberNode3, xamlDirective);
				CheckModifierUsage(domObject, memberNode3, xamlDirective);
				if (xamlDirective == XamlLanguage.Name)
				{
					DirectUIXamlType directUIXamlType2 = (DirectUIXamlType)domObject.Type;
					if (directUIXamlType2.IsValueType)
					{
						Errors.Add(new XamlValidationErrorCannotNameValueTypes(domObject));
					}
					XamlDomMember memberNode2 = domObject.GetMemberNode("Name");
					if (memberNode2 != null && directUIXamlType2.GetAliasedProperty(XamlLanguage.Name) == memberNode2.Member)
					{
						Errors.Add(new XamlValidationErrorCannotNameElementTwice(domObject));
					}
					EnsureUniqueElementName(domObject, DomHelper.GetStringValueOfProperty(memberNode3));
				}
				else if (xamlDirective == XamlLanguage.Items)
				{
					ProcessItemsNode(domObject, memberNode3);
				}
			}
			else
			{
				ProcessNormalPropertyNode(memberNode3);
			}
		}
	}

	private void ValidateTypePresentInMinVersion(Type type, XamlDomObject domObject, XamlDomMember domMember)
	{
		if (_skipMinSdkValidation || domObject?.ApiInformation != null || domMember?.ApiInformation != null || _minVersionTypeCache.Contains(type.FullName))
		{
			return;
		}
		_minVersionTypeCache.Add(type.FullName);
		foreach (CustomAttributeData item in type.CustomAttributes.Where((CustomAttributeData a, int ind) => a.AttributeType.IsContractVersionAttribute()))
		{
			IList<CustomAttributeTypedArgument> constructorArguments = item.ConstructorArguments;
			string text = null;
			if (constructorArguments.Count < 2)
			{
				continue;
			}
			Type type2 = constructorArguments[0].Value as Type;
			text = ((!(type2 != null)) ? (constructorArguments[0].Value as string) : type2.FullName);
			Version version = ContractVersion.ToVersion((uint)constructorArguments[1].Value);
			Version value = null;
			if (_contractCache.TryGetValue(text, out value) && value < version)
			{
				if (domObject != null)
				{
					Warnings.Add(new XamlValidationErrorWrongContract(domObject, type.FullName, text, version.ToString(), value.ToString()));
				}
				else if (domMember != null)
				{
					Warnings.Add(new XamlValidationErrorWrongContract(domMember, type.FullName, text, version.ToString(), value.ToString()));
				}
			}
		}
	}

	private void ValidateMemberPresentInMinVersion(DirectUIXamlMember duiMember, XamlDomMember domMember)
	{
		if (_skipMinSdkValidation || domMember.ApiInformation != null || !(duiMember?.UnderlyingMember?.DeclaringType != null) || _minVersionMemberCache.Contains(Tuple.Create(duiMember.UnderlyingMember.DeclaringType.FullName, duiMember.UnderlyingMember.Name)))
		{
			return;
		}
		_minVersionMemberCache.Add(Tuple.Create(duiMember.UnderlyingMember.DeclaringType.FullName, duiMember.UnderlyingMember.Name));
		MemberInfo underlyingMember = duiMember.UnderlyingMember;
		Type declaringType = duiMember.UnderlyingMember.DeclaringType;
		Type type = null;
		Type[] interfaces = declaringType.GetInterfaces();
		foreach (Type type2 in interfaces)
		{
			if (type2.GetMember(underlyingMember.Name).Length != 0)
			{
				type = type2;
				break;
			}
		}
		if (type == null)
		{
			type = declaringType;
		}
		ValidateTypePresentInMinVersion(type, null, domMember);
	}

	private void EnsureUniqueElementName(XamlDomObject domObject, string name)
	{
		if (_namedElementsHash.IsNameAlreadyUsed(name))
		{
			Errors.Add(new XamlValidationErrorElementNameAlreadyUsed(domObject, name));
		}
		_namedElementsHash.AddNamedElement(name);
	}

	private void CheckDomObjectIsConstructibleOrTypeConvertable(XamlDomObject domObject)
	{
		if (!IsPass1)
		{
			XamlType type = domObject.Type;
			XamlDomMember memberNode = domObject.GetMemberNode(XamlLanguage.Initialization);
			if (memberNode == null && (!type.IsConstructible || type.ConstructionRequiresArguments))
			{
				Errors.Add(new XamlValidationErrorNotConstructibleObject(domObject, type));
			}
		}
	}

	private void ProcessGetObjectNode(XamlDomObject domObject)
	{
		if (IsPass1 || domObject.MemberNodes == null)
		{
			return;
		}
		foreach (XamlDomMember memberNode in domObject.MemberNodes)
		{
			if (memberNode.Member == XamlLanguage.Items)
			{
				ProcessItemsNode(domObject, memberNode);
			}
		}
	}

	private void ProcessClassName(XamlDomObject domRoot, XamlDomMember domMember, string className)
	{
		string[] array = className.Split('.');
		if (array.Length == 1)
		{
			Errors.Add(new XamlValidationErrorClassMustHaveANamespace(domMember, className));
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			int idx;
			if (string.IsNullOrWhiteSpace(text))
			{
				Errors.Add(new XamlValidationErrorClassNameEmptyPathPart(domMember, className));
			}
			else if (text.Contains(' ') || text.Contains('\t') || text.Contains('\n'))
			{
				Errors.Add(new XamlValidationErrorClassNameNoWhiteSpace(domMember, className));
			}
			else if (!IsValidIdentifierName(text, out idx))
			{
				Errors.Add(new XamlValidationErrorBadName(domMember, className, text[idx]));
			}
		}
		if (!IsPass1 && _schemaContext.DirectUISystem.IComponentConnector.IsAssignableFrom(domRoot.Type.UnderlyingType))
		{
			Warnings.Add(new XamlXClassDerivedFromXClassWarning(domRoot, className, domRoot.Type.UnderlyingType.FullName));
		}
	}

	private void ProcessNormalPropertyNode(XamlDomMember domMember)
	{
		if (domMember.Items.Count == 0)
		{
			return;
		}
		XamlMember member = domMember.Member;
		if (CheckIsUnresolvedForwardedType(domMember, out var errorMessage))
		{
			Errors.Add(new XamlValidationErrorUnresolvedForwardedTypeAssembly(domMember, errorMessage));
			return;
		}
		XamlDomValue xamlDomValue = domMember.Item as XamlDomValue;
		XamlDomObject xamlDomObject = domMember.Item as XamlDomObject;
		if (!IsPass1)
		{
			DirectUIXamlMember directUIXamlMember = (DirectUIXamlMember)domMember.Member;
			if (directUIXamlMember.IsDeprecated)
			{
				if (directUIXamlMember.IsHardDeprecated)
				{
					Errors.Add(new XamlValidationErrorDeprecated(domMember, directUIXamlMember.Name, directUIXamlMember.DeprecatedMessage));
				}
				else
				{
					Warnings.Add(new XamlValidationWarningDeprecated(domMember, directUIXamlMember.Name, directUIXamlMember.DeprecatedMessage));
				}
			}
			if (directUIXamlMember.IsExperimental)
			{
				Warnings.Add(new XamlValidationWarningExperimental(ErrorCode.WMC1501, domMember, directUIXamlMember.Name));
			}
			ValidateMemberPresentInMinVersion(directUIXamlMember, domMember);
		}
		DirectUIXamlType directUIXamlType = (DirectUIXamlType)domMember.Member.DeclaringType;
		if (directUIXamlType.IsCodeGenType)
		{
			CheckPropertyTypeForIllegalValueType(domMember);
		}
		if (xamlDomValue != null)
		{
			string text = (string)xamlDomValue.Value;
			CheckCanAssignTextToProperty(domMember, domMember.Member, text);
			if (!IsPass1 && domMember.Member.IsEvent)
			{
				CheckIsAmbiguousEvent(domMember.Parent, domMember);
			}
		}
		else if (!xamlDomObject.Type.IsUnknown)
		{
			ProcessNormalPropertyWithObjectChild(domMember, xamlDomObject);
		}
		if (DomHelper.IsBindExtension(domMember))
		{
			ProcessXBindPropertyNode(domMember);
		}
	}

	private void ProcessXBindPropertyNode(XamlDomMember domMember)
	{
		if (DomHelper.IsBindExtension(domMember.Parent))
		{
			Errors.Add(new XamlXBindInsideXBindError(domMember));
		}
		if (!DomHelper.IsDependencyProperty(domMember) && DomHelper.HasTwoWayBinding(domMember))
		{
			Errors.Add(new XamlXBindTwoWayBindingToANonDependencyPropertyError(domMember));
		}
		if (!_domRootHasCodeBehind)
		{
			Errors.Add(new XamlXBindWithoutCodeBehindError(domMember));
		}
		if (DomHelper.HasTargetNullValue(domMember) && !domMember.Member.IsUnknown && !domMember.Member.Type.IsNullable)
		{
			Errors.Add(new XamlXBindTargetNullValueOnNonNullableTypeError(domMember));
		}
		if (DomHelper.IsDerivedFromControlTemplate(domMember.Parent))
		{
			Errors.Add(new XamlXBindOnControlTemplateError(domMember));
		}
		if (!IsPass1 && !DomHelper.UnderANamescope(domMember.Parent))
		{
			XamlDomObject domRoot = DomHelper.GetDomRoot(domMember);
			if (!_schemaContext.DirectUISystem.FrameworkElement.IsAssignableFrom(domRoot.Type.UnderlyingType) && !_schemaContext.DirectUISystem.Window.IsAssignableFrom(domRoot.Type.UnderlyingType))
			{
				Errors.Add(new XamlXBindRootNoLoadingEvent(domMember, domRoot.Type.UnderlyingType.FullName));
			}
		}
	}

	private bool CheckIsAmbiguousEvent(XamlDomObject domObject, XamlDomMember domMember)
	{
		return true;
	}

	private bool CheckCanAssignTextToProperty(XamlDomNode locationForErrors, XamlMember property, string text)
	{
		XamlType type = property.Type;
		string item = property.UnderlyingMember?.DeclaringType?.Name;
		string name = property.Name;
		if (type.IsCollection)
		{
			XamlType itemType = type.ItemType;
			if (type.TypeConverter == null && !type.HasCreateFromStringMethod() && (itemType.TypeConverter != null || itemType.HasCreateFromStringMethod()))
			{
				return SuccinctCollectionSyntaxVerifier.TryParse(text, locationForErrors, Errors, property);
			}
		}
		if (property.IsReadOnly)
		{
			Errors.Add(new XamlValidationErrorCannotAssignToReadOnlyProperty(locationForErrors, property, text));
			return false;
		}
		if (property.IsEvent)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				Errors.Add(new XamlValidationErrorEventValuesMustBeText(locationForErrors, property.Name));
				return false;
			}
			return true;
		}
		if (type == _schemaContext.DirectUIXamlLanguage.String || type == _schemaContext.DirectUIXamlLanguage.Object)
		{
			return true;
		}
		if (type.TypeConverter == null && !type.HasCreateFromStringMethod())
		{
			Errors.Add(new XamlValidationErrorCannotAssignTextToProperty(locationForErrors, property, text));
			return false;
		}
		if (type.HasCreateFromStringMethod())
		{
			XamlCompileError xamlCompileError = _schemaContext.EnsureCreateFromStringResolved(type.Name, type.GetCreateFromStringMethod(), locationForErrors);
			if (xamlCompileError != null)
			{
				Errors.Add(xamlCompileError);
				return false;
			}
		}
		if (type.IsEnum())
		{
			string[] array = text.Split(',');
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				string text3 = text2.Trim();
				UnsupportedEnumValues.TryGetValue((item, name), out var value);
				if ((!type.GetEnumNames().Contains(text3, StringComparer.OrdinalIgnoreCase) || (value != null && value.Contains(text3, StringComparer.OrdinalIgnoreCase))) && (!int.TryParse(text3, out var result) || (value != null && value.Contains(result.ToString()))))
				{
					Errors.Add(new XamlValidationErrorCannotAssignTextToProperty(locationForErrors, property, text));
					return false;
				}
			}
		}
		return true;
	}

	private void ProcessNormalPropertyWithObjectChild(XamlDomMember domMember, XamlDomObject domChildObject)
	{
		XamlMember member = domMember.Member;
		if (domChildObject.IsGetObject)
		{
			return;
		}
		if (member.IsReadOnly)
		{
			Errors.Add(new XamlValidationErrorCannotAssignToReadOnlyProperty(domMember));
			return;
		}
		XamlType type = member.Type;
		if (!domChildObject.Type.CanAssignTo(type) && !domChildObject.Type.IsMarkupExtension)
		{
			Errors.Add(new XamlValidationErrorAssignment(domChildObject, member, type));
		}
	}

	private void ProcessItemsNode(XamlDomObject domCollectionObject, XamlDomMember domItemsMember)
	{
		if (!IsPass1)
		{
			XamlType type = domCollectionObject.Type;
			if (domCollectionObject.Type.IsCollection)
			{
				ProcessCollectionItemsNode(domCollectionObject, domItemsMember);
				ProcessSpecialCollections(domCollectionObject, domItemsMember);
			}
			else if (domCollectionObject.Type.IsDictionary)
			{
				ProcessDictionaryItemsNode(domCollectionObject, domItemsMember);
			}
		}
	}

	private void ProcessCollectionItemsNode(XamlDomObject domCollectionObject, XamlDomMember domItemsMember)
	{
		XamlDomMember parent = domCollectionObject.Parent;
		XamlType type = parent.Member.Type;
		XamlType itemType = domCollectionObject.Type.ItemType;
		foreach (XamlDomItem item in domItemsMember.Items)
		{
			XamlDomValue xamlDomValue = item as XamlDomValue;
			XamlDomObject xamlDomObject = item as XamlDomObject;
			XamlDomItem xamlDomItem = null;
			if (xamlDomValue != null)
			{
				xamlDomItem = xamlDomValue;
				foreach (XamlType allowedContentType in type.AllowedContentTypes)
				{
					if (allowedContentType == _schemaContext.DirectUIXamlLanguage.String || allowedContentType == _schemaContext.DirectUIXamlLanguage.Object)
					{
						xamlDomItem = null;
						break;
					}
				}
			}
			else if (!xamlDomObject.Type.CanAssignTo(itemType) && !xamlDomObject.Type.IsMarkupExtension)
			{
				xamlDomItem = xamlDomObject;
			}
			if (xamlDomItem != null && (xamlDomObject != xamlDomItem || !xamlDomObject.Type.IsUnknown))
			{
				Errors.Add(new XamlValidationErrorCollectionAdd(xamlDomItem, itemType, domCollectionObject, parent));
			}
		}
	}

	private void ProcessSpecialCollections(XamlDomObject domCollectionObject, XamlDomMember domItemsMember)
	{
		XamlDomMember parent = domCollectionObject.Parent;
		if (parent != null)
		{
			XamlDomObject parent2 = parent.Parent;
			XamlType type = parent2.Type;
			if (_schemaContext.DirectUISystem.Style.IsAssignableFrom(type.UnderlyingType))
			{
				ProcessStyleSetterCollection(parent2, domItemsMember);
			}
		}
	}

	private void ProcessStyleSetterCollection(XamlDomObject domStyleObject, XamlDomMember domItemsMember)
	{
		XamlType andResolveStyleTargetTypeProperty = GetAndResolveStyleTargetTypeProperty(domStyleObject);
		if (andResolveStyleTargetTypeProperty == null)
		{
			return;
		}
		foreach (XamlDomItem item in domItemsMember.Items)
		{
			if (item is XamlDomObject xamlDomObject && !xamlDomObject.Type.IsUnknown && xamlDomObject.Type.UnderlyingType.IsAssignableFrom(_schemaContext.DirectUISystem.Setter))
			{
				ProcessSingleSetter(andResolveStyleTargetTypeProperty, xamlDomObject);
			}
		}
	}

	private void ProcessSingleSetter(XamlType xamlTargetType, XamlDomObject domSetterObject)
	{
		XamlDomMember memberNode = domSetterObject.GetMemberNode("Property");
		if (memberNode == null)
		{
			memberNode = domSetterObject.GetMemberNode("Target");
		}
		if (memberNode == null)
		{
			Errors.Add(new XamlValidationErrorSetterMissingField(domSetterObject, isProperty: true));
			return;
		}
		string stringValueOfProperty = DomHelper.GetStringValueOfProperty(memberNode);
		if (stringValueOfProperty == null)
		{
			Errors.Add(new XamlValidationErrorSetterMissingField(memberNode, isProperty: true));
			return;
		}
		XamlMember xamlMember = domSetterObject.ResolveMemberName(xamlTargetType, stringValueOfProperty);
		if (xamlMember == null)
		{
			Errors.Add(new XamlValidationErrorSetterUnknownMember(memberNode, xamlTargetType, stringValueOfProperty));
			return;
		}
		DirectUIXamlMember directUIXamlMember = xamlMember as DirectUIXamlMember;
		if (directUIXamlMember != null && !directUIXamlMember.IsAttachable && !directUIXamlMember.IsDependencyProperty)
		{
			Errors.Add(new XamlValidationErrorSetterSetterPropertyMustBeDP(memberNode, stringValueOfProperty));
		}
		XamlDomMember memberNode2 = domSetterObject.GetMemberNode("Value");
		if (memberNode2 == null)
		{
			Errors.Add(new XamlValidationErrorSetterMissingField(domSetterObject, isProperty: false));
			return;
		}
		if (memberNode2.Item == null)
		{
			Errors.Add(new XamlValidationErrorSetterMissingField(memberNode2, isProperty: false));
			return;
		}
		XamlDomValue xamlDomValue = memberNode2.Item as XamlDomValue;
		XamlDomObject xamlDomObject = memberNode2.Item as XamlDomObject;
		if (xamlDomValue != null)
		{
			string text = (string)xamlDomValue.Value;
			CheckCanAssignTextToProperty(xamlDomValue, xamlMember, text);
			return;
		}
		XamlType type = xamlDomObject.Type;
		if (!type.CanAssignTo(xamlMember.Type) && !type.IsMarkupExtension)
		{
			Errors.Add(new XamlValidationErrorAssignment(xamlDomObject, xamlMember, xamlMember.Type));
		}
	}

	private XamlType GetAndResolveStyleTargetTypeProperty(XamlDomObject domStyleObject, bool reportErrors = false)
	{
		XamlDomMember memberNode = domStyleObject.GetMemberNode("TargetType");
		if (memberNode == null)
		{
			if (reportErrors)
			{
				Errors.Add(new XamlValidationErrorStyleMustHaveTargetType(domStyleObject));
			}
			return null;
		}
		string stringValueOfProperty = DomHelper.GetStringValueOfProperty(memberNode);
		if (stringValueOfProperty == null)
		{
			if (reportErrors)
			{
				Errors.Add(new XamlValidationErrorStyleMustHaveTargetType(memberNode));
			}
			return null;
		}
		XamlType xamlType = domStyleObject.ResolveXmlName(stringValueOfProperty);
		if (xamlType == null)
		{
			if (reportErrors)
			{
				Errors.Add(new XamlValidationErrorUnknownStyleTargetType(memberNode, stringValueOfProperty));
			}
			return null;
		}
		return xamlType;
	}

	private void CheckBasedOnIsCorrect(XamlDomObject domStyleObject, XamlType xamlTargetType, XamlDomMember domBasedOnMember)
	{
		XamlDomObject xamlDomObject = domBasedOnMember.Item as XamlDomObject;
		XamlDomObject xamlDomObject2 = null;
		string text = null;
		string otherFile = null;
		if (xamlDomObject == null)
		{
			string text2 = ((!(domBasedOnMember.Item is XamlDomValue xamlDomValue)) ? string.Empty : (xamlDomValue.Value as string));
			Errors.Add(new XamlValidationErrorStyleBasedOnMustBeStyle(domStyleObject, text2));
		}
		else
		{
			if (xamlDomObject.Type.CanAssignTo(_schemaContext.DirectUIXamlLanguage.NullExtension) || xamlDomObject.Type.CanAssignTo(_schemaContext.DirectUIXamlLanguage.CustomResourceExtension))
			{
				return;
			}
			if (!_schemaContext.DirectUISystem.Style.IsAssignableFrom(xamlDomObject.Type.UnderlyingType))
			{
				xamlDomObject2 = xamlDomObject;
				if (!xamlDomObject2.Type.CanAssignTo(_schemaContext.DirectUIXamlLanguage.StaticResourceExtension))
				{
					Errors.Add(new XamlValidationErrorStyleBasedOnMustBeStyle(domStyleObject, xamlDomObject2));
					return;
				}
				text = DomHelper.GetStaticResource_ResourceKey(xamlDomObject2);
				if (text == null)
				{
					return;
				}
				xamlDomObject = ResolveStaticResource(domStyleObject, text, out otherFile);
				if (xamlDomObject == null)
				{
					return;
				}
			}
			if (!_schemaContext.DirectUISystem.Style.IsAssignableFrom(xamlDomObject.Type.UnderlyingType))
			{
				if (text == null)
				{
					Errors.Add(new XamlValidationErrorStyleBasedOnMustBeStyle(domStyleObject, xamlDomObject));
				}
				else
				{
					Errors.Add(new XamlValidationErrorStyleBasedOnMustBeStyle(domStyleObject, text, xamlDomObject, otherFile));
				}
				return;
			}
			XamlType andResolveStyleTargetTypeProperty = GetAndResolveStyleTargetTypeProperty(xamlDomObject);
			if (!(andResolveStyleTargetTypeProperty == null) && !xamlTargetType.CanAssignTo(andResolveStyleTargetTypeProperty))
			{
				Errors.Add(new XamlValidationErrorStyleBasedOnBadStyleTargetType(domStyleObject, andResolveStyleTargetTypeProperty, xamlTargetType));
			}
		}
	}

	private void ProcessDictionaryItemsNode(XamlDomObject domDictionaryObject, XamlDomMember domItemsMember)
	{
		XamlDomMember parent = domDictionaryObject.Parent;
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (XamlDomItem item in domItemsMember.Items)
		{
			XamlDomValue xamlDomValue = item as XamlDomValue;
			XamlDomObject xamlDomObject = item as XamlDomObject;
			XamlType itemType = domDictionaryObject.Type.ItemType;
			if (xamlDomValue != null)
			{
				Errors.Add(new XamlValidationErrorDictionaryAdd(xamlDomValue));
			}
			else
			{
				if (CheckIsUnresolvedForwardedType(xamlDomObject, out var _))
				{
					continue;
				}
				XamlType type = xamlDomObject.Type;
				if (!type.IsUnknown && !type.CanAssignTo(itemType) && !type.IsMarkupExtension)
				{
					Errors.Add(new XamlValidationErrorDictionaryAdd(xamlDomObject, itemType, domDictionaryObject, parent));
				}
				XamlDomMember xamlDomMember = xamlDomObject.GetMemberNode(XamlLanguage.Key);
				if (xamlDomMember == null || xamlDomMember.Member != XamlLanguage.Key)
				{
					XamlDomMember memberNode = xamlDomObject.GetMemberNode(XamlLanguage.Name);
					if (memberNode != null)
					{
						xamlDomMember = memberNode;
					}
				}
				if (xamlDomMember == null)
				{
					Errors.Add(new XamlValidationDictionaryKeyError(xamlDomObject));
					continue;
				}
				string text = DomHelper.GetStringValueOfProperty(xamlDomMember);
				if (string.IsNullOrWhiteSpace(text) || xamlDomObject.ApiInformation != null)
				{
					continue;
				}
				List<string> list3;
				if (xamlDomMember.Member.Name == "TargetType")
				{
					list3 = list2;
					XamlType xamlType = xamlDomObject.ResolveXmlName(text);
					if (xamlType != null)
					{
						text = xamlType.UnderlyingType.FullName;
					}
				}
				else
				{
					list3 = list;
				}
				if (list3.Contains(text))
				{
					Errors.Add(new XamlValidationDictionaryKeyError(xamlDomObject, text));
					break;
				}
				list3.Add(text);
			}
		}
	}

	private void ProcessUnknownMemberNode(XamlDomObject domObject, XamlDomMember domMember)
	{
		XamlMember member = domMember.Member;
		if (member.IsDirective)
		{
			XamlDirective xamlDirective = member as XamlDirective;
			if (xamlDirective == XamlLanguage.UnknownContent)
			{
				Errors.Add(new XamlValidationErrorMissingCPA(domObject, domMember.Items[0]));
			}
			else
			{
				if (DomHelper.IsPropertiesMember(domMember))
				{
					return;
				}
				if (DomHelper.IsDefaultBindModeMember(domMember))
				{
					string stringValueOfProperty = DomHelper.GetStringValueOfProperty(domMember);
					if (!Enum.TryParse<DefaultBindMode>(stringValueOfProperty, ignoreCase: true, out var _))
					{
						Errors.Add(new XamlValidationError_DefaultBindModeInvalidValue(domMember));
					}
					return;
				}
				if (DomHelper.IsDeferLoadStrategyMember(domMember))
				{
					string stringValueOfProperty2 = DomHelper.GetStringValueOfProperty(domMember);
					if (!Enum.TryParse<DeferLoadStrategy>(stringValueOfProperty2, ignoreCase: false, out var _))
					{
						Errors.Add(new XamlValidationError_DeferLoadStrategyInvalidValue(domMember));
					}
					if (DomHelper.GetAliasedMemberNode(domObject, XamlLanguage.Name, forcePass1Eval: true) == null)
					{
						Errors.Add(new XamlValidationError_DeferLoadStrategyMissingXName(domObject));
					}
					if (!CanHaveDeferLoadStrategyOrLoad(domObject))
					{
						Errors.Add(new XamlValidationError_CannotHaveDeferLoadStrategy(domMember));
					}
					return;
				}
				if (DomHelper.IsLoadMember(domMember))
				{
					string stringValueOfProperty3 = DomHelper.GetStringValueOfProperty(domMember);
					bool result3 = false;
					if (!bool.TryParse(stringValueOfProperty3, out result3))
					{
						if (DomHelper.IsBindExtension(domMember))
						{
							ProcessXBindPropertyNode(domMember);
						}
						else
						{
							Errors.Add(new XamlValidationError_LoadInvalidValue(domMember));
						}
					}
					if (DomHelper.GetAliasedMemberNode(domObject, XamlLanguage.Name, forcePass1Eval: true) == null)
					{
						Errors.Add(new XamlValidationError_LoadMissingName(domObject));
					}
					if (!CanHaveDeferLoadStrategyOrLoad(domObject))
					{
						Errors.Add(new XamlValidationError_LoadNotSupported(domMember));
					}
					return;
				}
				if (DomHelper.IsDataTypeMember(domMember))
				{
					if (!IsPass1)
					{
						string stringValueOfProperty4 = DomHelper.GetStringValueOfProperty(domMember);
						if (!string.IsNullOrEmpty(stringValueOfProperty4) && domObject.ResolveXmlName(stringValueOfProperty4) == null)
						{
							Errors.Add(new XamlValidationError_CantResolveDataType(domObject, stringValueOfProperty4));
						}
					}
					return;
				}
				if (DomHelper.IsPhaseMember(domMember))
				{
					string stringValueOfProperty5 = DomHelper.GetStringValueOfProperty(domMember);
					if (!string.IsNullOrEmpty(stringValueOfProperty5))
					{
						int result4 = 0;
						if (!int.TryParse(stringValueOfProperty5, out result4) || result4 < 0 || result4 > 24)
						{
							Errors.Add(new XamlValidationError_InvalidValueForPhase(domObject));
						}
					}
					if (!DomHelper.DoesAnyMemberUseBindExpression(domObject))
					{
						Errors.Add(new XamlValidationError_PhaseCanBeUsedOnlyWithBind(domObject));
					}
					if (!DomHelper.UnderANamescope(domObject))
					{
						Errors.Add(new XamlValidationError_PhaseOnlyAllowedInDataTemplate(domObject));
					}
					return;
				}
				if (DomHelper.IsSuppressXamlTrimWarningsMember(domMember))
				{
					string stringValueOfProperty6 = DomHelper.GetStringValueOfProperty(domMember);
					if (!bool.TryParse(stringValueOfProperty6, out var _))
					{
						Errors.Add(new XamlValidationError_InvalidValueForSuppressXamlTrimWarnings(domObject));
					}
					return;
				}
			}
		}
		if (IsPass1)
		{
			XamlType declaringType = member.DeclaringType;
			if (declaringType != null && declaringType.IsUnknown && IsPossiblyALocalType(declaringType))
			{
				return;
			}
		}
		XamlMember member2;
		if (member.IsAttachable)
		{
			Errors.Add(new XamlValidationErrorUnknownMember(domObject, domMember));
		}
		else if (TryFindPropertyInSchema(member, out member2) && !member2.IsUnknown && member2.IsReadOnly && domMember.Items.Count != 0)
		{
			Errors.Add(new XamlValidationErrorCannotAssignToReadOnlyProperty(domMember));
		}
		else
		{
			Errors.Add(new XamlValidationErrorUnknownMember(domObject, domMember));
		}
	}

	private bool IsPossiblyALocalType(XamlType xamlType)
	{
		string usingTypePath;
		return XamlHarvester.IsPossiblyALocalType(xamlType, out usingTypePath);
	}

	private bool TryFindPropertyInSchema(XamlMember unknownMember, out XamlMember member)
	{
		DirectUIXamlType directUIXamlType = unknownMember.DeclaringType as DirectUIXamlType;
		member = null;
		if (directUIXamlType != null)
		{
			member = directUIXamlType.LookupMember_SkipReadOnlyCheck(unknownMember.Name);
		}
		return member != null;
	}

	private void CheckPropertyTypeForIllegalValueType(XamlDomMember domMember)
	{
		if (domMember.Member.IsUnknown || domMember.Member.IsEvent)
		{
			return;
		}
		DirectUIXamlType directUIXamlType = (DirectUIXamlType)domMember.Member.Type;
		if (directUIXamlType.IsInvalidType)
		{
			if (directUIXamlType.IsSignedChar)
			{
				Errors.Add(new XamlCompileErrorInvalidPropertyType_SignedChar(domMember));
			}
			else
			{
				Errors.Add(new XamlCompileErrorInvalidPropertyType(domMember));
			}
		}
	}

	private bool HasStringAttribute(XamlDomObject domObject, XamlMember member)
	{
		string value;
		XamlDomMember domMember;
		return TryGetStringAttribute(domObject, member, out value, out domMember);
	}

	private bool TryGetStringAttribute(XamlDomObject domObject, XamlMember member, out string value, out XamlDomMember domMember)
	{
		value = null;
		domMember = domObject.GetMemberNode(member);
		if (domMember == null)
		{
			return false;
		}
		value = DomHelper.GetStringValueOfProperty(domMember);
		return !string.IsNullOrWhiteSpace(value);
	}

	private void CheckIdDirectiveUsage(XamlDomObject domObject, XamlDomMember domMember, XamlDirective xamlDirective)
	{
		if (!(xamlDirective == XamlLanguage.Uid) && !(xamlDirective == XamlLanguage.Name) && !(xamlDirective == XamlLanguage.Key))
		{
			return;
		}
		string stringValueOfProperty = DomHelper.GetStringValueOfProperty(domMember);
		if (string.IsNullOrWhiteSpace(stringValueOfProperty))
		{
			Errors.Add(new XamlValidationIdPropertiesMustBeText(domMember));
		}
		else if (xamlDirective == XamlLanguage.Name)
		{
			if (!IsValidIdentifierName(stringValueOfProperty, out var idx))
			{
				Errors.Add(new XamlValidationErrorBadName(domMember, stringValueOfProperty, stringValueOfProperty[idx]));
			}
		}
		else if (xamlDirective == XamlLanguage.Key && !IsValidKeyIdentifierName(stringValueOfProperty))
		{
			Errors.Add(new XamlValidationErrorBadName(domMember, stringValueOfProperty));
		}
	}

	private void CheckModifierUsage(XamlDomObject domObject, XamlDomMember domMember, XamlDirective xamlDirective)
	{
		if (xamlDirective == XamlLanguage.FieldModifier)
		{
			XamlDomMember memberNode = domObject.GetMemberNode(XamlLanguage.FieldModifier);
			string text = DomHelper.GetStringValueOfProperty(memberNode).ToLower();
			if (!IsValidModifier(text))
			{
				Errors.Add(new XamlValidationErrorInvalidFieldModifier(domObject, text));
			}
		}
	}

	private XamlDomObject ResolveStaticResource(XamlDomObject domObject, string keyString, out string otherFile)
	{
		otherFile = null;
		XamlDomObject xamlDomObject = null;
		XamlDomObject xamlDomObject2 = domObject;
		Type frameworkElement = _schemaContext.DirectUISystem.FrameworkElement;
		XamlDomObject searchLimit = null;
		while (xamlDomObject2 != null)
		{
			Type underlyingType = xamlDomObject2.Type.UnderlyingType;
			if (frameworkElement.IsAssignableFrom(underlyingType))
			{
				XamlDomMember memberNode = xamlDomObject2.GetMemberNode("Resources");
				if (memberNode != null)
				{
					xamlDomObject = FindKeyInResources(keyString, memberNode, searchLimit);
					if (xamlDomObject != null)
					{
						break;
					}
				}
			}
			XamlDomMember parent = xamlDomObject2.Parent;
			if (parent == null)
			{
				break;
			}
			if (IsAnItemInAResourceDictionary(xamlDomObject2))
			{
				searchLimit = xamlDomObject2;
			}
			xamlDomObject2 = parent.Parent;
		}
		return xamlDomObject;
	}

	private bool IsAnItemInAResourceDictionary(XamlDomObject item)
	{
		XamlDomMember parent = item.Parent;
		if (parent != null && parent.Member == XamlLanguage.Items)
		{
			XamlDomObject parent2 = parent.Parent;
			Type resourceDictionary = _schemaContext.DirectUISystem.ResourceDictionary;
			if (parent2 != null && resourceDictionary.IsAssignableFrom(parent2.Type.UnderlyingType))
			{
				return true;
			}
		}
		return false;
	}

	private XamlDomObject FindKeyInResources(string keyString, XamlDomMember resources, XamlDomObject searchLimit)
	{
		if (resources.Item is XamlDomObject xamlDomObject)
		{
			XamlDomMember memberNode = xamlDomObject.GetMemberNode(XamlLanguage.Items);
			if (memberNode != null && memberNode.Items.Count > 0)
			{
				foreach (XamlDomItem item in memberNode.Items)
				{
					if (!(item is XamlDomObject xamlDomObject2))
					{
						continue;
					}
					if (xamlDomObject2 == searchLimit)
					{
						return null;
					}
					XamlDomMember memberNode2 = xamlDomObject2.GetMemberNode(XamlLanguage.Key, allowPropertyAliasing: false);
					if (memberNode2 == null)
					{
						memberNode2 = xamlDomObject2.GetMemberNode(XamlLanguage.Name, allowPropertyAliasing: false);
					}
					if (memberNode2 != null)
					{
						string stringValueOfProperty = DomHelper.GetStringValueOfProperty(memberNode2);
						if (string.Equals(stringValueOfProperty, keyString, StringComparison.Ordinal))
						{
							return xamlDomObject2;
						}
					}
				}
			}
		}
		return null;
	}

	public static bool IsValidIdentifierName(string name)
	{
		int idx;
		return IsValidIdentifierName(name, out idx);
	}

	public static bool IsValidIdentifierName(string name, out int idx)
	{
		idx = 0;
		if (name == null)
		{
			return false;
		}
		for (int i = 0; i < name.Length; i++)
		{
			UnicodeCategory unicodeCategory = char.GetUnicodeCategory(name[i]);
			bool flag = unicodeCategory == UnicodeCategory.UppercaseLetter || unicodeCategory == UnicodeCategory.LowercaseLetter || unicodeCategory == UnicodeCategory.TitlecaseLetter || unicodeCategory == UnicodeCategory.OtherLetter || unicodeCategory == UnicodeCategory.LetterNumber || name[i] == '_';
			bool flag2 = unicodeCategory == UnicodeCategory.NonSpacingMark || unicodeCategory == UnicodeCategory.SpacingCombiningMark || unicodeCategory == UnicodeCategory.ModifierLetter || unicodeCategory == UnicodeCategory.DecimalDigitNumber;
			if (i == 0)
			{
				if (!flag)
				{
					idx = i;
					return false;
				}
			}
			else if (!(flag || flag2))
			{
				idx = i;
				return false;
			}
		}
		return true;
	}

	public static bool IsValidKeyIdentifierName(string name)
	{
		if (name == null)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidModifier(string modifierValue)
	{
		bool result = false;
		if (string.Compare(modifierValue, "private", StringComparison.OrdinalIgnoreCase) == 0)
		{
			result = true;
		}
		else if (string.Compare(modifierValue, "public", StringComparison.OrdinalIgnoreCase) == 0)
		{
			result = true;
		}
		else if (string.Compare(modifierValue, "protected", StringComparison.OrdinalIgnoreCase) == 0)
		{
			result = true;
		}
		else if (string.Compare(modifierValue, "internal", StringComparison.OrdinalIgnoreCase) == 0)
		{
			result = true;
		}
		else if (string.Compare(modifierValue, "friend", StringComparison.OrdinalIgnoreCase) == 0)
		{
			result = true;
		}
		return result;
	}

	public bool CanHaveDeferLoadStrategyOrLoad(XamlDomObject domObject)
	{
		if (domObject.Parent != null && DomHelper.IsDerivedFromResourceDictionary(domObject.Parent.Parent))
		{
			return false;
		}
		if (domObject.Parent?.Parent == null)
		{
			return false;
		}
		if (domObject.Parent != null && DomHelper.IsDerivedFromDataTemplate(domObject.Parent.Parent))
		{
			return false;
		}
		if (!DomHelper.IsDerivedFromUIElement(domObject) && !DomHelper.IsDerivedFromFlyoutBase(domObject))
		{
			return false;
		}
		return true;
	}

	private void ValidateNamespaces(IEnumerable<XamlDomNamespace> namespacesToValidate)
	{
		foreach (XamlDomNamespace item in namespacesToValidate)
		{
			string text = item.NamespaceDeclaration.Namespace;
			if (text.IsConditionalNamespace())
			{
				try
				{
					ConditionalNamespace.Parse(text);
				}
				catch (ParseException ex)
				{
					Errors.Add(new XamlValidationConditionalNamespaceError(text, ex.Message, item));
				}
			}
		}
	}
}
