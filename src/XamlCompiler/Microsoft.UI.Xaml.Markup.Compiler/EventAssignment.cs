using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class EventAssignment : ILineNumberAndErrorInfo
{
	private IEnumerable<Parameter> parameters;

	public ApiInformation ApiInformation { get; }

	public TypeForCodeGen EventType { get; set; }

	public TypeForCodeGen DeclaringType { get; set; }

	public string HandlerName { get; set; }

	public string EventParamsForCppSignature { get; set; }

	public DirectUIXamlMember Event { get; set; }

	public LineNumberInfo LineNumberInfo { get; set; }

	public string EventName { get; set; }

	public IEnumerable<Parameter> Parameters
	{
		get
		{
			if (parameters == null)
			{
				parameters = EventType.UnderlyingType.TryGetInvokeParameters();
			}
			return parameters;
		}
	}

	public EventAssignment(XamlDomMember domMember)
	{
		ApiInformation = domMember.ApiInformation ?? domMember.Parent.ApiInformation;
		LineNumberInfo = new LineNumberInfo(domMember);
		EventName = domMember.Member.Name;
		HandlerName = (domMember.Item as XamlDomValue).Value as string;
		Event = domMember.Member as DirectUIXamlMember;
		EventParamsForCppSignature = LookupEventParamsForCppSignature();
		EventType = new TypeForCodeGen(domMember.Member.Type);
		DeclaringType = new TypeForCodeGen(domMember.Member.DeclaringType);
	}

	internal string LookupEventParamsForCppSignature()
	{
		Type underlyingType = Event.Type.UnderlyingType;
		if (underlyingType.BaseType.FullName != typeof(MulticastDelegate).FullName)
		{
			return null;
		}
		MethodInfo method = underlyingType.GetMethod("Invoke");
		if (method == null)
		{
			return null;
		}
		ParameterInfo[] array = method.GetParameters();
		if (array == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder("(");
		bool flag = true;
		ParameterInfo[] array2 = array;
		foreach (ParameterInfo parameterInfo in array2)
		{
			string fullName = null;
			Type underlyingType2 = Event.DeclaringType.SchemaContext.GetXamlType(parameterInfo.ParameterType).UnderlyingType;
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			else
			{
				flag = false;
			}
			if (XamlSchemaCodeInfo.GetGlobalizedFullNameForCppRefType(underlyingType2, out fullName))
			{
				fullName += "^*";
				stringBuilder.Append(fullName);
				continue;
			}
			fullName = XamlSchemaCodeInfo.GetFullGenericNestedName(underlyingType2, "C++", globalized: true);
			if (underlyingType2.IsArray)
			{
				if (parameterInfo.IsIn)
				{
					fullName = "const ::Platform::Array<" + fullName.Substring(0, fullName.Length - 2) + ">";
				}
				else if (parameterInfo.IsOut)
				{
					fullName = "::Platform::WriteOnlyArray<" + fullName.Substring(0, fullName.Length - 2) + ">";
				}
			}
			stringBuilder.Append(fullName);
			if (!underlyingType2.IsValueType)
			{
				stringBuilder.Append("^");
			}
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	public XamlCompileError GetAttributeProcessingError()
	{
		return new XamlRewriterErrorEventLongForm(LineNumberInfo.StartLineNumber, LineNumberInfo.StartLinePosition);
	}
}
