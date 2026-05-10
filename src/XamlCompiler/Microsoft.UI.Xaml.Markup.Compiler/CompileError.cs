using System;
using System.Xaml;
namespace Microsoft.UI.Xaml.Markup.Compiler;

using CodeGen;
using Properties;
using Utilities;
using XamlDom;

public enum ErrorCode
{
	WMC0001 = 1,
	WMC0003 = 3,
	WMC0005 = 5,
	WMC0010 = 10,
	WMC0011 = 11,
	WMC0015 = 15,
	WMC0020 = 20,
	WMC0021 = 21,
	WMC0025 = 25,
	WMC0026 = 26,
	WMC0027 = 27,
	WMC0030 = 30,
	WMC0035 = 35,
	WMC0040 = 40,
	WMC0045 = 45,
	WMC0046 = 46,
	WMC0047 = 47,
	WMC0050 = 50,
	WMC0055 = 55,
	WMC0056 = 56,
	WMC0060 = 60,
	WMC0065 = 65,
	WMC0070 = 70,
	WMC0075 = 75,
	WMC0080 = 80,
	WMC0085 = 85,
	WMC0086 = 86,
	WMC0090 = 90,
	WMC0091 = 91,
	WMC0095 = 95,
	WMC0100 = 100,
	WMC0105 = 105,
	WMC0110 = 110,
	WMC0115 = 115,
	WMC0120 = 120,
	WMC0121 = 121,
	WMC0125 = 125,
	WMC0130 = 130,
	WMC0131 = 131,
	WMC0132 = 132,
	WMC0140 = 140,
	WMC0141 = 141,
	WMC0142 = 142,
	WMC0145 = 145,
	WMC0150 = 150,
	WMC0153 = 153,
	WMC0154 = 154,
	WMC0155 = 155,
	WMC0500 = 500,
	WMC0501 = 501,
	WMC0502 = 502,
	WMC0503 = 503,
	WMC0504 = 504,
	WMC0505 = 505,
	WMC0600 = 600,
	WMC0601 = 601,
	WMC0605 = 605,
	WMC0610 = 610,
	WMC0612 = 612,
	WMC0615 = 615,
	WMC0620 = 620,
	WMC0621 = 621,
	WMC0800 = 800,
	WMC0805 = 805,
	WMC0806 = 806,
	WMC0810 = 810,
	WMC0815 = 815,
	WMC0820 = 820,
	WMC0821 = 821,
	WMC0822 = 822,
	WMC0901 = 901,
	WMC0902 = 902,
	WMC0903 = 903,
	WMC0905 = 905,
	WMC0906 = 906,
	WMC0907 = 907,
	WMC0908 = 908,
	WMC0909 = 909,
	WMC0910 = 910,
	WMC0911 = 911,
	WMC0912 = 912,
	WMC0913 = 913,
	WMC0914 = 914,
	WMC0915 = 915,
	WMC0916 = 916,
	WMC0917 = 917,
	WMC0918 = 918,
	WMC0919 = 919,
	WMC0920 = 920,
	WMC1002 = 1002,
	WMC1003 = 1003,
	WMC1005 = 1005,
	WMC1006 = 1006,
	WMC1007 = 1007,
	WMC1008 = 1008,
	WMC1009 = 1009,
	WMC1010 = 1010,
	WMC1011 = 1011,
	WMC1012 = 1012,
	WMC1013 = 1013,
	WMC1110 = 1110,
	WMC1111 = 1111,
	WMC1112 = 1112,
	WMC1113 = 1113,
	WMC1114 = 1114,
	WMC1115 = 1115,
	WMC1116 = 1116,
	WMC1117 = 1117,
	WMC1118 = 1118,
	WMC1119 = 1119,
	WMC1120 = 1120,
	WMC1121 = 1121,
	WMC1122 = 1122,
	WMC1123 = 1123,
	WMC1124 = 1124,
	WMC1125 = 1125,
	WMC0151 = 151,
	WMC0152 = 152,
	WMC1001 = 1001,
	WMC1004 = 1004,
	WMC1014 = 1014,
	WMC1500 = 1500,
	WMC1501 = 1501,
	WMC1502 = 1502,
	WMC1503 = 1503,
	WMC1504 = 1504,
	WMC1505 = 1505,
	WMC1506 = 1506,
	WMC1507 = 1507,
	WMC1508 = 1508,
	WMC1509 = 1509,
	WMC1510 = 1510,
	WMC9997 = 9997,
	WMC9998 = 9998,
	WMC9999 = 9999
}

public static class ErrorCodeExtension
{
	public static string AsErrorCode(this ErrorCode code)
	{
		return ((int)code).AsErrorCode();
	}

	public static string AsErrorCode(this int code)
	{
		return "WMC" + code.ToString("D4");
	}
}

public class XamlCompileErrorBase(ErrorCode code, string fileName, int lineNumber, int lineOffset)
{
	public ErrorCode Code { get; protected set; } = code;

	public string Message { get; protected set; }

	public string FileName { get; protected set; } = fileName;

	public int LineNumber { get; private set; } = lineNumber;

	public int LineOffset { get; private set; } = lineOffset;
}

public class XamlCompileError : XamlCompileErrorBase
{
	protected XamlCompileError(ErrorCode code)
		: base(code, null, 0, 0)
	{
	}

	public XamlCompileError(ErrorCode code, IXamlDomNode domNode)
		: base(code, domNode?.SourceFilePath ?? null, domNode?.StartLineNumber ?? 0, domNode?.StartLinePosition ?? 0)
	{
	}

	public XamlCompileError(ErrorCode code, int lineNumber, int lineOffset)
		: base(code, null, lineNumber, lineOffset)
	{
	}

	protected XamlCompileError(ErrorCode code, string fileName, int lineNumber, int lineOffset)
		: base(code, fileName, lineNumber, lineOffset)
	{
	}
}

public class XamlCompileWarning : XamlCompileErrorBase
{
	public XamlCompileWarning(ErrorCode code, IXamlDomNode domNode)
		: base(code, domNode.SourceFilePath, domNode.StartLineNumber, domNode.StartLinePosition)
	{
	}

	protected XamlCompileWarning(ErrorCode code)
		: base(code, null, 0, 0)
	{
	}
}

internal class XamlValidationErrorUnknownObject : XamlCompileError
{
	public XamlValidationErrorUnknownObject(XamlDomObject domObject)
		: base(ErrorCode.WMC0001, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownObject, domObject.Type.Name, domObject.Type.PreferredXamlNamespace);
	}
}

internal class XamlValidationErrorUnresolvedForwardedTypeAssembly : XamlCompileError
{
	public XamlValidationErrorUnresolvedForwardedTypeAssembly(XamlDomMember domMember, string errorMessage)
		: base(ErrorCode.WMC0003, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnresolvedForwardedTypeAssembly, errorMessage);
	}

	public XamlValidationErrorUnresolvedForwardedTypeAssembly(XamlDomObject domObject, string errorMessage)
		: base(ErrorCode.WMC0003, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnresolvedForwardedTypeAssembly, errorMessage);
	}
}

internal class XamlValidationErrorNonPublicType : XamlCompileError
{
	public XamlValidationErrorNonPublicType(XamlDomObject domObject)
		: base(ErrorCode.WMC0005, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAccessNonPublicType, domObject.Type.Name, domObject.Type.PreferredXamlNamespace);
	}
}

internal class XamlValidationErrorUnknownMember : XamlCompileError
{
	public XamlValidationErrorUnknownMember(XamlDomObject domObject, XamlDomMember domMember)
		: base(ErrorCode.WMC0010, domMember)
	{
		XamlMember member = domMember.Member;
		XamlType type = domMember.Parent.Type;
		XamlType declaringType = member.DeclaringType;
		if (declaringType != null && member.IsAttachable)
		{
			Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownAttachableMember, declaringType.Name, member.Name, type.Name);
		}
		else
		{
			Code = ErrorCode.WMC0011;
			Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownMember, member.Name, type.Name);
		}
	}
}

internal class XamlValidationErrorAssignment : XamlCompileError
{
	public XamlValidationErrorAssignment(XamlDomObject domChildObject, XamlMember property, XamlType propertyItemType)
		: base(ErrorCode.WMC0015, domChildObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAssign, domChildObject.Type.Name, property.Name, propertyItemType.Name);
	}
}

internal class XamlValidationErrorCollectionAdd : XamlCompileError
{
	public XamlValidationErrorCollectionAdd(XamlDomItem domChildItem, XamlType itemType, XamlDomObject collectionObject, XamlDomMember collectionMember)
		: base(ErrorCode.WMC0020, domChildItem)
	{
		XamlDomValue xamlDomValue = domChildItem as XamlDomValue;
		XamlDomObject xamlDomObject = domChildItem as XamlDomObject;
		string text = (xamlDomValue != null) ? (xamlDomValue.Value as string) : xamlDomObject.Type.Name;
		if (collectionObject.IsGetObject)
		{
			Code = ErrorCode.WMC0020;
			Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAddToCollectionProperty, text, collectionMember.Member.Name, itemType.Name);
		}
		else
		{
			Code = ErrorCode.WMC0021;
			Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAddToCollectionObject, text, collectionObject.Type.Name, itemType.Name);
		}
	}
}

internal class XamlValidationErrorDictionaryAdd : XamlCompileError
{
	public XamlValidationErrorDictionaryAdd(XamlDomValue domChildValue)
		: base(ErrorCode.WMC0025, domChildValue)
	{
		string text = domChildValue.Value as string;
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DictionaryItemsCannotBeText, text);
	}

	public XamlValidationErrorDictionaryAdd(XamlDomObject domChildObject, XamlType itemType, XamlDomObject domDictionaryObject, XamlDomMember domDictionaryProperty)
		: base(ErrorCode.WMC0026, domChildObject)
	{
		if (domDictionaryObject.IsGetObject)
		{
			Code = ErrorCode.WMC0026;
			Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAddToDictionaryProperty, domChildObject.Type.Name, domDictionaryProperty.Member.Name, itemType.Name);
		}
		else
		{
			Code = ErrorCode.WMC0027;
			Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAddToDictionaryObject, domChildObject.Type.Name, domDictionaryObject.Type.Name, itemType.Name);
		}
	}
}

internal class XamlValidationIdPropertiesMustBeText : XamlCompileError
{
	public XamlValidationIdPropertiesMustBeText(XamlDomMember domMember)
		: base(ErrorCode.WMC0030, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_IdPropertiesMustBeText, domMember.Member.Name);
	}
}

internal class XamlValidationErrorDuplicateAssigment : XamlCompileError
{
	public XamlValidationErrorDuplicateAssigment(XamlDomMember domMember)
		: base(ErrorCode.WMC0035, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DuplicationAssignment, domMember.Member.Name, domMember.Parent.Type.Name);
	}
}

internal class XamlValidationErrorBadName : XamlCompileError
{
	public XamlValidationErrorBadName(XamlDomMember domMember, string name)
		: base(ErrorCode.WMC0040, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_BadName, name, domMember.Member.Name, domMember.Parent.Type.Name);
	}

	public XamlValidationErrorBadName(XamlDomMember domMember, string name, char badChar)
		: base(ErrorCode.WMC0040, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_BadNameChar, name, domMember.Member.Name, domMember.Parent.Type.Name, badChar);
	}
}

internal class XamlValidationErrorCannotNameValueTypes : XamlCompileError
{
	public XamlValidationErrorCannotNameValueTypes(XamlDomObject domObject)
		: base(ErrorCode.WMC0045, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantNameValueTypes, domObject.Type.Name);
	}
}

internal class XamlValidationErrorCannotNameElementTwice : XamlCompileError
{
	public XamlValidationErrorCannotNameElementTwice(XamlDomObject domObject)
		: base(ErrorCode.WMC0046, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CannotNameElementTwice);
	}
}

internal class XamlValidationErrorElementNameAlreadyUsed : XamlCompileError
{
	public XamlValidationErrorElementNameAlreadyUsed(XamlDomObject domObject, string duplicateName)
		: base(ErrorCode.WMC0047, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_ElementNameAlreadyUsed, duplicateName);
	}
}

internal class XamlValidationErrorCannotAssignToReadOnlyProperty : XamlCompileError
{
	public XamlValidationErrorCannotAssignToReadOnlyProperty(XamlDomMember domMember)
		: base(ErrorCode.WMC0050, domMember)
	{
		XamlDomValue xamlDomValue = domMember.Items[0] as XamlDomValue;
		XamlDomObject xamlDomObject = domMember.Items[0] as XamlDomObject;
		string text = (xamlDomValue != null) ? (xamlDomValue.Value as string) : xamlDomObject.Type.Name;
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAssignToReadOnlyProperty, text, domMember.Member.Name);
	}

	public XamlValidationErrorCannotAssignToReadOnlyProperty(XamlDomNode location, XamlMember member, string value)
		: base(ErrorCode.WMC0050, location)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAssignToReadOnlyProperty, value, member.Name);
	}
}

internal class XamlValidationErrorCannotAssignTextToProperty : XamlCompileError
{
	public XamlValidationErrorCannotAssignTextToProperty(XamlDomNode location, XamlMember member, string value)
		: base(ErrorCode.WMC0055, location)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAssignTextToProperty, value, member.Name, member.Type.Name);
	}
}

internal class XamlValidationErrorCannotAssignNullableProperty : XamlCompileError
{
	public XamlValidationErrorCannotAssignNullableProperty(XamlDomNode location, XamlMember member)
		: base(ErrorCode.WMC0056, location)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_NullablePropertyType, member.Name);
	}
}

internal class XamlValidationDictionaryKeyError : XamlCompileError
{
	public XamlValidationDictionaryKeyError(XamlDomObject domObject)
		: base(ErrorCode.WMC0060, domObject)
	{
		string name = domObject.Type.Name;
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DictionaryItemsMustHaveKeys, name);
	}

	public XamlValidationDictionaryKeyError(XamlDomObject domObject, string keyText)
		: base(ErrorCode.WMC0065, domObject)
	{
		string name = domObject.Type.Name;
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DictionaryItemsHasDuplicateKey, name, keyText);
	}
}

internal class XamlValidationErrorBadCPA : XamlCompileError
{
	public XamlValidationErrorBadCPA(XamlDomObject domObject, string cpaName)
		: base(ErrorCode.WMC0070, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidCPA, domObject.Type.Name, cpaName);
	}
}

internal class XamlValidationErrorMissingCPA : XamlCompileError
{
	public XamlValidationErrorMissingCPA(XamlDomObject domParentObject, XamlDomItem firstChild)
		: base(ErrorCode.WMC0075, firstChild)
	{
		XamlDomValue xamlDomValue = firstChild as XamlDomValue;
		XamlDomObject xamlDomObject = firstChild as XamlDomObject;
		string text = ((xamlDomValue != null) ? (xamlDomValue.Value as string) : xamlDomObject.Type.Name);
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_MissingCPA, domParentObject.Type.Name, text);
	}
}

internal class XamlValidationErrorStyleMustHaveTargetType : XamlCompileError
{
	public XamlValidationErrorStyleMustHaveTargetType(XamlDomNode styleOrTargetType)
		: base(ErrorCode.WMC0080, styleOrTargetType)
	{
		Message = XamlCompilerResources.XamlCompiler_StyleMustHaveTargetType;
	}
}

internal class XamlValidationErrorSetterMissingField : XamlCompileError
{
	public XamlValidationErrorSetterMissingField(XamlDomNode setterOrProperty, bool isProperty)
		: base(ErrorCode.WMC0085, setterOrProperty)
	{
		if (isProperty)
		{
			Code = ErrorCode.WMC0085;
			Message = XamlCompilerResources.XamlCompiler_SettersMustHaveProperty;
		}
		else
		{
			Code = ErrorCode.WMC0086;
			Message = XamlCompilerResources.XamlCompiler_SetterMustHaveValue;
		}
	}
}

internal class XamlValidationErrorSetterUnknownMember : XamlCompileError
{
	public XamlValidationErrorSetterUnknownMember(XamlDomMember domPropertyMember, XamlType xamlTargetType, string propertyName)
		: base(ErrorCode.WMC0090, domPropertyMember)
	{
		if (propertyName.IndexOf('.') == -1)
		{
			Code = ErrorCode.WMC0090;
			Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownMember, propertyName, xamlTargetType.Name);
		}
		else
		{
			Code = ErrorCode.WMC0091;
			Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownSetterAttachableMember, propertyName, xamlTargetType.Name);
		}
	}
}

internal class XamlValidationErrorSetterSetterPropertyMustBeDP : XamlCompileError
{
	public XamlValidationErrorSetterSetterPropertyMustBeDP(XamlDomMember domPropertyMember, string propertyName)
		: base(ErrorCode.WMC0095, domPropertyMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_SetterPropertyMustBeDP, propertyName);
	}
}

internal class XamlValidationErrorNotConstructibleObject : XamlCompileError
{
	public XamlValidationErrorNotConstructibleObject(XamlDomObject domObject, XamlType xamlType)
		: base(ErrorCode.WMC0100, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_NotConstructibleObj, xamlType.Name);
	}
}

internal class XamlCompilerTypeMustHaveANamespace : XamlCompileError
{
	public XamlCompilerTypeMustHaveANamespace(XamlDomObject domObject)
		: base(ErrorCode.WMC0105, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_TypeMustHaveANamespace, domObject.Type.Name);
	}
}

internal class XamlValidationErrorUnknownStyleTargetType : XamlCompileError
{
	public XamlValidationErrorUnknownStyleTargetType(XamlDomMember targetTypeMember, string typeName)
		: base(ErrorCode.WMC0110, targetTypeMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownStyleTargetType, typeName);
	}
}

internal class XamlCompilerErrorProcessingStyle : XamlCompileError
{
	public XamlCompilerErrorProcessingStyle(XamlDomObject domStyle)
		: base(ErrorCode.WMC0115, domStyle)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InternalErrorProcessingStyle);
	}
}

internal class XamlCompileErrorInvalidPropertyType : XamlCompileError
{
	public XamlCompileErrorInvalidPropertyType(XamlDomMember domMember)
		: base(ErrorCode.WMC0120, domMember)
	{
		string name = domMember.Member.Type.Name;
		string name2 = domMember.Member.Name;
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidPropertyType, name, name2);
	}
}

internal class XamlCompileErrorInvalidPropertyType_SignedChar : XamlCompileError
{
	public XamlCompileErrorInvalidPropertyType_SignedChar(XamlDomMember domMember)
		: base(ErrorCode.WMC0121, domMember)
	{
		string name = domMember.Member.Type.Name;
		string name2 = domMember.Member.Name;
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidSignedChar, name, name2);
	}
}

internal class XamlValidationErrorEventValuesMustBeText : XamlCompileError
{
	public XamlValidationErrorEventValuesMustBeText(XamlDomNode domNode, string eventName)
		: base(ErrorCode.WMC0125, domNode)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_EventValuesMustBeText, eventName);
	}
}

internal class XamlValidationErrorClassMustHaveANamespace : XamlCompileError
{
	public XamlValidationErrorClassMustHaveANamespace(XamlDomMember domMember, string classname)
		: base(ErrorCode.WMC0130, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_EventValuesMustBeText, classname);
	}
}

internal class XamlValidationErrorClassNameEmptyPathPart : XamlCompileError
{
	public XamlValidationErrorClassNameEmptyPathPart(XamlDomMember domMember, string classname)
		: base(ErrorCode.WMC0131, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.Harvester_ClassNameEmptyPathPart, classname);
	}
}

internal class XamlValidationErrorClassNameNoWhiteSpace : XamlCompileError
{
	public XamlValidationErrorClassNameNoWhiteSpace(XamlDomMember domMember, string classname)
		: base(ErrorCode.WMC0132, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.Harvester_ClassNameNoWhiteSpace, classname);
	}
}

internal class XamlValidationErrorStyleBasedOnMustBeStyle : XamlCompileError
{
	public XamlValidationErrorStyleBasedOnMustBeStyle(XamlDomObject styleObject, string keyString, XamlDomObject domBaseStyleObject, string otherFile)
		: base(ErrorCode.WMC0140, styleObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_StyleBasedOnMustBeStyle_SR, keyString, domBaseStyleObject.Type.Name);
	}

	public XamlValidationErrorStyleBasedOnMustBeStyle(XamlDomObject styleObject, XamlDomObject domNotStyleObject)
		: base(ErrorCode.WMC0141, styleObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_StyleBasedOnMustBeStyle_BadObj, domNotStyleObject.Type.Name);
	}

	public XamlValidationErrorStyleBasedOnMustBeStyle(XamlDomObject styleObject, string text)
		: base(ErrorCode.WMC0142, styleObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_StyleBasedOnMustBeStyle_Text, text);
	}
}

internal class XamlValidationErrorStyleBasedOnBadStyleTargetType : XamlCompileError
{
	public XamlValidationErrorStyleBasedOnBadStyleTargetType(XamlDomNode styleObject, XamlType targetType, XamlType basedOnTargetType)
		: base(ErrorCode.WMC0145, styleObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_StyleBasedOnBadStyleTargetType, targetType.Name, basedOnTargetType.Name);
	}
}

internal class XamlValidationErrorDeprecated : XamlCompileError
{
	public XamlValidationErrorDeprecated(XamlDomObject domObject, string name, string message)
		: base(ErrorCode.WMC0150, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Deprecated, name, message);
	}

	public XamlValidationErrorDeprecated(XamlDomMember domMember, string name, string message)
		: base(ErrorCode.WMC0150, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Deprecated, name, message);
	}
}

internal class XamlValidationErrorWrongContract : XamlCompileWarning
{
	public XamlValidationErrorWrongContract(XamlDomObject domObject, string typeName, string contractName, string runtimeVer, string parseVer)
		: base(ErrorCode.WMC0151, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_WrongTypeContract, typeName, contractName, runtimeVer, parseVer);
	}

	public XamlValidationErrorWrongContract(XamlDomMember domMember, string typeName, string contractName, string runtimeVer, string parseVer)
		: base(ErrorCode.WMC0151, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_WrongMemberContract, typeName, contractName, runtimeVer, parseVer, domMember.Member.Name);
	}
}

internal class XamlValidationErrorContractDoesNotExist : XamlCompileWarning
{
	public XamlValidationErrorContractDoesNotExist(XamlDomObject domObject, string typeName, string contractName, string runtimeVer)
		: base(ErrorCode.WMC0152, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_TypeContractDoesNotExist, typeName, contractName, runtimeVer);
	}

	public XamlValidationErrorContractDoesNotExist(XamlDomMember domMember, string typeName, string contractName, string runtimeVer)
		: base(ErrorCode.WMC0152, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_MemberContractDoesNotExist, typeName, contractName, runtimeVer, domMember.Member.Name);
	}
}

internal class XamlValidationErrorAmbiguousEvent : XamlCompileError
{
	public XamlValidationErrorAmbiguousEvent(XamlDomMember domMember)
		: base(ErrorCode.WMC0154, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlValidationError_AmbiguousEvent);
	}
}

internal class XamlSuccinctSyntaxError : XamlCompileError
{
	public XamlSuccinctSyntaxError(int line, int col, string offendingToken, string fileName)
		: base(ErrorCode.WMC0155, fileName, line, col)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlValidationError_SuccinctSyntaxError, line, col, offendingToken);
	}
}

internal class XamlRewriterErrorEventLongForm : XamlCompileError
{
	public XamlRewriterErrorEventLongForm(int line, int column)
		: base(ErrorCode.WMC0500, line, column)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlRewriter_EventsCannotBeInElementForm);
	}
}

internal class XamlRewriterErrorLineBreak : XamlCompileError
{
	public XamlRewriterErrorLineBreak(int line, int column)
		: base(ErrorCode.WMC0501, line, column)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlRewriter_EventsAcrossLine);
	}
}

internal class XamlRewriterErrorFileOpenFailure : XamlCompileError
{
	public XamlRewriterErrorFileOpenFailure(string xamlFileName, string message)
		: base(ErrorCode.WMC0502, xamlFileName, 0, 0)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_FileOpenFailure, message);
	}
}

internal class XamlRewriterErrorCompiledBindingLongForm : XamlCompileError
{
	public XamlRewriterErrorCompiledBindingLongForm(int line, int column)
		: base(ErrorCode.WMC0503, line, column)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlRewriter_CompiledBindingsCannotBeInElementForm);
	}
}

internal class XamlRewriterErrorDataTypeLongForm : XamlCompileError
{
	public XamlRewriterErrorDataTypeLongForm(int line, int column)
		: base(ErrorCode.WMC0504, line, column)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlRewriter_XamlRewriterErrorDataTypeLongForm);
	}
}

internal class ErrorXPropertyUsageNotSupported : XamlCompileError
{
	public ErrorXPropertyUsageNotSupported(XamlDomObject domObject, Language language)
		: base(ErrorCode.WMC0505, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XPropertyUsageNotSupportedForLanguage, language.Name);
	}
}

internal class XbfOutputFileOpenFailure : XamlCompileError
{
	public XbfOutputFileOpenFailure(string xbfFile, string message)
		: base(ErrorCode.WMC0600, xbfFile, 0, 0)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_XbfOutputFileOpenFailure, message);
	}
}

internal class XbfInputFileOpenFailure : XamlCompileError
{
	public XbfInputFileOpenFailure(string xbfFile, string message)
		: base(ErrorCode.WMC0601, xbfFile, 0, 0)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_XamlInputFileOpenFailure, message);
	}
}

internal class XbfGenerationGeneralFailure : XamlCompileError
{
	public XbfGenerationGeneralFailure(string message)
		: base(ErrorCode.WMC0605)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_GeneralFailure, message);
	}
}

internal class XbfGenerationParseError : XamlCompileError
{
	public XbfGenerationParseError(string fileName, int line, int column, int xbfErrorCode)
		: base(ErrorCode.WMC0610, fileName, line, column)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_SyntaxError, "0x" + xbfErrorCode.ToString("x4"));
	}
}

internal class XbfGenerationPropertyNotFoundError : XamlCompileError
{
	public XbfGenerationPropertyNotFoundError(string fileName, int line, int column)
		: base(ErrorCode.WMC0612, fileName, line, column)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_PropertyNotFoundError);
	}
}

internal class XbfGeneration_NonMeInCurlyBraces : XamlCompileError
{
	public XbfGeneration_NonMeInCurlyBraces(string fileName, int line, int column, string nonMeName, int xbfErrorCode)
		: base(ErrorCode.WMC0615, fileName, line, column)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_SyntaxErrorME, nonMeName, "0x" + xbfErrorCode.ToString("x4"));
	}
}

internal class XbfGeneration_NoWindowsSdk : XamlCompileError
{
	public XbfGeneration_NoWindowsSdk()
		: base(ErrorCode.WMC0620)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_MissingGenXbfPath);
	}
}

internal class XbfGeneration_CouldNotLoadXbfGenerator : XamlCompileError
{
	public XbfGeneration_CouldNotLoadXbfGenerator(string path)
		: base(ErrorCode.WMC0621, path, 0, 0)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_CouldNotLoadXbfGenerator, path);
	}
}

internal class XamlSchemaError_BadBindablePropertyProvider : XamlCompileError
{
	public XamlSchemaError_BadBindablePropertyProvider(string typeName)
		: base(ErrorCode.WMC0800)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_BadBindablePropertyProvider, typeName);
	}
}

internal class XamlSchemaError_TypeLoadException : XamlCompileError
{
	public XamlSchemaError_TypeLoadException(string typeName, string asmName)
		: base(ErrorCode.WMC0805)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_TypeLoadException, typeName, asmName);
	}

	public XamlSchemaError_TypeLoadException(XamlDomObject domObject, string typeName, string innerMessage)
		: base(ErrorCode.WMC0806, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_TypeLoadExceptionMessage, typeName, innerMessage);
	}
}

internal class XamlSchemaError_CustomAttributesTypeLoadException : XamlCompileError
{
	public XamlSchemaError_CustomAttributesTypeLoadException(string asmName)
		: base(ErrorCode.WMC0810)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_CustomAttributesTypeLoadException, asmName);
	}
}

internal class XamlSchemaError_WRTAssembliesMissing : XamlCompileError
{
	public XamlSchemaError_WRTAssembliesMissing()
		: base(ErrorCode.WMC0815)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_WRTAssembliesMissing);
	}
}

internal class XamlSchemaError_AmbiguousCollectionAdd : XamlCompileError
{
	public XamlSchemaError_AmbiguousCollectionAdd(string typeName, string methodName, int argumentCount)
		: base(ErrorCode.WMC0820)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_AmbiguousCollectionAdd, typeName, methodName, argumentCount);
	}
}

internal class XamlSchemaError_BindableNotSupportedOnGeneric : XamlCompileError
{
	public XamlSchemaError_BindableNotSupportedOnGeneric(string typeName)
		: base(ErrorCode.WMC0821)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_BindableNotSupportedOnGeneric, typeName);
	}
}

internal class XamlSchemaError_UnknownTypeError : XamlCompileError
{
	public XamlSchemaError_UnknownTypeError(string typeName)
		: base(ErrorCode.WMC0822)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownTypeError, typeName);
	}
}

internal class XamlErrorDuplicateType : XamlCompileError
{
	public XamlErrorDuplicateType(string fullName)
		: base(ErrorCode.WMC0901)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DuplicateTypeName, fullName);
	}
}

internal class XamlValidationErrorInvalidFieldModifier : XamlCompileError
{
	public XamlValidationErrorInvalidFieldModifier(XamlDomObject domObject, string invalidModifier)
		: base(ErrorCode.WMC0905, domObject)
	{
		XamlDomMember aliasedMemberNode = DomHelper.GetAliasedMemberNode(domObject, XamlLanguage.Name);
		if (aliasedMemberNode == null)
		{
			Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidFieldModifier, invalidModifier, domObject.Type.Name);
		}
		else
		{
			Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidFieldModifier, invalidModifier, DomHelper.GetStringValueOfProperty(aliasedMemberNode));
		}
	}
}

internal class XamlValidationError_DeferLoadStrategyInvalidValue : XamlCompileError
{
	public XamlValidationError_DeferLoadStrategyInvalidValue(XamlDomMember domMember)
		: base(ErrorCode.WMC0906, domMember)
	{
		XamlMember member = domMember.Member;
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlValidationError_DeferLoadStrategyInvalidValue, member.Name);
	}
}

internal class XamlValidationError_LoadInvalidValue : XamlCompileError
{
	public XamlValidationError_LoadInvalidValue(XamlDomMember domMember)
		: base(ErrorCode.WMC0906, domMember)
	{
		XamlMember member = domMember.Member;
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlValidationError_InvalidAttributeValue, member.Name);
	}
}

internal class XamlValidationError_DeferLoadStrategyMissingXName : XamlCompileError
{
	public XamlValidationError_DeferLoadStrategyMissingXName(XamlDomObject domObject)
		: base(ErrorCode.WMC0907, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DeferLoadStrategyMissingXName);
	}
}

internal class XamlValidationError_LoadMissingName : XamlCompileError
{
	public XamlValidationError_LoadMissingName(XamlDomObject domObject)
		: base(ErrorCode.WMC0907, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_LoadMissingName);
	}
}

internal class XamlValidationError_DataTypeOnlyAllowedOnDataTemplate : XamlCompileError
{
	public XamlValidationError_DataTypeOnlyAllowedOnDataTemplate(XamlDomObject domObject)
		: base(ErrorCode.WMC0908, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlValidationError_DataTypeOnlyAllowedOnDataTemplate);
	}
}

internal class XamlValidationError_CantResolveDataType : XamlCompileError
{
	public XamlValidationError_CantResolveDataType(XamlDomObject domObject, string dataTypeName)
		: base(ErrorCode.WMC0909, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantResolveDataType, dataTypeName);
	}
}

internal class XamlValidationError_InvalidValueForPhase : XamlCompileError
{
	public XamlValidationError_InvalidValueForPhase(XamlDomObject domObject)
		: base(ErrorCode.WMC0910, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidValueForPhase);
	}
}

internal class XamlValidationError_PhaseCanBeUsedOnlyWithBind : XamlCompileError
{
	public XamlValidationError_PhaseCanBeUsedOnlyWithBind(XamlDomObject domObject)
		: base(ErrorCode.WMC0911, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_PhaseMustHaveAssociatedBind);
	}
}

internal class XamlValidationError_PhaseOnlyAllowedInDataTemplate : XamlCompileError
{
	public XamlValidationError_PhaseOnlyAllowedInDataTemplate(XamlDomObject domObject)
		: base(ErrorCode.WMC0912, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_PhaseMustBeUsedWithinADataTemplate);
	}
}

internal class XamlValidationError_CannotHaveDeferLoadStrategy : XamlCompileError
{
	public XamlValidationError_CannotHaveDeferLoadStrategy(XamlDomMember domMember)
		: base(ErrorCode.WMC0913, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CannotHaveDeferLoadStrategy);
	}
}

internal class XamlValidationError_LoadNotSupported : XamlCompileError
{
	public XamlValidationError_LoadNotSupported(XamlDomMember domMember)
		: base(ErrorCode.WMC0913, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_LoadNotSupported);
	}
}

internal class XamlValidationError_LoadConflict : XamlCompileError
{
	public XamlValidationError_LoadConflict(XamlDomMember domMember)
		: base(ErrorCode.WMC0914, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_LoadConflict);
	}
}

internal class XamlValidationCreateFromStringError : XamlCompileError
{
	public XamlValidationCreateFromStringError(string typeName, string createFromStringMethodName, string message, XamlDomNode locationForErrors)
		: base(ErrorCode.WMC0915, locationForErrors)
	{
		Message = string.Format(message, createFromStringMethodName, typeName);
	}
}

internal class XamlValidationConditionalNamespaceError : XamlCompileError
{
	public XamlValidationConditionalNamespaceError(string expressionBeingParsed, string message, XamlDomNode domNode)
		: base(ErrorCode.WMC0916, domNode)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.ConditionalNamespace_FailedToParse, expressionBeingParsed, message);
	}
}

internal class XamlValidationError_DefaultBindModeInvalidValue : XamlCompileError
{
	public XamlValidationError_DefaultBindModeInvalidValue(XamlDomMember domMember)
		: base(ErrorCode.WMC0917, domMember)
	{
		XamlMember member = domMember.Member;
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlValidationError_DefaultBindModeInvalidValue, member.Name);
	}
}

internal class XamlValidationPlatformConditionalStrict : XamlCompileError
{
	public XamlValidationPlatformConditionalStrict(XamlDomNode domNode)
		: base(ErrorCode.WMC0918, domNode)
	{
		Message = XamlCompilerResources.ConditionalNamespace_ConditionalInStandard;
	}
}

internal class XamlValidationError_InvalidValueForSuppressXamlTrimWarnings : XamlCompileError
{
	public XamlValidationError_InvalidValueForSuppressXamlTrimWarnings(XamlDomObject domObject)
		: base(ErrorCode.WMC0920, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidValueForSuppressXamlTrimWarnings);
	}
}

internal class XamlValidationWarningNoXaml : XamlCompileWarning
{
	public XamlValidationWarningNoXaml()
		: base(ErrorCode.WMC1001)
	{
		Message = XamlCompilerResources.XamlCompiler_NoXamlGiven;
	}
}

internal class XamlValidationWarningUsingCodeGenFlags : XamlCompileWarning
{
	public XamlValidationWarningUsingCodeGenFlags(CodeGenCtrlFlags flags)
		: base(ErrorCode.WMC1004)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CodeGenString_Using, flags.ToString());
	}
}

internal class XamlTypeInfoReflectionTypeNamingConventionViolation : XamlCompileWarning
{
	public XamlTypeInfoReflectionTypeNamingConventionViolation(string typeName, string asmName)
		: base(ErrorCode.WMC1005)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.TypeInfoReflection_TypeViolatesNamingConvention, typeName, asmName);
	}
}

internal class XamlFileMustEndInDotXaml : XamlCompileError
{
	public XamlFileMustEndInDotXaml(string fileName)
		: base(ErrorCode.WMC1010, fileName, 0, 0)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XamlFileMustEndInDotXaml);
	}
}

internal class XamlXBindParseError : XamlCompileError
{
	public XamlXBindParseError(IXamlDomNode node, CompiledBindingParseException ex)
		: base(ErrorCode.WMC1110, node.SourceFilePath, node.StartLineNumber, ex.StartCharacterPosition)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_XamlXBindParseError, ex.ExpressionBeingParsed, ex.Message);
	}

	public XamlXBindParseError(BindAssignmentBase bindAssignment, CompiledBindingParseException ex)
		: this(bindAssignment, ex.StartCharacterPosition, ex.ExpressionBeingParsed, ex.Message)
	{
	}

	public XamlXBindParseError(BindAssignmentBase bindAssignment, int startCharacterPosition, string expressionBeingParsed, string exceptionMessage)
		: base(ErrorCode.WMC1110, bindAssignment.ConnectionIdElement.ParentFileCodeInfo.FullPathToXamlFile, bindAssignment.LineNumberInfo.StartLineNumber, startCharacterPosition)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_XamlXBindParseError, expressionBeingParsed, exceptionMessage);
	}
}

internal class XamlXBindDataTemplateDoesNotDefineDataTypeError : XamlCompileError
{
	public XamlXBindDataTemplateDoesNotDefineDataTypeError(IXamlDomNode node)
		: base(ErrorCode.WMC1111, node)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.Harvester_DataTemplateDoesNotDefineDataType);
	}
}

internal class XamlXBindControlTemplateDoesNotDefineTargetTypeError : XamlCompileError
{
	public XamlXBindControlTemplateDoesNotDefineTargetTypeError(IXamlDomNode node)
		: base(ErrorCode.WMC1111, node)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.Harvester_ControlTemplateDoesNotDefineTargetType);
	}
}

internal class XamlXBindUsedInStyleError : XamlCompileError
{
	public XamlXBindUsedInStyleError(IXamlDomNode node)
		: base(ErrorCode.WMC1112, node)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_XamlXBindUsedInStyleError);
	}
}

internal class XamlValidationWarningUnsupportedCodeGenFlags : XamlCompileWarning
{
	public XamlValidationWarningUnsupportedCodeGenFlags(CodeGenCtrlFlags flags)
		: base(ErrorCode.WMC1014)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CodeGenString_NotSupported, flags.ToString());
	}
}

internal class XamlXBindTwoWayBindingToANonDependencyPropertyError : XamlCompileError
{
	public XamlXBindTwoWayBindingToANonDependencyPropertyError(XamlDomMember domMember)
		: base(ErrorCode.WMC1118, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_TwoWayTargetNotADependencyProperty, domMember.Member.Name);
	}
}

internal class XamlXBindWithoutCodeBehindError : XamlCompileError
{
	public XamlXBindWithoutCodeBehindError(XamlDomMember domMember)
		: base(ErrorCode.WMC1119, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindWithoutCodeBehind);
	}
}

internal class XamlXBindTargetNullValueOnNonNullableTypeError : XamlCompileError
{
	public XamlXBindTargetNullValueOnNonNullableTypeError(XamlDomMember domMember)
		: base(ErrorCode.WMC1120, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindTargetNullValueOnNonNullableType, domMember.Member.Name, domMember.Member.Type.Name);
	}
}

internal class BindAssignmentValidationError : XamlCompileError
{
	public BindAssignmentValidationError(IXamlDomNode node, string message)
		: base(ErrorCode.WMC1121, node)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_XamlXBindAssignmentValidationError, message);
	}
}

internal class XamlXBindInsideXBindError : XamlCompileError
{
	public XamlXBindInsideXBindError(XamlDomMember domMember)
		: base(ErrorCode.WMC1122, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindInsideXBind, domMember.Member.Name);
	}
}

internal class XamlXBindOnControlTemplateError : XamlCompileError
{
	public XamlXBindOnControlTemplateError(XamlDomMember domMember)
		: base(ErrorCode.WMC1123, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindOnControlTemplate);
	}
}

internal class XamlXBindOutOfScopeUnsupported : XamlCompileError
{
	public XamlXBindOutOfScopeUnsupported(BindAssignment ba, string elementName, int namedElementLineNumber)
		: base(ErrorCode.WMC1124, ba.ConnectionIdElement.ParentFileCodeInfo.FullPathToXamlFile, ba.LineNumberInfo.StartLineNumber, ba.LineNumberInfo.StartLinePosition)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindOutOfScopeUnsupported, elementName, namedElementLineNumber);
	}
}

internal class XamlXBindRootNoLoadingEvent : XamlCompileError
{
	public XamlXBindRootNoLoadingEvent(XamlDomMember domMember, string rootElementType)
		: base(ErrorCode.WMC1125, domMember)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindRootMustHaveLoading, rootElementType);
	}
}

internal class XamlValidationWarningDeprecated : XamlCompileWarning
{
	public XamlValidationWarningDeprecated(IXamlDomNode domObject, string name, string message)
		: base(ErrorCode.WMC1500, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Deprecated, name, message);
	}
}

internal class XamlXBindParseWarning : XamlCompileWarning
{
	public XamlXBindParseWarning(XamlDomObject domObject, string message)
		: base(ErrorCode.WMC1507, domObject)
	{
		Message = message;
	}
}

internal class XamlXClassDerivedFromXClassWarning : XamlCompileWarning
{
	public XamlXClassDerivedFromXClassWarning(XamlDomObject domObject, string derivedClass, string baseClass)
		: base(ErrorCode.WMC1508, domObject)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XClassDerivesFromXClass, derivedClass, baseClass);
	}
}

internal class XamlLocalAssemblyNotFound : XamlCompileWarning
{
	public XamlLocalAssemblyNotFound()
		: base(ErrorCode.WMC1509)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_LocalAssemblyMissingWarning);
	}
}

internal class XamlBindingAotCompatibilityWarning : XamlCompileWarning
{
	public XamlBindingAotCompatibilityWarning(IXamlDomNode node)
		: base(ErrorCode.WMC1510, node)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_BindingAotCompatibility);
	}
}

internal class BindAssignmentValidationWarning : XamlCompileWarning
{
	public BindAssignmentValidationWarning(IXamlDomNode node, ErrorCode errorCode, string message)
		: base(errorCode, node)
	{
		Message = message;
	}
}

internal class XamlValidationWarningExperimental : XamlCompileWarning
{
	public XamlValidationWarningExperimental(ErrorCode warningCode, string name)
		: base(warningCode)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Experimental, name);
	}

	public XamlValidationWarningExperimental(ErrorCode warningCode, IXamlDomNode domNode, string name)
		: base(warningCode, domNode)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Experimental, name);
	}
}

internal class XamlValidationWarningPreview : XamlCompileWarning
{
	public XamlValidationWarningPreview(ErrorCode warningCode, string name)
		: base(warningCode)
	{
		Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Preview, name);
	}
}
