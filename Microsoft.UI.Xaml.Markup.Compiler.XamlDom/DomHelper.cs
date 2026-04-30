using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xaml;
using System.Xml;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

internal class DomHelper
{
	public static IEnumerable<XamlDomNode> DomNodeAncestry(XamlDomNode node)
	{
		XamlDomMember currentMember = node as XamlDomMember;
		XamlDomObject currentObject = node as XamlDomObject;
		while (currentMember != null || currentObject != null)
		{
			if (currentMember != null)
			{
				currentObject = currentMember.Parent;
				currentMember = null;
				yield return currentObject;
				continue;
			}
			currentMember = currentObject.Parent;
			currentObject = null;
			if (currentMember != null)
			{
				yield return currentMember;
				continue;
			}
			break;
		}
	}

	public static string GetStringValueOfProperty(XamlDomObject domObject, string memberName)
	{
		return GetStringValueOfProperty(domObject.GetMemberNode(memberName));
	}

	public static string GetStringValueOfProperty(XamlDomObject domObject, XamlMember member)
	{
		return GetStringValueOfProperty(domObject.GetMemberNode(member));
	}

	public static string GetStringValueOfProperty(XamlDomMember domMember)
	{
		return ((domMember?.Item is XamlDomValue xamlDomValue) ? xamlDomValue.Value : null) as string;
	}

	public static XamlDomObject GetDomRoot(XamlDomNode node)
	{
		if (node == null)
		{
			return null;
		}
		XamlDomMember xamlDomMember = node as XamlDomMember;
		XamlDomObject xamlDomObject = node as XamlDomObject;
		while (xamlDomMember != null || xamlDomObject != null)
		{
			if (xamlDomMember != null)
			{
				xamlDomObject = xamlDomMember.Parent;
				xamlDomMember = null;
				continue;
			}
			XamlDomMember parent = xamlDomObject.Parent;
			if (parent == null)
			{
				return xamlDomObject;
			}
			xamlDomMember = parent;
			xamlDomObject = null;
		}
		return null;
	}

	public static bool IsLocalType(XamlType xamlType)
	{
		if (xamlType == null)
		{
			return false;
		}
		if (xamlType.UnderlyingType == null)
		{
			return true;
		}
		DirectUIAssembly directUIAssembly = xamlType.UnderlyingType.Assembly as DirectUIAssembly;
		if (directUIAssembly == null)
		{
			directUIAssembly = DirectUIAssembly.Wrap(xamlType.UnderlyingType.Assembly);
		}
		if (!(xamlType.SchemaContext is DirectUISchemaContext directUISchemaContext))
		{
			return false;
		}
		return directUISchemaContext.IsLocalAssembly(directUIAssembly);
	}

	public static XamlDomMember GetAliasedMemberNode(XamlDomObject domObject, XamlDirective directive, bool forcePass1Eval = false)
	{
		if (!directive.IsDirective)
		{
			throw new ArgumentException(XamlCompilerResources.DuiSchema_ArgumentNotXamlDirective, "directive");
		}
		XamlDomMember memberNode = domObject.GetMemberNode(directive, !forcePass1Eval);
		if (memberNode != null)
		{
			return memberNode;
		}
		if (forcePass1Eval && IsLocalType(domObject.Type))
		{
			return null;
		}
		XamlMember aliasedProperty = domObject.Type.GetAliasedProperty(directive);
		if (aliasedProperty == null)
		{
			return null;
		}
		return domObject.GetMemberNode(aliasedProperty);
	}

	public static string SaveToString(XamlDomObject rootObjectNode)
	{
		XamlSchemaContext schemaContext = rootObjectNode.Type.SchemaContext;
		XamlDomReader xamlReader = new XamlDomReader(rootObjectNode, schemaContext);
		StringWriter stringWriter = new StringWriter();
		using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
		{
			xmlTextWriter.Formatting = Formatting.Indented;
			xmlTextWriter.Indentation = 2;
			XamlXmlWriter xamlWriter = new XamlXmlWriter(xmlTextWriter, schemaContext);
			XamlServices.Transform(xamlReader, xamlWriter);
		}
		return stringWriter.ToString();
	}

	internal static bool UnderANamescope(XamlDomObject namedObject, bool isPass1 = true)
	{
		foreach (XamlDomNode item in DomNodeAncestry(namedObject))
		{
			XamlDomMember xamlDomMember = item as XamlDomMember;
			XamlDomObject xamlDomObject = item as XamlDomObject;
			XamlType xamlType = null;
			if (xamlDomMember == null)
			{
				xamlType = xamlDomObject.Type;
				if (xamlType == null)
				{
					continue;
				}
			}
			else
			{
				if (xamlDomMember.Member.IsDirective || xamlDomMember.Member.IsUnknown)
				{
					continue;
				}
				XamlMember member = xamlDomMember.Member;
				xamlType = member.Type;
			}
			if (xamlType.IsNameScope)
			{
				return true;
			}
			if (xamlType.DeferringLoader != null)
			{
				return true;
			}
			if (!(IsLocalType(xamlType) && isPass1) && xamlType.IsDerivedFromFrameworkTemplate())
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsInsideControlTemplate(XamlDomObject namedObject, bool isPass1 = false)
	{
		foreach (XamlDomNode item in DomNodeAncestry(namedObject))
		{
			XamlDomMember xamlDomMember = item as XamlDomMember;
			XamlDomObject xamlDomObject = item as XamlDomObject;
			XamlType xamlType = null;
			if (xamlDomMember == null)
			{
				xamlType = xamlDomObject.Type;
				if (xamlType == null)
				{
					continue;
				}
			}
			else
			{
				if (xamlDomMember.Member.IsDirective || xamlDomMember.Member.IsUnknown)
				{
					continue;
				}
				XamlMember member = xamlDomMember.Member;
				xamlType = member.Type;
			}
			if (xamlType.IsDerivedFromDataTemplate())
			{
				return false;
			}
			if (xamlType.IsDerivedFromControlTemplate())
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsDerivedFromDataTemplate(XamlDomObject domObject)
	{
		if (!domObject.Type.IsUnknown)
		{
			return domObject.Type.IsDerivedFromDataTemplate();
		}
		return false;
	}

	public static bool IsDerivedFromControlTemplate(XamlDomObject domObject)
	{
		if (!domObject.Type.IsUnknown)
		{
			return domObject.Type.IsDerivedFromControlTemplate();
		}
		return false;
	}

	public static bool IsDerivedFromResourceDictionary(XamlDomObject domObject)
	{
		if (!domObject.Type.IsUnknown)
		{
			return domObject.Type.IsDerivedFromResourceDictionary();
		}
		return false;
	}

	public static bool IsDerivedFromUIElement(XamlDomObject domObject)
	{
		if (!domObject.Type.IsUnknown)
		{
			return domObject.Type.IsDerivedFromUIElement();
		}
		return false;
	}

	public static bool IsDerivedFromFlyoutBase(XamlDomObject domObject)
	{
		if (!domObject.Type.IsUnknown)
		{
			return domObject.Type.IsDerivedFromFlyoutBase();
		}
		return false;
	}

	public static bool IsNamedDirective(XamlDomMember domMember, string directiveName)
	{
		XamlMember member = domMember.Member;
		if (member.IsDirective)
		{
			XamlDirective xamlDirective = member as XamlDirective;
			if (xamlDirective.Name.Equals(directiveName, StringComparison.InvariantCulture))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsDeferLoadStrategyMember(XamlDomMember domMember)
	{
		return IsNamedDirective(domMember, "DeferLoadStrategy");
	}

	public static bool IsLoadMember(XamlDomMember domMember)
	{
		return IsNamedDirective(domMember, "Load");
	}

	public static bool IsPropertiesMember(XamlDomMember domMember)
	{
		return IsNamedDirective(domMember, "Properties");
	}

	public static bool IsDefaultBindModeMember(XamlDomMember domMember)
	{
		return IsNamedDirective(domMember, "DefaultBindMode");
	}

	public static bool IsDataTypeMember(XamlMember xamlMember, bool checkForDirectiveOnly = false)
	{
		if (xamlMember.IsDirective)
		{
			XamlDirective xamlDirective = xamlMember as XamlDirective;
			if (xamlDirective.Name.Equals("DataType", StringComparison.InvariantCulture))
			{
				return true;
			}
		}
		else if (!checkForDirectiveOnly && xamlMember.Name.Equals("TargetType", StringComparison.InvariantCulture))
		{
			return true;
		}
		return false;
	}

	public static XamlDomMember GetSuppressXamlTrimWarningsMember(XamlDomObject namedObject)
	{
		return namedObject.MemberNodes.Where((XamlDomMember x) => IsSuppressXamlTrimWarningsMember(x)).FirstOrDefault();
	}

	public static bool IsSuppressXamlTrimWarningsMember(XamlDomMember domMember)
	{
		return IsNamedDirective(domMember, "SuppressXamlTrimWarnings");
	}

	public static bool IsDataTypeMember(XamlDomMember domMember, bool checkForDirectiveOnly = false)
	{
		XamlMember member = domMember.Member;
		return IsDataTypeMember(member, checkForDirectiveOnly);
	}

	public static XamlDomMember GetDataTypeMember(XamlDomObject domObject, bool getDirectiveOnly = false)
	{
		foreach (XamlDomMember memberNode in domObject.MemberNodes)
		{
			if (IsDataTypeMember(memberNode, getDirectiveOnly))
			{
				return memberNode;
			}
		}
		return null;
	}

	public static string GetStaticResource_ResourceKey(XamlDomObject domStaticResourceObject)
	{
		string result = null;
		if (domStaticResourceObject.SchemaContext is DirectUISchemaContext directUISchemaContext && domStaticResourceObject.Type.CanAssignTo(directUISchemaContext.DirectUIXamlLanguage.StaticResourceExtension))
		{
			XamlDomMember memberNode = domStaticResourceObject.GetMemberNode("ResourceKey");
			if (memberNode != null)
			{
				result = GetStringValueOfProperty(memberNode);
			}
			else
			{
				XamlDomMember memberNode2 = domStaticResourceObject.GetMemberNode(XamlLanguage.PositionalParameters);
				if (memberNode2 != null && memberNode2.Items.Count == 1 && memberNode2.Items[0] != null)
				{
					result = ((memberNode2.Items[0] is XamlDomValue xamlDomValue) ? (xamlDomValue.Value as string) : null);
				}
			}
		}
		return result;
	}

	public static bool IsPhaseMember(XamlMember xamlMember)
	{
		if (xamlMember.IsDirective)
		{
			XamlDirective xamlDirective = xamlMember as XamlDirective;
			if (xamlDirective.Name.Equals("Phase", StringComparison.InvariantCulture))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPhaseMember(XamlDomMember domMember)
	{
		XamlMember member = domMember.Member;
		return IsPhaseMember(member);
	}

	public static bool IsBindExtension(XamlDomMember domMember)
	{
		if (domMember.Item is XamlDomObject domObject)
		{
			return IsBindExtension(domObject);
		}
		return false;
	}

	public static bool IsBindExtension(XamlDomObject domObject)
	{
		if (domObject.SchemaContext is DirectUISchemaContext directUISchemaContext)
		{
			return domObject.Type == directUISchemaContext.DirectUIXamlLanguage.BindExtension;
		}
		return false;
	}

	public static bool IsDependencyProperty(XamlDomMember domMember)
	{
		DirectUIXamlMember directUIXamlMember = domMember.Member as DirectUIXamlMember;
		if (directUIXamlMember != null)
		{
			if (!directUIXamlMember.IsDependencyProperty)
			{
				return directUIXamlMember.IsAttachable;
			}
			return true;
		}
		return false;
	}

	public static XamlDomObject GetBindExtensionOrNull(XamlDomMember domMember)
	{
		XamlDomObject xamlDomObject = domMember.Item as XamlDomObject;
		DirectUISchemaContext directUISchemaContext = domMember.Parent.SchemaContext as DirectUISchemaContext;
		if (xamlDomObject != null && directUISchemaContext != null && xamlDomObject.Type == directUISchemaContext.DirectUIXamlLanguage.BindExtension)
		{
			return xamlDomObject;
		}
		return null;
	}

	public static bool HasTwoWayBinding(XamlDomMember domMember)
	{
		XamlDomObject bindExtensionOrNull = GetBindExtensionOrNull(domMember);
		if (bindExtensionOrNull != null)
		{
			string stringValueOfProperty = GetStringValueOfProperty(bindExtensionOrNull.GetMemberNode("Mode"));
			if (stringValueOfProperty != null)
			{
				return stringValueOfProperty == "TwoWay";
			}
			return false;
		}
		return false;
	}

	public static bool HasTargetNullValue(XamlDomMember domMember)
	{
		XamlDomObject bindExtensionOrNull = GetBindExtensionOrNull(domMember);
		if (bindExtensionOrNull != null)
		{
			return bindExtensionOrNull.GetMemberNode("TargetNullValue") != null;
		}
		return false;
	}

	public static bool HasUpdateSourceTrigger(XamlDomMember domMember)
	{
		XamlDomObject bindExtensionOrNull = GetBindExtensionOrNull(domMember);
		if (bindExtensionOrNull != null)
		{
			return bindExtensionOrNull.GetMemberNode("UpdateSourceTrigger") != null;
		}
		return false;
	}

	public static bool DoesAnyMemberUseBindExpression(XamlDomObject domObject)
	{
		foreach (XamlDomMember memberNode in domObject.MemberNodes)
		{
			if (IsBindExtension(memberNode))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanBeInstantiatedLater(XamlDomObject namedObject)
	{
		if (HasLoadOrDeferLoadStrategyMember(namedObject))
		{
			return true;
		}
		foreach (XamlDomNode item in DomNodeAncestry(namedObject))
		{
			if (item is XamlDomObject namedObject2 && HasLoadOrDeferLoadStrategyMember(namedObject2))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasLoadOrDeferLoadStrategyMember(XamlDomObject namedObject)
	{
		return namedObject.MemberNodes.Where((XamlDomMember x) => IsDeferLoadStrategyMember(x) || IsLoadMember(x)).Any();
	}

	public static bool HasDefaultBindModeMember(XamlDomObject namedObject)
	{
		return namedObject.MemberNodes.Where((XamlDomMember x) => IsDefaultBindModeMember(x)).Any();
	}

	public static bool ConditionalValidForPlatform(Platform platCond, Platform targPlat)
	{
		switch (targPlat)
		{
		case Platform.Any:
			return platCond == Platform.Any;
		case Platform.Android:
			if (platCond != Platform.Any)
			{
				return platCond == Platform.Android;
			}
			return true;
		case Platform.iOS:
			if (platCond != Platform.Any)
			{
				return platCond == Platform.iOS;
			}
			return true;
		case Platform.UWP:
			if (platCond != Platform.Any)
			{
				return platCond == Platform.UWP;
			}
			return true;
		default:
			throw new Exception("Unknown target platform!");
		}
	}

	private static bool IsTypeInvalidForPlatform(XamlType type, Platform targPlat)
	{
		Platform platCond = Platform.Any;
		DirectUIXamlType directUIXamlType = type as DirectUIXamlType;
		if (directUIXamlType != null)
		{
			platCond = directUIXamlType.TargetPlatform;
		}
		else
		{
			string preferredXamlNamespace = type.PreferredXamlNamespace;
			if (preferredXamlNamespace.HasUsingPrefix())
			{
				string text = preferredXamlNamespace.StripUsingPrefix();
				if (text.IsConditionalNamespace())
				{
					try
					{
						platCond = ConditionalNamespace.Parse(text).PlatConditional;
					}
					catch (ParseException)
					{
					}
				}
			}
		}
		return !ConditionalValidForPlatform(platCond, targPlat);
	}

	public static bool IsObjectInvalidForPlatform(XamlDomObject obj, Platform targPlat)
	{
		return IsTypeInvalidForPlatform(obj.Type, targPlat);
	}

	public static bool IsMemberInvalidForPlatform(XamlDomMember member, Platform targPlat)
	{
		return IsTypeInvalidForPlatform(member.Member.Type, targPlat);
	}

	public static string GetDefaultBindMode(XamlDomObject namedObject)
	{
		string defaultBindModeForSingleton = GetDefaultBindModeForSingleton(namedObject);
		if (defaultBindModeForSingleton == null)
		{
			foreach (XamlDomNode item in DomNodeAncestry(namedObject))
			{
				if (item is XamlDomObject namedObject2)
				{
					defaultBindModeForSingleton = GetDefaultBindModeForSingleton(namedObject2);
					if (defaultBindModeForSingleton != null)
					{
						break;
					}
				}
			}
		}
		return defaultBindModeForSingleton;
	}

	public static XamlDomMember GetDefaultBindModeMember(XamlDomObject namedObject)
	{
		return namedObject.MemberNodes.Where((XamlDomMember x) => IsDefaultBindModeMember(x)).FirstOrDefault();
	}

	private static string GetDefaultBindModeForSingleton(XamlDomObject namedObject)
	{
		XamlDomMember defaultBindModeMember = GetDefaultBindModeMember(namedObject);
		string result = null;
		if (defaultBindModeMember != null)
		{
			result = GetStringValueOfProperty(defaultBindModeMember);
		}
		return result;
	}

	public static bool IsNamedCollectableObject(XamlDomObject domObject, bool isPass1)
	{
		return GetAliasedMemberNode(domObject, XamlLanguage.Name, forcePass1Eval: true) != null;
	}
}
